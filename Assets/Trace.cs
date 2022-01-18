using System.Collections;
using System.Collections.Generic;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm;
using UnityEngine;
using MathNet;
using System;
using PrioQueue;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics;

// classes to trace a single streamline
// streamline
// seed
// seedPool

public enum EigenType
{
      Major,
    Minor
}

public class SeedPoint
{
    public bool IsSeed { get; set; }

    public float PriorityValue { get; set; }

    public Coordinate Pos { get; set; } // position

    public SeedPoint(Coordinate pos)
    {
        Pos = pos;
    }

    public override string ToString()
    {
        return $"{Pos.ToString()} PriorityValue {PriorityValue} IsSeed {IsSeed}";
    }
}

public static class RandomSeedProvider
{
    public static PriorityQueue<SeedPoint> seedqueue = new PriorityQueue<SeedPoint>(100,new SeedComparorWithRandomValue());
}


public class SeedComparor : IComparer<SeedPoint>
{
    public int Compare(SeedPoint p1,SeedPoint p2)
    {
        if (p1.PriorityValue > p2.PriorityValue)
        {
            return 1;
        }
        else if(p1.PriorityValue < p2.PriorityValue)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}

public class SeedComparorWithRandomValue : IComparer<SeedPoint>
{
    public int Compare(SeedPoint x, SeedPoint y)
    {
        System.Random r1 = new System.Random();
        return r1.Next(-1,1);
    }
}

/*
public class StreamLine
{
    private List<SeedPoint> m_points;

    private LineString m_polyline;

    //private SeedProvider seedProvider;

    private ITensorField m_tensorfield; // consider move this out of here

    private int maxIteration = 150;

    private int i = 0;

    public float StepLength { get; }

    public float MaxLength { get; }

    public StreamLine(float _stepLength, float _maxLength, ITensorField field)
    {
        MaxLength = _maxLength;
        StepLength = _stepLength;
        m_tensorfield = field;
        m_points = new List<SeedPoint>();
    }

    public void DrawSelf()
    {
        //draw linestring
        try
        {
            Utils.DrawLineString(m_polyline, Color.magenta);
        }
        catch
        {
            Debug.Log("drawing error");
        }
    }

    public void SetStartingPoint(Coordinate pt, EigenType eigenType)
    {
        if (!m_points.Any())
        {

            var tensor = m_tensorfield.SampleAtPos(pt).Normalize();
            Coordinate vec;
            tensor.EigenVectors(eigenType, out vec);
            m_points.Add(new SeedPoint(pt, vec, eigenType));

        }
        else
        {
            Debug.Log("points not empty");
        }
    }

    public int StepTrace()
    {
        if (m_points.Any())
        {
            double lineLength = m_polyline != null ? m_polyline.Length : 0;
            if (lineLength < MaxLength && i < maxIteration)
            {
                GetNextPoint();
                lineLength = m_polyline.Length;
                Debug.Log($"current iteration:{i++},polyline length:{lineLength},max length:{MaxLength}");
                return 1;
            }
            return 0;
        }
        return 0;
    }

    public void Trace()
    {
        if (m_points.Any())
        {
            double lineLength = 0;
            while(lineLength < MaxLength && i <maxIteration)
            {
                GetNextPoint();
                lineLength = m_polyline.Length;
                Debug.Log($"current iteration:{i++},polyline length:{lineLength},max length:{MaxLength}"); 
            }
        }
    }

    private IEnumerator waitforsec()
    { 
        yield return new WaitForSeconds(0.1f);
    }

    private void GetNextPoint()
    {
        SeedPoint nextPt = new SeedPoint(default(Coordinate),default(Coordinate),EigenType.Major);

        if (m_points.Any())
        {
            var prevPt = m_points.Last();

            // TODO consider if vector at sampled position is zero 
            Coordinate step = Extension.Multiplication(prevPt.Dir, StepLength);
            Coordinate nextPos = Extension.Add(prevPt.Pos, step);
            Coordinate nextVec;

            var tensor = m_tensorfield.SampleAtPos(nextPos).Normalize();
            Coordinate major;
            Coordinate minor;
            tensor.EigenVectors(out major,out minor);

            Coordinate corrected = prevPt.EigenType == EigenType.Major ? major : minor;
            nextVec = IsAngleValid(corrected, prevPt.Dir) ? corrected : Extension.Reverse(corrected);
   
            nextPt.Pos = nextPos;
            nextPt.EigenType = prevPt.EigenType;
            nextPt.Dir = nextVec;

            m_points.Add(nextPt);
            SeedProvider.seedqueue.push(nextPt);
            m_polyline = new LineString(m_points.Select(p => p.Pos).ToArray()); // update linestring
        }
        else
        {
            throw new Exception("no previous point exists");
        }

    }
        
  
    private bool IsAngleValid(Coordinate vec,Coordinate vec1)
    {
        double sqrxy = vec.X * vec1.X + vec.Y * vec1.Y;
        //double magsum = Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y) * Math.Sqrt(vec1.X * vec1.X + vec1.Y + vec1.Y);
        //var ang = Math.Acos(sqrxy / magsum);
        return sqrxy >= 0;
    }

}
*/