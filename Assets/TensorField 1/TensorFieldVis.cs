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
using trace.TensorStruct;
using trace.TensorField;

public static class TensorFieldProvider
{
    public static ITensorField[] GetPresetTensorFields()
    {
        // rad
        RadialTensorField radField = new RadialTensorField(new Coordinate(12, 10), 0.1);

        // polyline
        Coordinate[] coords = new Coordinate[] { new Coordinate(0, 0), new Coordinate(8, 2), new Coordinate(20, 12), new Coordinate(40, 19), new Coordinate(41, 25) };
        PolylineField polyField = new PolylineField(coords, 0.08);

        // grid
        double ang = (44f / 180f) * Math.PI;
        GridTensorField gridField = new GridTensorField(ang, 1, 0.1);

        return new ITensorField[] {radField, polyField };
    }
}

public class TensorFieldVis : MonoBehaviour
{
    AddedField added;
    Coordinate[] pts;
    Coordinate[] vecs;
    LinearRing m_boundary;

    void Start()
    {
        added = new AddedField(TensorFieldProvider.GetPresetTensorFields());

        pts = Utils.PopulatePointsOnGrid(2);

        vecs = pts.Select(
            p =>
            {
                var tensor = added.SampleAtPos(p);
                Coordinate major;
                Coordinate minor;
                tensor.EigenVectors(out major, out minor);
                return major;
            }).ToArray();

    }

    void Update()
    {
        added.DrawDebugShape();

        if(m_boundary != null)
        {
            Utils.DrawLineString(m_boundary, Color.cyan);
        }

        Utils.DrawShapeOnPositions(pts, vecs, (pt, vec) => Utils.DrawShapeOnPosition(pt, vec, "lshape"));
    }
}