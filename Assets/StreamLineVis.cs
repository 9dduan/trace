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
        private Polygon m_boundary;

        public RoadMapGenerator(AddedField combinedFields, SeedPoint startPoint, double stepLength, double maxLength)
        {
            m_combinedFields = combinedFields;
            m_startingSeedPoint = startPoint;
            m_stepLength = stepLength;
            m_maxSingleTracingLength = maxLength;
            m_candidatesQueue = new PrioQueue.PriorityQueue<SeedPoint>(1000, new SeedComparor());
            m_placedPoints = new KdTree<SeedPoint>();
            m_generatedLines = new List<List<LineString>>();
        }

        public void Draw()
        {
            try
            {
                foreach (var l in m_generatedLines)
                {
                    foreach (var seg in l)
                    {
                        Utils.DrawLineString(seg, Color.magenta);
                    }
                }
                
            }
            catch
            {
                Debug.Log("drawing error");
            }
        }

        public void iterate()
        {
            // initate conditions
            m_candidatesQueue.push(m_startingSeedPoint);
            m_currentTracingEigenType = EigenType.Major; // init eigen type as major;
            Coordinate prevDir = GetDirFromPt(m_startingSeedPoint.Pos, m_currentTracingEigenType);

            int maxInterative = 0;
            while (m_candidatesQueue.Count > 0 && maxInterative <5000)
            {
                maxInterative += 1;
                SeedPoint pt = m_candidatesQueue.pop();

                m_currentTracingLine = new List<LineString>();
                m_currentTracingLength = 0f;
                m_placedPoints.Insert(pt.Pos, pt);
                
                int count = 0;
                while (count<1000)
                {
                    // while valid, trace current (direction) line
                    count += 1;
                    var nextPt = GetNextFromSeedPoint(pt,prevDir);
                    LineString currSeg = new LineString(new Coordinate[] { pt.Pos, nextPt.Pos });

                    if (IsSegmentValid(currSeg) && IsCurrentTracingValid(currSeg))
                    {
                        m_currentTracingLength += currSeg.Length;
                        m_currentTracingLine.Add(currSeg);
                        m_placedPoints.Insert(nextPt.Pos, nextPt);
                        prevDir = GetDirFromPt(nextPt.Pos, m_currentTracingEigenType);
                        pt = nextPt;
                        var newCandidates = GetCandiatePoints(pt);

                        if (newCandidates.Any())
                        {
                            foreach(var c in newCandidates)
                            {
                                //c.PriorityValue = 1f;
                                m_candidatesQueue.push(c);
                            }
                        }
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
        }

        // TODO: add more validation method
        public bool IsCurrentTracingValid(LineString seg)
        {
            return m_currentTracingLength + seg.Length <= m_maxSingleTracingLength;
        }

        /// <summary>
        /// if any part of seg is out side of boundary, reject it TODO: consider adding more
        /// </summary>
        public bool IsSegmentValid(LineString seg)
        {
            if (seg.Length < 0.01f)
            {
                Debug.Log("segment crosses m_boundary or length is too small, rejecting...");
                return false;
            }
            return true;
        }

        public List<SeedPoint> GetCandiatePoints(SeedPoint pt)
        {
            var p = new SeedPoint(pt.Pos) { PriorityValue = (float)pt.Pos.Distance(new Coordinate(10, 10))};
            return new List<SeedPoint>() { p};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public float GetPriorityValue(Coordinate pos)
        {
            Coordinate centre = new Coordinate(10, 10);
            double dist = centre.Distance(pos);
            return 1;
        }

        public Coordinate GetDirFromPt(Coordinate pos, EigenType eigenType)
        {
            Coordinate dir;
            m_combinedFields.SampleAtPos(pos).EigenVectors(eigenType, out dir);
            return dir;
        }

        public SeedPoint GetNextFromSeedPoint(SeedPoint prevPt, Coordinate prevDir)
        {
            // if angle between dir and prevDir > 90, reverse dir

            var nextpos = prevPt.Pos.Add(prevDir.Multiplication(m_stepLength));

           

            return new SeedPoint(nextpos);
        }

        public Coordinate GetNextDir(SeedPoint pt,Coordinate prevDir)
        {
            Coordinate dir = GetDirFromPt(pt.Pos, m_currentTracingEigenType);

            if (!IsAngleValid(prevDir, dir))
            {
                dir = Extension.Reverse(dir);
            }

            return dir;
        }

        public LineString ModifySegment(LineString seg)
        {
            throw new NotImplementedException();
        }

        public bool IsAngleValid(Coordinate vec, Coordinate vec1)
        {
            double sqrxy = vec.X * vec1.X + vec.Y * vec1.Y;
            return sqrxy >= 0;
        }
    }

    public class StreamLineVis : MonoBehaviour
    {

        
        private AddedField added;
        private RoadMapGenerator m_generator;
        //private SeedProvider provider;

        private void Start()
        {
            /**
            streamLines = new List<StreamLine>();
            added = new AddedField(TensorFieldProvider.GetTensorField());
            var s = new StreamLine(2, 60f, added);
            s.SetStartingPoint(new Coordinate(12, 10), EigenType.Major);
            streamLines.Add(s);
            curline = s;
            
            //s.Trace();
            */

            var fields = new AddedField(TensorFieldProvider.GetTensorField());
            var pos = new Coordinate(6, 6);
            var startpt = new SeedPoint(pos);
            m_generator = new RoadMapGenerator(fields, startpt, 5, 60);
            m_generator.iterate();
            m_generator.Draw();
        }

        private void Update()
        {
            /*
            //StartCoroutine("waitforsec");
            //var curline = streamLines.Last();
            Debug.Log(streamLines.Count());
            if (streamLines.Count() < 15) updatelines();
            foreach (var streamLine in streamLines)
            {
                //streamLine.StepTrace();
                streamLine.DrawSelf();
            }
            */
            m_generator.Draw();
        }

        private void updatelines()
        {
            /*
            if (curline.StepTrace() == 0)
            {
                
                var newStart = SeedProvider.seedqueue.pop();
                var newStartType = newStart.EigenType == EigenType.Major ? EigenType.Minor : EigenType.Major;
                var newStreamline = new StreamLine(2, 60f, added);
                newStreamline.SetStartingPoint(newStart.Pos, newStartType);
                streamLines.Add(newStreamline);
                curline = newStreamline;
                Debug.Log($"starting new line at {newStart.Pos.X}-{newStart.Pos.Y}");
                
            }
            else
            {
                // do nothing
            }
            */
        }

        private IEnumerator waitforsec()
        {
            yield return new WaitForSeconds(1);
        }
    }
}