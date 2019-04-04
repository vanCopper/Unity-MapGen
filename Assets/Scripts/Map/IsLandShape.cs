// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 18:04:13
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IsLandShape
{
    static public double ISLAND_FACTOR = 1.07;
    public static IsLandShapeGen MakeRadial(uint seed)
    {
        ParkMillerRNG isLandRandom = new ParkMillerRNG();
        isLandRandom.Seed = seed;
        uint bumps = isLandRandom.NextIntRange(1, 6);
        double startAngle = isLandRandom.NextDoubleRange(0, 2 * Mathf.PI);
        double dipAngle = isLandRandom.NextDoubleRange(0, 2 * Mathf.PI);
        double dipWidth = isLandRandom.NextDoubleRange(0.2, 0.7);
        
        bool inside(Vector2f q)
        {
            double angle = Mathf.Atan2(q.y, q.x);
            float max = Mathf.Max(Mathf.Abs(q.x), Mathf.Abs(q.y));
            float length = 0.5f * (max + q.magnitude);
            double f = bumps + 3.0f;
            double r1 = 0.5f + 0.40f * System.Math.Sin(startAngle + bumps * angle + System.Math.Cos(f * angle));
            double r2 = 0.7 - 0.20 * System.Math.Sin(startAngle + bumps * angle - System.Math.Sin((bumps + 2) * angle));

            if (System.Math.Abs(angle - dipAngle) < dipWidth
                || System.Math.Abs(angle - dipAngle + 2 * System.Math.PI) < dipWidth
                || System.Math.Abs(angle - dipAngle - 2 * System.Math.PI) < dipWidth)
            {
                r1 = r2 = 0.2;
            }
            return (length < r1 || (length > r1 * ISLAND_FACTOR && length < r2));
        }

        IsLandShapeGen islandShapeGen = inside;
        return islandShapeGen;
    }

    public static IsLandShapeGen MakePerlin(uint seed)
    {
        bool inside(Vector2f q)
        {
            float px = (q.x + 1) * 128;
            float py = (q.y + 1) * 128;

            double c = Mathf.PerlinNoise(px + seed, py + seed);
            return c > (0.3+0.3*q.magnitude*q.magnitude);
        }

        IsLandShapeGen islandShapeGen = inside;
        return islandShapeGen;
    }

    public static IsLandShapeGen MakeSquare(uint seed)
    {
        bool inside(Vector2f q)
        {
            return true;
        }
        IsLandShapeGen isLandShapeGen = inside;
        return isLandShapeGen;
    }


}
