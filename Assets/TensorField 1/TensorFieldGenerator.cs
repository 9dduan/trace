using System.Collections;
using System.Collections.Generic;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm;
using UnityEngine;
using MathNet;
using System;
using System.Linq;
using MathNet.Numerics;
using trace.TensorField;
using trace.TensorStruct;
using trace.SeedPoint;
using trace.PriorityCalculator;

namespace trace.TensorField
{
    public class TensorFieldGenerator : MonoBehaviour
    {
        RadialTensorField m_radField;
        PolylineField m_polylineField;
        GridTensorField m_gridTensorField;

        public bool useRadial;
        public bool usePolyLine;
        public bool useGrid;

        public float radialCentreX;
        public float radialCentreY;

        public ITensorField[] Generate()
        {
            return null;
        }

        /**
        // rad
        RadialTensorField radField = new RadialTensorField(new Coordinate(12, 10), 0.1);

        // polyline
        Coordinate[] coords = new Coordinate[] { new Coordinate(0, 0), new Coordinate(8, 2), new Coordinate(20, 12), new Coordinate(40, 19), new Coordinate(41, 25) };
        PolylineField polyField = new PolylineField(coords, 0.08);

        // grid
        double ang = (44f / 180f) * Math.PI;
        GridTensorField gridField = new GridTensorField(ang, 1, 0.1);

        return new ITensorField[] {radField, polyField
        */
    }
}

