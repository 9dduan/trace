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
using System.Threading;
using MathNet.Numerics;
using trace.TensorField;
using trace.TensorStruct;
using trace.SeedPoint;
using trace.PriorityCalculator;

namespace tracing.StreamLineVis
{
    public class RoadMapGenerator : MonoBehaviour
    {
        private IPriorityCalculator m_priorityCalc = new RdmPriorityCalculator();
        private SeedPoint m_startingSeedPoint;
        private AddedField m_combinedFields;
        private PrioQueue.PriorityQueue<SeedPoint> m_candidatesQueue;
        private KdTree<TracePoint> m_placedPoints;
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
            m_placedPoints = new KdTree<TracePoint>();
            m_generatedLines = new List<List<LineString>>();
            m_maxTracedLines = maxTracedLines;
            m_boundary = boundary;
            m_snapThreshold = (float)m_stepLength*0.5f;
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
        public IEnumerator Iterate()
        {
            // set starting conditions
            m_candidatesQueue.push(m_startingSeedPoint);
            m_currentTracingEigenType = m_startingSeedPoint.EigenType;
            m_placedPoints.Insert(m_startingSeedPoint.Pos, m_startingSeedPoint);
            int iterativeCount = 0;

            while (m_candidatesQueue.Count > 0 && iterativeCount <m_maxTracedLines)
            {
                yield return new WaitForSeconds(0.5f);

                iterativeCount += 1;
                
                var seed = m_candidatesQueue.pop();
                List<SeedPoint> poped = new List<SeedPoint>();

                while (seed.EigenType != m_currentTracingEigenType)
                {
                    poped.Add(seed);
                    seed = m_candidatesQueue.pop();
                }

                foreach(var p in poped)
                {
                    m_candidatesQueue.push(p);
                }

                if(seed.EigenType!= m_currentTracingEigenType)
                {
                    Debug.Log($"{seed.EigenType}-{m_currentTracingEigenType}");
                }
                

                Coordinate currDir = GetDirFromPosAndEigenType(seed.Pos, m_currentTracingEigenType);

                if (!(seed.IsForward))
                {
                    currDir = currDir.Reverse();
                }

                TracePoint currPt = new TracePoint(seed.Pos);
                m_currentTracingLine = new List<LineString>();
                m_currentTracingLength = 0f;

                Debug.Log($"Start new line from point {currPt.Pos.X} - {currPt.Pos.Y} direction {currDir.X} - {currDir.Y}");

                int count = 0;
                while (count<1000)
                {
                    // trace current single streamline

                    count += 1;

                    Coordinate nextPos = GetNextPos(currPt.Pos, currDir);
                    LineString currSeg = new LineString(new Coordinate[] { currPt.Pos, nextPos });

                    var modifiedSeg = ModifySegment(currSeg);

                    if (modifiedSeg != null)
                    {
                        m_currentTracingLength += modifiedSeg.Length;
                        m_currentTracingLine.Add(modifiedSeg);

                        // var nextPt = new SeedPoint(modifiedSeg[1], true, m_currentTracingEigenType==EigenType.Minor ? EigenType.Major:EigenType.Minor);
                        var nextPt = new TracePoint(modifiedSeg[1]);
                        var nextDir = GetNextDir(nextPt, currDir);
                       
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
                m_currentTracingEigenType = Extension.SwitchEigen(m_currentTracingEigenType);
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
            KdNode<TracePoint> cloest = m_placedPoints.NearestNeighbor(endPt);

            if(cloest.Coordinate.Distance(endPt) < m_snapThreshold)
            {
                Coordinate newEndPt = cloest.Coordinate;
                return new LineString(new Coordinate[] { seg[0], newEndPt });
            }

            return seg;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<SeedPoint> GetCandiatePoints(TracePoint pt)
        {
            var eigen = Extension.SwitchEigen(m_currentTracingEigenType);
            System.Random r1 = new System.Random();

            var p1 = new SeedPoint(pt.Pos, true, eigen)  { PriorityValue = m_priorityCalc.GetPriorityValue(pt.Pos) };
            var p2 = new SeedPoint(pt.Pos, false, eigen) { PriorityValue = m_priorityCalc.GetPriorityValue(pt.Pos) };
            return new List<SeedPoint>() { p1, p2 };
        }

        /// <summary>
        /// 
        /// </summary>
        public float GetPriorityValue(Coordinate pos)
        {
            Coordinate centre = new Coordinate(10, 10);
            double dist = centre.Distance(pos);
            return Mathf.Exp(-(float)dist*(float)dist);
        }

        public EigenType GetReversedCurrentEigen()
        {
            return m_currentTracingEigenType == EigenType.Major ? EigenType.Minor : EigenType.Major;
        }

        public Coordinate GetDirFromPosAndEigenType(Coordinate pos, EigenType eigenType)
        {
            Coordinate dir;
            m_combinedFields.SampleAtPos(pos).EigenVectors(eigenType, out dir);
            return dir;
        }

        public SeedPoint GetNextPoint(SeedPoint currPt, Coordinate currDir, bool isFoward)
        {
            // if angle between dir and prevDir > 90, reverse dir

            var nextpos = GetNextPos(currPt.Pos, currDir);
            Debug.Log($"next point distance from curr point is {nextpos.Distance(currPt.Pos)}");
            return new SeedPoint(nextpos, isFoward, Extension.SwitchEigen(m_currentTracingEigenType));
        }

        public Coordinate GetNextPos(Coordinate currPos, Coordinate currDir)
        {
            var nextpos = currPos.Add(currDir.Multiplication(m_stepLength));
            return nextpos;
        }

        public Coordinate GetNextDir(TracePoint pt,Coordinate prevDir)
        {
            return GetNextDir(pt.Pos, prevDir);
        }

        public Coordinate GetNextDir(Coordinate ptPos,Coordinate prevDir)
        {
            Coordinate dir = GetDirFromPosAndEigenType(ptPos, m_currentTracingEigenType);

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
            var fields = new AddedField(TensorFieldProvider.GetPresetTensorFields());
            var pos = new Coordinate(6, 6);
            var startpt = new SeedPoint(pos, true, EigenType.Major);
            m_generator = new RoadMapGenerator(fields, startpt, null, 4, 50, 80);
            StartCoroutine(m_generator.Iterate());
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