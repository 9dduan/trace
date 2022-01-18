using System.Collections;
using System.Collections.Generic;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Index.KdTree;
using UnityEngine;
using MathNet;
using System;
using System.Linq;
using MathNet.Numerics;

namespace tracing.StreamLineVis
{
    public class RoadMapGenerator : MonoBehaviour
    {
        private SeedPoint m_startingSeedPoint;
        private AddedField m_combinedFields;
        private PrioQueue.PriorityQueue<SeedPoint> m_candidatesQueue;
        private KdTree<SeedPoint> m_placedPoints;
        private List<List<LineString>> m_generatedLines;
        private List<LineString> m_currentTracingLine;
        private EigenType m_currentTracingEigenType;
        private double m_currentTracingLength; // if current
        private double m_stepLength;
        private double m_maxSingleTracingLength;
        private LinearRing m_boundary;
        private int m_maxTracedLines;
        private float m_snapThreshold;

        public RoadMapGenerator(AddedField combinedFields, SeedPoint startPoint, LinearRing boundary, double stepLength, double maxLength,int maxTracedLines=20)
        {
            m_combinedFields = combinedFields;
            m_startingSeedPoint = startPoint;
            m_stepLength = stepLength;
            m_maxSingleTracingLength = maxLength;
            m_candidatesQueue = new PrioQueue.PriorityQueue<SeedPoint>(1000, new SeedComparor());
            m_placedPoints = new KdTree<SeedPoint>();
            m_generatedLines = new List<List<LineString>>();
            m_maxTracedLines = maxTracedLines;
            m_boundary = boundary;
            m_snapThreshold = 2f;
        }

