// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 18:43:24
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PointSelector
{
    public static int NUM_LLOYD_RELAXATIONS = 2;

    public static bool needsMoreRandomness(PointType type)
    {
        return type == PointType.Square || type == PointType.Hexagon;
    }

    public static PointGen generateRandom(int size, int seed)
    {
        PointGen pGen = generate;

        List<Vector2f> generate(int numPoint)
        {
            //TODO:
            return new List<Vector2f>();
        }

        return pGen;
    }

    public static PointGen generateRelaxed(int size, int seed)
    {
        PointGen pGen = generate;
        
        List<Vector2f> generate(int numPoint)
        {
            //TODO:
            return new List<Vector2f>();
        }

        return pGen;
    }

    public static PointGen generateSquare(int size, int seed)
    {
        PointGen pGen = generate;
        
        List<Vector2f> generate(int numPoint)
        {
            //TODO:
            return new List<Vector2f>();
        }

        return pGen;
    }

    public static PointGen generateHexagon(int size, int seed)
    {
        PointGen pGen = generate;

        List<Vector2f> generate(int numPoint)
        {
            //TODO;
            return new List<Vector2f>();
        }

        return pGen;
    }
}
