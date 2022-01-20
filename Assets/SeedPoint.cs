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
namespace trace.SeedPoint
{
    public enum EigenType
    {
        Major,
        Minor
    }

    public class SeedPoint
    {
        public bool IsSeed { get; set; }

        public bool IsForward { get; set; }

        public float PriorityValue { get; set; }

        public EigenType EigenType { get; set; }

        public Coordinate Pos { get; set; } // position

        public SeedPoint(Coordinate pos, bool isForward, EigenType eigenType)
        {
            Pos = pos;
            IsForward = isForward;
            EigenType = eigenType;
        }

        public override string ToString()
        {
            return $"{Pos.ToString()} PriorityValue {PriorityValue} IsSeed {IsSeed}";
        }

        public SeedPoint Clone()
        {
            var pt = new SeedPoint(this.Pos, this.IsForward, this.EigenType)
            {
                PriorityValue = this.PriorityValue,
                IsSeed = this.IsSeed
            };

            return pt;
        }
    }

    public static class RandomSeedProvider
    {
        public static PriorityQueue<SeedPoint> seedqueue = new PriorityQueue<SeedPoint>(100, new SeedComparorWithRandomValue());
    }


    public class SeedComparor : IComparer<SeedPoint>
    {
        public int Compare(SeedPoint p1, SeedPoint p2)
        {
            if (p1.PriorityValue > p2.PriorityValue)
            {
                return 1;
            }
            else if (p1.PriorityValue < p2.PriorityValue)
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
            return r1.Next(-1, 1);
        }
    }
}