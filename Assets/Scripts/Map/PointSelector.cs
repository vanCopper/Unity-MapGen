// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 18:43:24
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using csDelaunay;

public class PointSelector
{
    public static int NUM_LLOYD_RELAXATIONS = 2;

    public static bool needsMoreRandomness(PointType type)
    {
        return type == PointType.Square || type == PointType.Hexagon;
    }

    public static PointGen generateRandom(int size, uint seed)
    {
        List<Vector2f> generate(int numPoint)
        {
            ParkMillerRNG mapRandom = new ParkMillerRNG();
            mapRandom.Seed = seed;
            List<Vector2f> points = new List<Vector2f>();
            for(int i = 0; i < numPoint; i++)
            {
                double mx = mapRandom.NextDoubleRange(10, size - 10);
                double my = mapRandom.NextDoubleRange(10, size - 10);
                Vector2f p = new Vector2f(mx,my);
                points.Add(p);
            }
            return points;
        }

        PointGen pGen = generate;
        return pGen;
    }

    public static PointGen generateRelaxed(int size, uint seed)
    {
        List<Vector2f> generate(int numPoint)
        {
            Voronoi voronoi;
            List<Vector2f> region = new List<Vector2f>();
            List<Vector2f> points = generateRandom(size, seed)(numPoint);
            for(int i = 0; i < NUM_LLOYD_RELAXATIONS; i++)
            {
                voronoi = new Voronoi(points, new Rectf(0, 0, size, size));
                for(int j = 0; j < points.Count; j++)
                {
                    Vector2f p = points[j];
                    region = voronoi.Region(p);
                    p.x = 0.0f;
                    p.y = 0.0f;
                    
                    for(int k = 0; k < region.Count; k++)
                    {
                        Vector2f q = region[k];
                        p.x += q.x;
                        p.y += q.y;
                        region.Clear();
                    }
                }
                voronoi.Dispose();
            }

            return points;
        }

        PointGen pGen = generate;
        return pGen;
    }

    public static PointGen generateSquare(int size, uint seed)
    {
        List<Vector2f> generate(int numPoint)
        {
            List<Vector2f> points = new List<Vector2f>();
            int N = (int)Mathf.Sqrt(numPoint);
            for(int x = 0; x < N; x++)
            {
                for(int y = 0; y < N; y++)
                {
                    points.Add(new Vector2f((0.5f + x)/N * size, (0.5 + y)/N * size));
                }
            }
            return points;
        }

        PointGen pGen = generate;
        return pGen;
    }

    public static PointGen generateHexagon(int size, uint seed)
    {
        List<Vector2f> generate(int numPoint)
        {
            List<Vector2f> points = new List<Vector2f>();
            int N = (int)Mathf.Sqrt(numPoint);
            for(int x = 0; x < N; x++)
            {
                for(int y = 0; y < N; y++)
                {
                    points.Add(new Vector2f((0.5 + x)/N * size, (0.25+0.5*x%2+y)/N * size));
                }
            }
            return points;
        }

        PointGen pGen = generate;
        return pGen;
    }
}
