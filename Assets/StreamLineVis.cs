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

namespace tracing.StreamLineVis
{
    public class StreamLineVis : MonoBehaviour
{
    private List<StreamLine> streamLines;
    private AddedField added;
    private StreamLine curline;
    //private SeedProvider provider;

    private void Start()
    {
        streamLines = new List<StreamLine>();
        added = new AddedField(TensorFieldProvider.GetTensorField());
        var s = new StreamLine(2, 60f, added);
        s.SetStartingPoint(new Coordinate(12, 10), EigenType.Major);
        streamLines.Add(s);
        curline = s;
        
        //s.Trace();
    }

    private void Update()
    {
        //StartCoroutine("waitforsec");
        //var curline = streamLines.Last();
        Debug.Log(streamLines.Count());
        if (streamLines.Count() < 15) updatelines();
        foreach (var streamLine in streamLines)
        {
            //streamLine.StepTrace();
            streamLine.DrawSelf();
        }
    }

    private void updatelines()
    {
        if (curline.StepTrace() == 0)
        {
            var newStart = SeedProvider.seedqueue.pop();
            var newStartType = newStart.eigenType == EigenType.Major ? EigenType.Minor : EigenType.Major;
            var newStreamline = new StreamLine(2, 60f, added);
            newStreamline.SetStartingPoint(newStart.Pos, newStartType);
            streamLines.Add(newStreamline);
            curline = newStreamline;
            Debug.Log($"starting new line at {newStart.Pos.X}-{newStart.Pos.Y}");
        }
        else
        {

        }
    }

    private IEnumerator waitforsec()
    {
        yield return new WaitForSeconds(1);
    }

    private void IterativeTrace()
    {
        
    }
}
}