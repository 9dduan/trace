﻿using System;
using System.Collections;
using System.Collections.Generic;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Index.KdTree;
using UnityEngine;
using MathNet;
using System.Linq;
using System.Threading;
using MathNet.Numerics;
using trace.TensorField;
using trace.TensorStruct;
using trace.SeedPoint;

namespace trace.PriorityCalculator
{
    public interface IPriorityCalculator
    {
        float GetPriorityValue(Coordinate pos);

        // TODO make a shared combine method
    }

    public class RdmPriorityCalculator : IPriorityCalculator
    {
        public float GetPriorityValue(Coordinate pos)
        {
            System.Random r1 = new System.Random();
            return r1.Next(0, 10);
        }
    }

    public class RadialPriorityCalculator : IPriorityCalculator
    {
        private Coordinate centre = new Coordinate(10, 10);

        public float GetPriorityValue(Coordinate pos)
        {
            float dist = (float)centre.Distance(pos);
            return Mathf.Exp(-dist * dist);
        }
    }

    public class AddedPriorityCalculator : IPriorityCalculator
    {
        private IPriorityCalculator[] m_priorityCalculators;
        
        public float GetPriorityValue(Coordinate pos)
        {
            float res = 0;

            foreach(var calc in m_priorityCalculators)
            {
                res += calc.GetPriorityValue(pos);
            }

            return res;
        }

        public AddedPriorityCalculator(IPriorityCalculator[] priorityCalculators)
        {
            m_priorityCalculators = priorityCalculators;
        }
    }

}
