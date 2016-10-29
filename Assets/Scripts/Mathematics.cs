using System;
using UnityEngine;

public static class Mathematics
{
    public static System.Random random = new System.Random();

    public static void SolveQuadraticEquation(double a, double b, double c, out double x1, out double x2)
    {
        //Quadratic Formula: x = (-b +- sqrt(b^2 - 4ac)) / 2a

        //Calculate the inside of the square root
        double insideSquareRoot = (b * b) - 4 * a * c;

        if (insideSquareRoot < 0)
        {
            //There is no solution
            x1 = double.NaN;
            x2 = double.NaN;
        }
        else
        {
            //Compute the value of each x
            //if there is only one solution, both x's will be the same
            double sqrt = Math.Sqrt(insideSquareRoot);
            x1 = (-b + sqrt) / (2 * a);
            x2 = (-b - sqrt) / (2 * a);
        }
    }

    public static void GeneralLinearEquationCoefficients(Vector2 p1, Vector2 p2, out float a, out float b, out float c)
    {
        // General Form Of Linear Equation:
        //(y1 - y2) * x + (x2 - x1) * y + (x1 * y2 - x2 * y1) = 0

        a = p1.y - p2.y;
        b = p2.x - p1.y;
        c = p1.x * p2.y - p2.x * p1.y;
    }

    public static Vector2 Vector2Rotate90Clockwise(Vector2 v)
    {
        return new Vector2(v.y, -v.x);
    }

    public static Vector2 Vector3ToVector2xz(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static double SignedAngleRadians(Vector2 v1, Vector2 v2)
    {
        return Math.Atan2(v1.x, v1.y) - Math.Atan2(v2.x, v2.y);
    }

    public static double SignedAngleDegrees(Vector2 v1, Vector2 v2)
    {
        return RadiansToDegrees(SignedAngleRadians(v1, v2));
    }

    #region AnglesConversion
    public static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    public static double RadiansToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
    #endregion AnglesConversion

    public static bool Probability(double chance)
    {
        return random.NextDouble() >= 1 - chance;
    }
}
