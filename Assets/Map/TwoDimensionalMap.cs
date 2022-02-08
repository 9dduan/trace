using System;
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

namespace trace.Maps
{
    public class TwoDimensionalMap
    {
        private LinearRing m_boundary;
        private LinearRing[] m_waterBoundaries;

        public TwoDimensionalMap(LinearRing boundary, LinearRing[] w_boundaries)
        {
            m_boundary = boundary;
        }
    }
}
