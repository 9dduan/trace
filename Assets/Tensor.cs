using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Algorithm;
using UnityEngine;
using MathNet;
using MathNet.Numerics;

public interface ITensorSolver
{
    double[] GetEigenValue(Tensor tensor);
    Coordinate[] GetEigenVectors(Tensor tensor);
}

public class AlgebraTensorSolver:ITensorSolver
{
    private double epsilon = 0.000001f;

    //private Coordinate coord = new Coordinate(0, 0);

    public double[] GetEigenValue(Tensor tensor)
    {
        double eigenVal = Math.Sqrt(tensor.A * tensor.A + tensor.B * tensor.B);
        if (eigenVal < double.Epsilon)
        {
            return new double[2] { 0, 0 };
        }
        return new double[2] {eigenVal,-eigenVal};
    }

    public Coordinate[] GetEigenVectors(Tensor tensor)
    {
        var eigenValues = GetEigenValue(tensor);

        if (tensor.B < epsilon)
        {
            return new Coordinate[2] {
                new Coordinate(1, 0),
                new Coordinate(0, 1) };
        }
        else
        {
            return new Coordinate[2] {
                new Coordinate(tensor.B, eigenValues[0] - tensor.A),
                new Coordinate(tensor.B, eigenValues[1] - tensor.A)};
        }
    }
}

// use mathnet.numerics to solve matrix
public class NumericTensorSolver : ITensorSolver
{
    public double[] GetEigenValue(Tensor tensor)
    {
        
        throw new NotImplementedException();
    }

    public Coordinate[] GetEigenVectors(Tensor tensor)
    {
        throw new NotImplementedException();
    }
}

public struct Tensor
{
    // tensor is a matrix with the form of:
    //
    // A  B     cos(2*theta) sin(2*theta)
    //       or     
    // B -A     sin(2*theta) -cos(2*theta)
    //
    
    public readonly double A;
    public readonly double B;

    public Tensor(double a,double b)
    {
        A = a;
        B = b;
    }

    // create tensor from r and theta
    public static Tensor FromRTheta(double r, double theta)
    {
        var a = Math.Cos(2 * theta);
        var b = Math.Sin(2 * theta);
        return new Tensor(r*a,r*b); // theta could be angle of grid field
    }

    // create tensor from relative x and relative y
    public static Tensor FromXY(double x, double y)
    {
        double doublexy = -2*x*y;
        double diffSquares = y*y - x*x;
        return new Tensor(diffSquares,doublexy).Normalize(); // return a normalized tensor so that a huge xy won't make a huge eigen value
    }

    public static Tensor FromXY(Coordinate vec)
    {
        return FromXY(vec.X, vec.Y);
    }

    public Tensor Normalize()
    {
        if(Math.Abs(A) < float.Epsilon && Math.Abs(B) < float.Epsilon)
        {
            return new Tensor(0, 0);
        }

        double l = Math.Sqrt(A * A + B * B);

        if (l < float.Epsilon)
        {
            return new Tensor(0, 0);
        }

        double normalizedA = A / l;
        double normalizedB = B / l;
        return new Tensor(normalizedA,normalizedB);
    }

    public static Tensor operator+(Tensor left, Tensor right)
    {
        return new Tensor(left.A + right.A, left.B + right.B);
    }

    public static Tensor operator*(double left, Tensor right)
    {
        return new Tensor(left*right.A, left*right.B);
    }

    public void EigenValues(out double major, out double minor)
    // which is major and minor doest matter in this case
    {
        var eval = Math.Sqrt(A * A + B * B);

        major = eval;
        minor = -eval;
    }

    public void EigenVectors(out Coordinate major, out Coordinate minor)
    {
        if (Math.Abs(B) < 0.0000001f)
        {
            if (Math.Abs(A) < 0.0000001f)
            {
                major = new Coordinate(0,0);
                minor = new Coordinate(0,0);
            }
            else
            // consider what is major and what is minor in this case
            {
                if (A > 0)
                {
                    major = new Coordinate(1, 0);
                    minor = new Coordinate(0, 1);
                }
                else
                {
                    major = new Coordinate(0, 1);
                    minor = new Coordinate(1, 0);
                }
                
            }
        }
        else
        {
            double e1, e2;
            EigenValues(out e1, out e2);

            major = new Coordinate((float)B, (float)(e1 - A));
            minor = new Coordinate((float)B, (float)(e2 - A));
        }
    }

    public void EigenVectors(EigenType eigenType,out Coordinate Selected)
    {
        Coordinate major;
        Coordinate minor;
        EigenVectors(out major, out minor);
        Selected = eigenType == EigenType.Major ? major : minor;
    }
}
