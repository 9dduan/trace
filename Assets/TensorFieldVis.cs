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

public static class TensorFieldProvider
{
    public static ITensorField[] GetTensorField()
    {
        // rad
        RadialTensorField rad = new RadialTensorField(new Coordinate(0, 0));
        // polyline
        Coordinate[] coords = new Coordinate[] { new Coordinate(0, 0), new Coordinate(8, 2), new Coordinate(20, 12), new Coordinate(40, 19) };
        PolylineField poly = new PolylineField(coords, 0.08);
        // grid
        double ang = (44f / 180f) * Math.PI;
        GridTensorField grid = new GridTensorField(ang, 1, 1);
        return new ITensorField[] {rad, poly, grid};
    }
}

public class TensorFieldVis : MonoBehaviour
{
    AddedField added;
    ITensorField toDraw;
    Coordinate[] pts;
    Coordinate[] vecs;
    StreamLine streamLine;


    //visualize a tensorfield
    // Start is called before the first frame update
    void Start()
    {
        added = new AddedField(TensorFieldProvider.GetTensorField());

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

        //streamLine = new StreamLine(0.5f,100f,added);
        //streamLine.SetStartingPoint(new Coordinate(5, 10), EigenType.Major);

    }

    // Update is called once per frame
    void Update()
    {
        //toDraw.DrawDebugShape();
        added.DrawDebugShape();
        Utils.DrawShapeOnPositions(pts, vecs, (pt,vec)=> Utils.DrawShapeOnPosition(pt,vec,"cross"));
        //streamLine.Trace();
        //streamLine.Draw();
    }
}
