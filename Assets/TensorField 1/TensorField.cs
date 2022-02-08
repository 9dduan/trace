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
using trace.SeedPoint;

namespace trace.TensorField
{
    // todo rewrite sample function with RK4 
    public interface ITensorField
    {
        void Combine(ITensorField field); // combine current tensor field with another one
        Tensor SampleAtPos(Coordinate pos); // return tensor at given position
        double decay { get; set; }
        void DrawDebugShape();
    }

    // radial tensor field
    public class RadialTensorField : MonoBehaviour, ITensorField
    {
        private Coordinate m_cent;
        private double m_decay;
        private bool m_isHidden = true;

        public double decay { get; set; }

        public RadialTensorField(Coordinate _cent, double _decay = 0)
        {
            m_cent = _cent;
            decay = _decay;
            m_decay = _decay;
        }

        public void Combine(ITensorField tensorField)
        {
            throw new NotImplementedException();
        }

        public void DrawDebugShape()
        {
            m_isHidden = false;
        }

        public Tensor SampleAtPos(Coordinate pos)
        {
            Coordinate relativePos = new Coordinate(pos.X - m_cent.X, pos.Y - m_cent.Y);
            double dist = pos.Distance(m_cent);
            var decayCoeff = Math.Exp(-m_decay * dist * dist); // by default decayCoeff == 1
            var tensor = Tensor.FromXY(relativePos).Normalize();
            return decayCoeff * tensor;
        }

        void OnDrawGizmos()
        {
            if (!m_isHidden)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(Utils.CoordToVector3(m_cent), 0.2f);
            }
        }
    }

    // grid tensor field
    public class GridTensorField : ITensorField
    {
        private Tensor tensor;

        public GridTensorField(double _theta, double _r, double _length)
        {
            tensor = _length * Tensor.FromRTheta(_r, _theta);
        }

        public double decay { get; set; }

        public void Combine(ITensorField tensorField)
        {
            throw new System.Exception();
        }

        public void DrawDebugShape()
        {
            throw new NotImplementedException();
        }

        public Tensor SampleAtPos(Coordinate pos)
        {
            return tensor; // in grid tensorfield tensor at all positions is universal
        }
    }

    // polyline tensor field
    public class PolylineField : ITensorField
    {
        private LineString polyline;
        private List<LineSegment> lineSegments = new List<LineSegment>();
        public double decay { get; set; }

        public PolylineField(Coordinate[] coords, double _decay = 0)
        {
            decay = _decay; // todo consider proper default value

            try
            {
                polyline = new LineString(coords);
                for (int i = 0; i < polyline.Coordinates.Length - 1; i++)
                {
                    var start = polyline.Coordinates[i];
                    var end = polyline.Coordinates[i + 1];
                    lineSegments.Add(new LineSegment(start, end));
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public void Combine(ITensorField tensorField)
        {
            throw new NotImplementedException();
        }

        public void DrawDebugShape()
        {
            Utils.DrawLineString(polyline, Color.black);
        }

        public Tensor SampleAtPos(Coordinate pos)
        {
            Tensor result = new Tensor(0, 0);
            if (lineSegments.Any())
            {
                foreach (var line in lineSegments)
                {
                    var currentTensor = Tensor.FromRTheta(1, line.Angle).Normalize();
                    double dist = line.Distance(pos);
                    double decayCoeff = Math.Exp(-decay * dist * dist); // decay coeff is exp(-decay*dist^2)
                    result += decayCoeff * currentTensor; // consider formalize here?
                }
            }
            return result;
        }
    }

    // todo calculate height field from gradient 
    public class HeightTensorField : ITensorField
    {
        public void Combine(ITensorField field)
        {
            throw new NotImplementedException();
        }

        public void DrawDebugShape()
        {
            throw new NotImplementedException();
        }

        public double decay { get; set; }

        public Tensor SampleAtPos(Coordinate pos)
        {
            throw new NotImplementedException();
        }
    }

    public class AddedField : ITensorField
    {
        private List<ITensorField> fields;

        public double decay { get; set; }

        public AddedField(IEnumerable<ITensorField> _fields)
        {
            fields = _fields.ToList();
        }
        public void Combine(ITensorField field)
        {
            throw new NotImplementedException();
        }

        public void DrawDebugShape()
        {
            foreach (var f in fields)
            {
                try
                {
                    f.DrawDebugShape();
                }
                catch (Exception e)
                {
                    // do nothing
                    Debug.LogWarning(e.Message);
                }
            }
        }

        public Tensor SampleAtPos(Coordinate pos)
        {
            Tensor tensor = new Tensor(0, 0);

            foreach (var field in fields)
            {
                tensor += field.SampleAtPos(pos);
            }
            return tensor;
        }
    }

    public static class Extension
    {
        // todo combine two tensor fields
        public static ITensorField Combine(this ITensorField first, ITensorField second)
        {
            throw new NotImplementedException();
        }

        public static Coordinate Add(this Coordinate first, Coordinate second)
        {
            return new Coordinate(first.X + second.X, first.Y + second.Y);
        }

        public static Coordinate Reverse(this Coordinate vec)
        {
            return new Coordinate(-vec.X, -vec.Y);
        }

        public static Coordinate Multiplication(this Coordinate vec, double multiply)
        {
            return new Coordinate(multiply * vec.X, multiply * vec.Y);
        }

        public static Coordinate Normalize(this Coordinate vec)
        {
            double sqrtSum = Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
            return vec.Multiplication(1 / sqrtSum);
        }

        public static EigenType SwitchEigen(EigenType eigen)
        {
            return eigen == EigenType.Major ? EigenType.Minor : EigenType.Major;
        }
    }
}

