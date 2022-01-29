using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using NetTopologySuite;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

public static class Utils
{
    public const string CROSS = "cross";
    public const string LSHAPE = "lshape";

    // create grid and mesh on grid
    public static Coordinate[] PopulatePointsOnGrid(float width,int x=20, int y=20)
    {
        // draw dumb grid of points, consider use a topology lib such as shapely to implement this
        Coordinate[] res = new Coordinate[x*y];

        //spaw grid of points
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                res[i * x + j] = new Coordinate(i * width,j * width);
            }
        }

        return res;
    }

    public static void GenerateMesh(Coordinate[] coordinates)
    {
        GameObject obj = new GameObject();
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.green;
        renderer.material = mat;
    }

    public static Coordinate[] PopulatePointsInsideBoundary(LinearRing boundary, float interval)
    {
        throw new NotImplementedException();
    }

    public static void DrawShapeOnPositions(Coordinate[] pos, Coordinate[] vecs, Action<Coordinate, Coordinate> func)
    {
        if (pos.Count() != vecs.Count())
        {
            throw new Exception("pos and ves don't have same count");
        }

        for (int i = 0; i < pos.Count(); i++)
        {
            var p = pos[i];
            var vec = vecs[i];
            func(p, vec);
        }
    }

    public static void DrawShapeOnPosition(Coordinate pos, Coordinate vec, string shape = null)
    {
        float mag = 0.5f;
        var pos3d = CoordToVector3(pos);
        var vec3d = CoordToVector3(vec).normalized;
        
        bool isVecValid = vec3d != Vector3.zero;
        if(!isVecValid)
        {
            var pt1 = pos3d - 0.5f*Vector3.right;
            var pt2 = pos3d + 0.5f*Vector3.right; 
            Debug.DrawLine(pt1, pt2, Color.white);
            
        }
        else
        {
            var rotated = Vector3.Cross(vec3d, Vector3.up).normalized;
            var pt1 = pos3d + vec3d;
            var pt2 = pos3d - vec3d;
            var pt3 = pos3d + mag * rotated;
            var pt4 = pos3d - mag * rotated;

            if (string.IsNullOrEmpty(shape))
            {
                Debug.DrawLine(pt1, pt2);
            }
            else if (shape.Equals(CROSS))
            {
                Debug.DrawLine(pt1, pt2, Color.red);
                Debug.DrawLine(pt3, pt4, Color.blue);
            }
            else if (shape.Equals(LSHAPE))
            {
                Debug.DrawLine(pos3d, pt1, Color.red);
                Debug.DrawLine(pos3d, pt3, Color.blue);
            }
        }
    }



    public static void DrawLineString(LineString geom, Color color)
    {
        if (geom == null)
        {
            throw new Exception("invalid geom");
        }
        for (int i = 0; i < geom.Coordinates.Length - 1; i++)
        {
            var pt = geom.Coordinates[i];
            var next = geom.Coordinates[i + 1];
            var start = new Vector3((float)pt.X, 0, (float)pt.Y);
            var end = new Vector3((float)next.X, 0, (float)next.Y);
            Debug.DrawLine(start, end, color);
        }
    }

    public static Vector3 CoordToVector3(Coordinate coord)
    {
        return new Vector3((float)coord.X, 0, (float)coord.Y);
    }
}

// test class todo remove
public class DrawLine : MonoBehaviour
{
    private LineRenderer lr;
    public float x = 0.1f;
    public float y = 0.1f;
    const string CROSS = "cross";
    private IEnumerable<Vector3> testPoints;
    // Start is called before the first frame update
    void Start()
    {
        InitLineRenderer();
        testPoints = GeneratePointsOnGrid(2f);
    }

    // Update is called once per frame
    void Update()
    {
        //DrawCrossOnPosition(new Vector2(0, 0), new Vector2(1, 1));
        var vec = new Vector2(1, 1);
        foreach (var p in this.testPoints)
        {
            //DrawSignOnPosition(new Vector2(p.x,p.z), vec, CROSS);
        }

        //DrawExampleTopoLinestring();
    }

    private void FixedUpdate()
    {

    }

    void InitLineRenderer()
    {
        lr = GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.startColor = Color.blue;
        lr.endColor = Color.blue;
    }

    void DrawSineCurve()
    {
        //set some positions
        Vector3[] positions = new Vector3[100];

        for (int i = 0; i < 100; i++)
        {
            positions[i] = new Vector3(x, y, 0);
            x += 0.1f; // x = x + 0.1f x+=0.1f
            y = Mathf.Sin(x);
        }

        lr.positionCount = positions.Length;
        lr.SetPositions(positions);
    }

    Vector3[] GeneratePointsOnGrid(float width)
    {
        // draw dumb grid of points, consider use a topology lib such as shapely to implement this
        int num = 50;

        Vector3[] res = new Vector3[100];

        //spaw grid of points
        for(int i = 0; i < 10; i++)
        {
            for(int j = 0; j < 10; j++)
            {
                res[i * 10 + j] = new Vector3(i * width, 0, j * width);
            }
        }

        return res;
    }

    void DrawDiamondLineRendererOnPosition(Vector2 pos,Vector2 vec)
    {
        //float half = 0.5f;
        var pos3d = ConvertToVector3(pos).normalized;
        var vec3d = ConvertToVector3(vec).normalized;

        var rotated = Vector3.Cross(vec3d, Vector3.up);
        var pt1 = pos3d + vec3d;
        var pt2 = pos3d - vec3d;
        var pt3 = pos3d + rotated;
        var pt4 = pos3d - rotated;
        var pts = new[] { pt1, pt3, pt2, pt4, pt1};

        lr.positionCount = pts.Length;
        lr.SetPositions(pts.ToArray());
    }

    Vector3 ConvertToVector3(Vector2 vec)
    {
        return new Vector3(vec.x, 0, vec.y);
    }

    void DrawExampleTopoLinestring()
    {
        var pt1 = new Coordinate(0, 0);
        var pt2 = new Coordinate(1, 1);
        var pt3 = new Coordinate(2, 4);
        var geom = new LineString(new Coordinate[] {pt1,pt2,pt3});
        DrawLineString(geom);
    }

    void DrawLineString(LineString geom)
    {
        if (geom == null)
        {
            throw new Exception("invalid geom");
        }
        for (int i = 0; i < geom.Coordinates.Length - 1; i++)
        {
            var pt = geom.Coordinates[i];
            var next = geom.Coordinates[i + 1];
            var start = new Vector3((float)pt.X, 0, (float)pt.Y);
            var end = new Vector3((float)next.X, 0, (float)next.Y);
            Debug.DrawLine(start, end, Color.magenta);
        }
    }

    static LineRenderer CreateLineRenderer()
    {
        // todo instantiate prefab to get multiple lineRenderes
        throw new Exception();
    }
}
