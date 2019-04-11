// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 15:32:45
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using csDelaunay;

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
    public static float LAKE_THRESHOLD = 0.1f;

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

    public void MapGen()
    {
        Points = PointSelectorGen(NumPoints);

        for(int i = 0; i < 2; i++)
        {
            Points = Voronoi.RelaxPoints(Points, new Rectf(0, 0, MapSize, MapSize));
        }
        Voronoi voronoi = new Voronoi(Points, new Rectf(0, 0, MapSize, MapSize));
        BuildGraph(Points, voronoi);
        //ImproveCorners();
        voronoi.Dispose();
        voronoi = null;
        Points = null;

        //////////////////////////////////////
        AssignCornerElevations();
        AssignOceanCoastAndLand();
        RedistributeElevations(landCorners(Corners));

        foreach (Corner q in Corners)
        {
            if(q.Ocean || q.Coast)
            {
                q.Elevation = 0;
            }
        }
        // Center的海拔值为所在多边形所有Corner海拔值的平均值
        AssignPolygonElevations();
        AssignBiomes();
        ///////////////////////////////////////
        
        CalculateDownslopes();
        CalculateWatersheds();
        CreateRivers();

        AssignCornerMoisture();
        RedistributeMoisture(landCorners(Corners));
        AssignPolygonMoisture();
        //////////////////////////////////////
        AssignBiomes();
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

    public void NewIsLand(IsLandShapeType islandType, PointType pointType, int numPoints, uint seed, uint variant )
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

    public void ImproveCorners()
    {
        List<Vector2f> newCorners = new List<Vector2f>(Corners.Count);
        Vector2f point;

        foreach(Corner q in Corners)
        {
            if(q.Border)
            {
                newCorners.Insert(q.Index, q.Point);
            }else
            {
                point = new Vector2f();
                foreach(Center r in q.Touches)
                {
                    point.x += r.Point.x;
                    point.y += r.Point.y;
                }

                point.x /= q.Touches.Count;
                point.y /= q.Touches.Count;
                newCorners.Insert(q.Index, point);
            }
        }

        for(int i = 0; i < Corners.Count; i++)
        {
            Corners[i].Point = newCorners[i];
        }

        foreach(Edge edge in Edges)
        {
            if(edge.v0 != null && edge.v1 != null)
            {
                //edge.MidPoint = new Vector2f((edge.v1.Point.x - edge.v0.Point.x) * 0.5,
                //                             (edge.v1.Point.y - edge.v0.Point.y) * 0.5);
                edge.MidPoint = Map.Lerp(edge.v0.Point, edge.v1.Point, 0.5f);
            }
        }
    }

    public List<Corner> landCorners(List<Corner> corners)
    {
        List<Corner> locations = new List<Corner>();
        foreach(Corner q in corners)
        {
            if(!q.Ocean && !q.Coast)
            {
                locations.Add(q);
            }
        }
        return locations;
    }

    public void BuildGraph(List<Vector2f> points, Voronoi voronoi)
    {
        List<csDelaunay.Edge> libedges = voronoi.Edges;
        Dictionary<Vector2f, Center> centerLookup = new Dictionary<Vector2f, Center>();

        foreach(Vector2f point in points)
        {
            Center p = new Center();
            p.Index = Centers.Count;
            p.Point = point;
            p.Neighbors = new List<Center>();
            p.Borders = new List<Edge>();
            p.Corners = new List<Corner>();
            Centers.Add(p);
            centerLookup.Add(point, p);
        }

        foreach(Center tp in Centers)
        {
            voronoi.Region(tp.Point);
        }

        Dictionary<int, List<Corner>> _cornerMap = new Dictionary<int, List<Corner>>();

        Corner makeCorner(Vector2f point)
        {
            Corner q = null;
            if (point == null) return q;

            int bucket;
            for (bucket = (int)point.x - 1; bucket <= (int)point.x + 1; bucket++)
            {
                List<Corner> corners;
                if(_cornerMap.TryGetValue(bucket, out corners))
                {
                    for(int j = 0; j < corners.Count; j++)
                    {
                        q = corners[j];
                        float dx = point.x - q.Point.x;
                        float dy = point.y - q.Point.y;
                        if(dx*dx + dy*dy < 0.000001f)
                        {
                            return q;
                        }
                    }
                }
            }

            bucket = (int)point.x;
            if(!_cornerMap.ContainsKey(bucket))
            {
                _cornerMap.Add(bucket, new List<Corner>());
            }

            q = new Corner();
            q.Index = Corners.Count;
            Corners.Add(q);
            q.Point = point;
            q.Border = point.x == 0 || point.x == MapSize
                        || point.y == 0 || point.y == MapSize;
            q.Touches = new List<Center>();
            q.Protrudes = new List<Edge>();
            q.Adjacent = new List<Corner>();
            _cornerMap[bucket].Add(q);

            return q;
        }

        void addToCornerList(List<Corner> v, Corner x)
        {
            if (x != null && !v.Contains(x)) v.Add(x);
        }

        void addToCenterList(List<Center> v, Center x)
        {
            if (x != null && !v.Contains(x)) v.Add(x);
        }

        foreach(csDelaunay.Edge libedge in libedges)
        {
            LineSegment dedge = libedge.DelaunayLine();
            LineSegment vedge = libedge.VoronoiEdge();

            Edge edge = new Edge();
            edge.Index = Edges.Count;
            edge.River = 0;
            Edges.Add(edge);
            if (vedge.p0 != null && vedge.p1 != null)
            {
                //edge.MidPoint = new Vector2f((vedge.p0.x - vedge.p1.x) * 0.5,
                //(vedge.p0.y - vedge.p1.y) * 0.5);
                edge.MidPoint = Map.Lerp(vedge.p0, vedge.p1, 0.5f);
            }

            // 边对应的两个角
            edge.v0 = makeCorner(vedge.p0);
            edge.v1 = makeCorner(vedge.p1);

            // 穿过该边的三角形边对应的两个点
            Center d0Center;
            if(centerLookup.TryGetValue(dedge.p0, out d0Center))
            {
                edge.d0 = d0Center;
            }
            Center d1Center;
            if(centerLookup.TryGetValue(dedge.p1, out d1Center))
            {
                edge.d1 = d1Center;
            }

            //点对应的边，角对应的边
            if (edge.d0 != null) edge.d0.Borders.Add(edge);
            if (edge.d1 != null) edge.d1.Borders.Add(edge);
            if (edge.v0 != null) edge.v0.Protrudes.Add(edge);
            if (edge.v1 != null) edge.v1.Protrudes.Add(edge);
            
            if(edge.d0 != null && edge.d1 != null)
            {
                addToCenterList(edge.d0.Neighbors, edge.d1);
                addToCenterList(edge.d1.Neighbors, edge.d0);
            }

            if(edge.v0 != null && edge.v1 != null)
            {
                addToCornerList(edge.v0.Adjacent, edge.v1);
                addToCornerList(edge.v1.Adjacent, edge.v0);
            }

            if (edge.d0 != null)
            {
                addToCornerList(edge.d0.Corners, edge.v0);
                addToCornerList(edge.d0.Corners, edge.v1);
            }
            if (edge.d1 != null)
            {
                addToCornerList(edge.d1.Corners, edge.v0);
                addToCornerList(edge.d1.Corners, edge.v1);
            }

            // Corners point to centers
            if (edge.v0 != null)
            {
                addToCenterList(edge.v0.Touches, edge.d0);
                addToCenterList(edge.v0.Touches, edge.d1);
            }
            if (edge.v1 != null)
            {
                addToCenterList(edge.v1.Touches, edge.d0);
                addToCenterList(edge.v1.Touches, edge.d1);
            }
        }
    }

    /// <summary>
    /// 设置Voronoi corners的海拔值以及是否为水
    /// </summary>
    /// <returns></returns>
    public void AssignCornerElevations()
    {
        //List<Corner> queue = new List<Corner>();
        Stack<Corner> queue = new Stack<Corner>();

        foreach(Corner q in Corners)
        {
            q.Water = !Inside(q.Point);
        }

        foreach(Corner q in Corners)
        {
            if(q.Border)
            {
                q.Elevation = 0;    //地图边缘海拔值为 0
                queue.Push(q);      //地图边缘的角
            }else
            {
                q.Elevation = float.PositiveInfinity;
            }
        }
       // queue存储了边缘角
       while(queue.Count > 0)
        {
            Corner q = queue.Pop();

            foreach(Corner s in q.Adjacent) // 从边缘角通过相邻角向里收缩 
            {
                double newElevation = 0.01f + q.Elevation;
                if(!q.Water && !s.Water)
                {
                    newElevation += 1;
                    if(NeedsMoreRandomness)
                    {
                        // 如果是Square或Hexagon类型地图，海拔加入随机以确保后面河流的生成有足够的随机性
                        newElevation += ParkMillerRng.NextDouble();
                    }
                }

                if (newElevation < s.Elevation)
                {
                    s.Elevation = newElevation;
                    queue.Push(s);
                }
            }
        }
        
    }

    public void RedistributeElevations(List<Corner> locations)
    {
        double SCALE_FACTOR = 1.1;
        double y;
        double x;
        locations.Sort((a,b) => a.Elevation.CompareTo(b.Elevation));

        for(int i = 0; i < locations.Count; i++)
        {
            y = (double)i / (double)(locations.Count - 1);
            x = System.Math.Sqrt(SCALE_FACTOR) - System.Math.Sqrt(SCALE_FACTOR * (1-y));
            if (x > 1.0) x = 1.0;
            locations[i].Elevation = x;
        }
    }

    public void RedistributeMoisture(List<Corner> locations)
    {
        locations.Sort((a, b) => a.Moisture.CompareTo(b.Moisture));
        for(int i = 0; i<locations.Count; i++)
        {
            double moisture = (double)i / (double)(locations.Count - 1);
            locations[i].Moisture = moisture;
        }
    }

    /// <summary>
    /// 确定当前多边形是 海洋/海岸线/陆地
    /// </summary>
    public void AssignOceanCoastAndLand()
    {
        Stack<Center> queue = new Stack<Center>();
        int numWater;
        foreach(Center p in Centers)
        {
            numWater = 0;
            foreach(Corner q in p.Corners)
            {
                if(q.Border)// 边缘的Center均为海洋，Corner均为水
                {
                    p.Border = true;
                    p.Ocean = true;
                    q.Water = true;
                    queue.Push(p);
                }

                if(q.Water)
                {
                    numWater += 1;
                }
            }

            p.Water = (p.Ocean || numWater >= p.Corners.Count * LAKE_THRESHOLD);
        }

        while(queue.Count > 0)
        {
            Center p = queue.Pop();
            // 连接海洋节点的水节点 均为海洋
            foreach(Center r in p.Neighbors)
            {
                if(r.Water && !r.Ocean)
                {
                    r.Ocean = true;
                    queue.Push(r);
                }
            }
        }
       
        foreach(Center p in Centers)
        {
            int numOcena = 0;
            int numLand = 0;
            // 连接海洋节点和陆地节点的节点为海岸线
            foreach(Center r in p.Neighbors)
            {
                if (r.Ocean) numOcena++;
                if (!r.Water) numLand++;
            }
            p.Coast = (numOcena > 0) && (numLand > 0);
        }
       
        foreach(Corner c in Corners)
        {
            int numOcean = 0;
            int numLand = 0;
            foreach(Center p in c.Touches)
            {
                if (p.Ocean) numOcean++;
                if (!p.Water) numLand++;
            }
            // 设置Corner属性
            c.Ocean = (numOcean == c.Touches.Count);
            c.Coast = (numOcean > 0) && (numLand > 0);
            c.Water = c.Border || ((numLand != c.Touches.Count) && !c.Coast);
        }
    }

    public void AssignPolygonElevations()
    {
        double sumElevation;
        foreach(Center p in Centers)
        {
            sumElevation = 0.0;
            foreach(Corner q in p.Corners)
            {
                sumElevation += q.Elevation;
            }

            p.Elevation = sumElevation / (double)p.Corners.Count;
        }
    }

    public void CalculateDownslopes()
    {
        Corner r;
        foreach(Corner q in Corners)
        {
            r = q;
            foreach(Corner s in q.Adjacent)
            {
                if(s.Elevation <= r.Elevation)
                {
                    r = s;
                }
            }
            q.Downslope = r;
        }
    }

    public void CalculateWatersheds()
    {
        bool changed;
        Corner r;
        foreach(Corner q in Corners)
        {
            q.Watershed = q;
            if(!q.Ocean && !q.Coast)
            {
                q.Watershed = q.Downslope;
            }
        }
        //TODO: 100根据点数多少 可变
        for(int i = 0; i < 100; i++)
        {
            changed = false;
            foreach(Corner q in Corners)
            {
                if(!q.Ocean && !q.Coast && !q.Watershed.Coast)
                {
                    r = q.Downslope.Watershed;
                    if(!r.Ocean)
                    {
                        q.Watershed = r;
                        changed = true;
                    }
                }
            }
            if (!changed) break;
        }

        foreach(Corner q in Corners)
        {
            r = q.Watershed;
            r.WatershedSize = 1 + r.WatershedSize;
        }
    }

    public void CreateRivers()
    {
        Corner q;
        Edge edge;
        for(int i = 0; i < MapSize/2; i++)
        {
            q = Corners[(int)ParkMillerRng.NextIntRange(0, Corners.Count - 1)];
            if (q.Ocean || q.Elevation < 0.3 || q.Elevation > 0.9) continue;
            while (!q.Coast)
            {
                if (q == q.Downslope) break;

                edge = LookupEdgeFromCorner(q, q.Downslope);
                edge.River = edge.River + 1;
                q.River = q.River + 1;
                q.Downslope.River = q.Downslope.River + 1;
                q = q.Downslope;
            }
        }
    }

    public void AssignCornerMoisture()
    {
        Stack<Corner> queue = new Stack<Corner>();
        double newMoisture;
        foreach(Corner q in Corners)
        {
            if((q.Water || q.River > 0) && !q.Ocean)
            {
                q.Moisture = q.River > 0 ? Mathf.Min(3.0f, (0.2f * q.River)) : 1.0f;
                queue.Push(q);
            }else
            {
                q.Moisture = 0.0;
            }
        }
        
        while(queue.Count > 0)
        {
            Corner q = queue.Pop();
            foreach(Corner r in q.Adjacent)
            {
                newMoisture = q.Moisture * 0.9;
                if(newMoisture > r.Moisture)
                {
                    r.Moisture = newMoisture;
                    queue.Push(r);
                }
            }
        }
        
        foreach(Corner q in Corners)
        {
            if(q.Ocean || q.Coast)
            {
                q.Moisture = 1.0;
            }
        }
    }

    public void AssignPolygonMoisture()
    {
        double sumMoisture;
        foreach(Center p in Centers)
        {
            sumMoisture = 0.0;
            foreach(Corner q in p.Corners)
            {
                if (q.Moisture > 1.0) q.Moisture = 1.0;
                sumMoisture += q.Moisture;
            }
            p.Moisture = (double)(sumMoisture / (double)p.Corners.Count);
        }
    }

    public void AssignBiomes()
    {
        foreach(Center p in Centers)
        {
            p.Biome = GetBiome(p);
        }
    }

    public Edge LookupEdgeFromCenter(Center p, Center r)
    {
        foreach(Edge edge in p.Borders)
        {
            if (edge.d0 == r || edge.d1 == r) return edge;
        }
        return null;
    }

    public Edge LookupEdgeFromCorner(Corner q, Corner s)
    {
        foreach(Edge edge in q.Protrudes)
        {
            if (edge.v0 == s || edge.v1 == s) return edge;
        }
        return null;
    }

    public bool Inside(Vector2f p)
    {
        return IslandShapeGen(new Vector2f(2*(p.x/MapSize - 0.5f), 2*(p.y/MapSize - 0.5f)));
    }

    public static float Lerp(float a, float b, float by)
    {
        //return a * (1 - b) + b * by;
        return a + (b - a) * by;
    }

    public static Vector2f Lerp(Vector2f a, Vector2f b, float by)
    {
        float retX = Lerp(a.x, b.x, by);
        float retY = Lerp(a.y, b.y, by);
        return new Vector2f(retX, retY);
    }

    public static string GetBiome(Center p)
    {
        if (p.Ocean)
        {
            return "OCEAN";
        }
        else if (p.Water)
        {
            if (p.Elevation < 0.1) return "MARSH";
            if (p.Elevation > 0.8) return "ICE";
            return "LAKE";
        }
        else if (p.Coast)
        {
            return "BEACH";
        }
        else if (p.Elevation > 0.8)
        {
            if (p.Moisture > 0.50) return "SNOW";
            else if (p.Moisture > 0.33) return "TUNDRA";
            else if (p.Moisture > 0.16) return "BARE";
            else return "SCORCHED";
        }
        else if (p.Elevation > 0.6)
        {
            if (p.Moisture > 0.66) return "TAIGA";
            else if (p.Moisture > 0.33) return "SHRUBLAND";
            else return "TEMPERATE_DESERT";
        }
        else if (p.Elevation > 0.3)
        {
            if (p.Moisture > 0.83) return "TEMPERATE_RAIN_FOREST";
            else if (p.Moisture > 0.50) return "TEMPERATE_DECIDUOUS_FOREST";
            else if (p.Moisture > 0.16) return "GRASSLAND";
            else return "TEMPERATE_DESERT";
        }
        else
        {
            if (p.Moisture > 0.66) return "TROPICAL_RAIN_FOREST";
            else if (p.Moisture > 0.33) return "TROPICAL_SEASONAL_FOREST";
            else if (p.Moisture > 0.16) return "GRASSLAND";
            else return "SUBTROPICAL_DESERT";
        }
    }
}
 