        public void Draw()
        {
            List<Color> colors = new List<Color>() { Color.magenta, Color.green, Color.cyan };

            try
            {
                int i = 0;
                foreach (var l in m_generatedLines)
                {
                    if (i > 2) i = 0;
                    var currColor = colors[i];
                    i += 1;

                    foreach (var seg in l)
                    {
                        Utils.DrawLineString(seg, currColor);
                    }
                }
                
            }
            catch
            {
                Debug.Log("drawing error");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Iterate()
        {
            // set starting conditions
            m_candidatesQueue.push(m_startingSeedPoint);
            m_currentTracingEigenType = EigenType.Major;
            m_placedPoints.Insert(m_startingSeedPoint.Pos, m_startingSeedPoint);
            int iterativeCount = 0;

            while (m_candidatesQueue.Count > 0 && iterativeCount <m_maxTracedLines)
            {
                iterativeCount += 1;
                SeedPoint currPt = m_candidatesQueue.pop();
                Coordinate currDir = GetDirFromPosAndEigenType(currPt.Pos, m_currentTracingEigenType);
                m_currentTracingLine = new List<LineString>();
                m_currentTracingLength = 0f;
                Debug.Log($"Start new line from point {currPt.Pos.X}-{currPt.Pos.Y} direction {currDir.X}-{currDir.Y}");

                int count = 0;
                while (count<100)
                {
                    // trace current single streamline
                    count += 1;
                    var nextPt = GetNextPoint(currPt, currDir);
                    var nextDir = GetNextDir(nextPt, currDir);
                    LineString currSeg = new LineString(new Coordinate[] { currPt.Pos, nextPt.Pos });
                    var modifiedSeg = ModifySegment(currSeg);

                    if (modifiedSeg != null)
                    {
                        nextPt = new SeedPoint(modifiedSeg[1]);
                        nextDir = GetNextDir(nextPt, currDir);

                        m_currentTracingLength += modifiedSeg.Length;
                        m_currentTracingLine.Add(modifiedSeg);
                        m_placedPoints.Insert(nextPt.Pos, nextPt);

                        var newCandidates = GetCandiatePoints(nextPt);

                        if (newCandidates.Any())
                        {
                            foreach(var c in newCandidates)
                            {
                                m_candidatesQueue.push(c);
                                Debug.Log($"pushing seed points in pq {c.ToString()}");
                            }
                        }

                        // update currPt and currDir
                        currDir = nextDir;
                        currPt = nextPt;
                    }
                    else
                    {
                        break;
                    }
                }

                m_generatedLines.Add(m_currentTracingLine);

                // switch tracing Eigen
                if (m_currentTracingEigenType == EigenType.Major)
                {
                    m_currentTracingEigenType = EigenType.Minor;
                }
                else
                {
                    m_currentTracingEigenType = EigenType.Major;
                }
            }

            Debug.Log($"{m_generatedLines.Count()} pieces of stream lines created...");
        }

        // TODO: add more validation method
        public bool IsCurrentTracingValid(LineString seg)
        {
            return m_currentTracingLength + seg.Length <= m_maxSingleTracingLength;
        }

        /// <summary>
        /// if any part of seg is out side of boundary, reject it TODO: consider adding more
        /// </summary>
        public LineString ModifySegment(LineString seg)
        {
            if (seg.Length < 0.01f)
            {
                Debug.Log("segment length is too small, rejecting...");
                return null;
            }

            if (m_boundary != null && seg.Intersects(m_boundary))
            {
                Debug.Log("segment crosses m_boundary, rejecting...");
                return null;
            }

            if (!IsCurrentTracingValid(seg))
            {
                return null;
            }

            // modify segment
            Coordinate endPt = seg[1];
            KdNode<SeedPoint> cloest = m_placedPoints.NearestNeighbor(endPt);

            if(cloest.Coordinate.Distance(endPt) < m_snapThreshold)
            {
                Coordinate newEndPt = cloest.Coordinate;
                return new LineString(new Coordinate[] { seg[0], newEndPt });
            }

            return seg;
        }

        public List<SeedPoint> GetCandiatePoints(SeedPoint pt)
        {
            var p = new SeedPoint(pt.Pos) { PriorityValue = GetPriorityValue(pt.Pos)};
            return new List<SeedPoint>() { p };
        }

        /// <summary>
        /// 
        /// </summary>
        public float GetPriorityValue(Coordinate pos)
        {
            Coordinate centre = new Coordinate(10, 10);
            double dist = centre.Distance(pos);
            return 1/(float)dist;
        }

        public Coordinate GetDirFromPosAndEigenType(Coordinate pos, EigenType eigenType)
        {
            Coordinate dir;
            m_combinedFields.SampleAtPos(pos).EigenVectors(eigenType, out dir);
            return dir;
        }

        public SeedPoint GetNextPoint(SeedPoint currPt, Coordinate currDir)
        {
            // if angle between dir and prevDir > 90, reverse dir

            var nextpos = currPt.Pos.Add(currDir.Multiplication(m_stepLength));
            Debug.Log($"next point distance from curr point is {nextpos.Distance(currPt.Pos)}");
            return new SeedPoint(nextpos);
        }

        public Coordinate GetNextDir(SeedPoint pt,Coordinate prevDir)
        {
            Coordinate dir = GetDirFromPosAndEigenType(pt.Pos, m_currentTracingEigenType);

            if (!IsAngleValid(prevDir, dir))
            {
                dir = Extension.Reverse(dir);
            }

            return dir;
        }

        public bool IsAngleValid(Coordinate vec, Coordinate vec1)
        {
            double sqrxy = vec.X * vec1.X + vec.Y * vec1.Y;
            return sqrxy >= 0;
        }
    }

    public class StreamLineVis : MonoBehaviour
    {
        private RoadMapGenerator m_generator;

        private void Start()
        {

            var fields = new AddedField(TensorFieldProvider.GetTensorField());
            var pos = new Coordinate(6, 6);
            var startpt = new SeedPoint(pos);
            m_generator = new RoadMapGenerator(fields, startpt, null, 5,30,10);
            m_generator.Iterate();
            m_generator.Draw();
        }

        private void Update()
        {
            m_generator.Draw();
        }

        private IEnumerator waitforsec()
        {
            yield return new WaitForSeconds(1);
        }
    }
}