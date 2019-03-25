// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 15:32:45
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum IsLandShapeType // 地图的形状
{
    Radial, //ParkMiller
    Perlin, // Prelin 噪音
    Square  // 
}

public enum PointType   // 生成点的分布类型
{
    Random,
    Relaxed,
    Square,
    Hexagon
}

public delegate bool IsLandShapeGen(Vector2f q);
public delegate List<Vector2f> PointGen(int numPoints);

public class Map
{
    public static float LAKE_THRESHOLD = 0.3f;

    public int MapSize;
    public ParkMillerRNG ParkMillerRng = new ParkMillerRNG();
    public bool NeedsMoreRandomness;

    public int NumPoints;

    public List<Vector2f> Points;
    public List<Center> Centers;
    public List<Corner> Corners;
    public List<Edge> Edges;


    public IsLandShapeGen IslandShapeGen;
    public PointGen PointSelectorGen;

    public Map(int size)
    {
        MapSize = size;
        NumPoints = 1;

        Reset();
    }

    public void Reset()
    {
        if (Points != null) Points.Clear();

        if (Centers != null)
        {
            Centers.RemoveAll((Center center)=>
            {
                center.Clear();
                return true;
            });
        }

        if(Corners != null)
        {
            Corners.RemoveAll((Corner corner)=> {
                corner.Clear();
                return true;
            });
        }

        if(Edges != null)
        {
            Edges.RemoveAll((Edge edge)=> {
                edge.Clear();
                return true;
            });
        }

        if (Points == null) Points = new List<Vector2f>();
        if (Edges == null) Edges = new List<Edge>();
        if (Corners == null) Corners = new List<Corner>();
        if (Centers == null) Centers = new List<Center>();
    }

    public void NewIsLand(IsLandShapeType islandType, PointType pointType, int numPoints, int seed, uint variant )
    {
        switch(islandType)
        {
            case IsLandShapeType.Perlin:
                IslandShapeGen = IsLandShape.MakePerlin(seed);
                break;
            case IsLandShapeType.Radial:
                IslandShapeGen = IsLandShape.MakeRadial(seed);
                break;
            case IsLandShapeType.Square:
                IslandShapeGen = IsLandShape.MakeSquare(seed);
                break;
            default:
                break;
        }

        switch(pointType)
        {
            case PointType.Random:
                PointSelectorGen = PointSelector.generateRandom(MapSize, seed);
                break;
            case PointType.Relaxed:
                PointSelectorGen = PointSelector.generateRelaxed(MapSize, seed);
                break;
            case PointType.Square:
                PointSelectorGen = PointSelector.generateSquare(MapSize, seed);
                break;
            case PointType.Hexagon:
                PointSelectorGen = PointSelector.generateHexagon(MapSize, seed);
                break;
            default:
                break;
        }

        NeedsMoreRandomness = PointSelector.needsMoreRandomness(pointType);
        NumPoints = numPoints;
        ParkMillerRng.Seed = variant;
    }
}
