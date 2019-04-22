using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System;

public class MapGen : MonoBehaviour
{
    private static Dictionary<string, Color> DisplayColor = new Dictionary<string, Color>();
    private static Dictionary<string, string> DebugBiomes = new Dictionary<string, string>();
    private Material m_DebugMat;
    private Map m_Map;
    private int m_MapSize = 512;
    private List<Vector2f> m_Points;
    private List<Center> m_Centers;

    //private string m_IslandType = "Perlin";
    private static uint m_IslandSeedInitial = 85882;
    private static uint m_Variant = 8;
    // Point distribution
    //private string m_PointType = "Relaxed";
    private int m_NumPoints = 1000;
    // Start is called before the first frame update
    private void Awake()
    {
        // Features
        DisplayColor.Add("OCEAN", ParseColor("#44447a"));      //海洋
        DisplayColor.Add("COAST", ParseColor("#33335a"));      //海岸
        DisplayColor.Add("LAKESHORE", ParseColor("#225588"));  //湖岸
        DisplayColor.Add("LAKE", ParseColor("#336699"));       //湖泊
        DisplayColor.Add("RIVER", ParseColor("#225588"));      //河
        DisplayColor.Add("MARSH", ParseColor("#2f6666"));      //沼泽
        DisplayColor.Add("ICE", ParseColor("#99ffff"));        //冰
        DisplayColor.Add("BEACH", ParseColor("#a09077"));      //海滩
        DisplayColor.Add("ROAD1", ParseColor("#442211"));      //道路 1
        DisplayColor.Add("ROAD2", ParseColor("#664433"));      //道路 2
        DisplayColor.Add("ROAD3", ParseColor("#225588"));      //道路 3
        DisplayColor.Add("BRIDGE", ParseColor("#686860"));     //桥
        DisplayColor.Add("LAVA", ParseColor("#cc3333"));       //熔岩

        DebugBiomes.Add("OCEAN", "a");      //海洋
        DebugBiomes.Add("COAST", "b");      //海岸
        DebugBiomes.Add("LAKESHORE", "c");  //湖岸
        DebugBiomes.Add("LAKE", "d");       //湖泊
        DebugBiomes.Add("RIVER", "e");      //河
        DebugBiomes.Add("MARSH", "f");      //沼泽
        DebugBiomes.Add("ICE", "g");        //冰
        DebugBiomes.Add("BEACH", "h");      //海滩
        DebugBiomes.Add("ROAD1", "i");      //道路 1
        DebugBiomes.Add("ROAD2", "j");      //道路 2
        DebugBiomes.Add("ROAD3", "k");      //道路 3
        DebugBiomes.Add("BRIDGE", "l");     //桥
        DebugBiomes.Add("LAVA", "m");       //熔岩

        //Terrain
        DisplayColor.Add("SNOW", ParseColor("#ffffff"));       //雪
        DisplayColor.Add("TUNDRA", ParseColor("#bbbbaa"));     //冻土地带
        DisplayColor.Add("BARE", ParseColor("#888888"));       
        DisplayColor.Add("SCORCHED", ParseColor("#555555"));   //烧焦
        DisplayColor.Add("TAIGA", ParseColor("#99aa77"));      //针叶树林地带
        DisplayColor.Add("SHRUBLAND", ParseColor("#889977"));  //灌木丛
        DisplayColor.Add("TEMPERATE_DESERT", ParseColor("#c9d29b"));       //温带森林
        DisplayColor.Add("TEMPERATE_RAIN_FOREST", ParseColor("#448855"));  //温带雨林
        DisplayColor.Add("TEMPERATE_DECIDUOUS_FOREST", ParseColor("#679459")); //温带落叶林
        DisplayColor.Add("GRASSLAND", ParseColor("#88aa55"));                  //草原
        DisplayColor.Add("SUBTROPICAL_DESERT", ParseColor("#d2b98b"));         //亚热带沙漠
        DisplayColor.Add("TROPICAL_RAIN_FOREST", ParseColor("#337755"));       //热带雨林
        DisplayColor.Add("TROPICAL_SEASONAL_FOREST", ParseColor("#559944"));   //热带季节性森林

        DebugBiomes.Add("SNOW", "n");       //雪
        DebugBiomes.Add("TUNDRA", "o");     //冻土地带
        DebugBiomes.Add("BARE", "p");
        DebugBiomes.Add("SCORCHED", "q");   //烧焦
        DebugBiomes.Add("TAIGA", "r");      //针叶树林地带
        DebugBiomes.Add("SHRUBLAND", "s");  //灌木丛
        DebugBiomes.Add("TEMPERATE_DESERT", "t");       //温带森林
        DebugBiomes.Add("TEMPERATE_RAIN_FOREST", "u");  //温带雨林
        DebugBiomes.Add("TEMPERATE_DECIDUOUS_FOREST", "v"); //温带落叶林
        DebugBiomes.Add("GRASSLAND", "w");                  //草原
        DebugBiomes.Add("SUBTROPICAL_DESERT", "x");         //亚热带沙漠
        DebugBiomes.Add("TROPICAL_RAIN_FOREST", "y");       //热带雨林
        DebugBiomes.Add("TROPICAL_SEASONAL_FOREST", "z");   //热带季节性森林
    }
    void Start()
    {
        GLHelper.InitDebugMat();
        m_Map = new Map(m_MapSize);
        m_Map.NewIsLand(IsLandShapeType.Radial, PointType.Relaxed, m_NumPoints, m_IslandSeedInitial, m_Variant);
        m_Map.Reset();
        m_Map.MapGen();
        m_Map.AssignBiomes();
        m_Points = m_Map.Points;
        m_Centers = m_Map.Centers;
        //Debug.Log(m_Points);
        string[,] gridTypes = new string[256, 256];
        string result = "";
        for(int i = 0; i < 256; i++)
        {
            result += Environment.NewLine;
            for (int j = 0; j < 256; j++)
            {
                gridTypes[i, j] = GetGridType(i, j);
                //result += DebugBiomes[gridTypes[i, j]];
            }
        }

        Debug.Log(result);
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    private void OnPostRender()
    {
        //GL
        //GL.Clear(true, true, Color.black);
        GL.LoadOrtho();
        GLHelper.m_DebugMat.SetPass(0);
        GL.PushMatrix();
       

        if (m_Points != null && m_Points.Count != 0)
        {
            foreach (Vector2f p in m_Points)
            {
                //GLHelper.DrawCircle(p.x, p.y, 0, 1f, 0.5f, Color.red);
            }
        }

        if (m_Centers != null && m_Centers.Count != 0)
        {
            foreach (Center c in m_Centers)
            {
                
                List<Vector3> triangles = new List<Vector3>();
                foreach (Edge edge in c.Borders)
                {
                    if (edge.v0 != null && edge.v1 != null)
                    {
                        if(edge.River > 0)
                        {
                            //GLHelper.DrawLine(new Vector3(edge.v0.Point.x, edge.v0.Point.y, 0), 
                            //    new Vector3(edge.v1.Point.x, edge.v1.Point.y, 0), DisplayColor["RIVER"]);

                            GLHelper.DrawLineWithThickness(new Vector3(edge.v0.Point.x, edge.v0.Point.y, 0),
                                new Vector3(edge.v1.Point.x, edge.v1.Point.y, 0), DisplayColor["RIVER"], 0.0025f);
                        }
                        else
                        {
                            GLHelper.DrawLine(new Vector3(edge.v0.Point.x, edge.v0.Point.y, 0), new Vector3(edge.v1.Point.x, edge.v1.Point.y, 0), Color.black);
                        }
                        triangles.Add(new Vector3(c.Point.x, c.Point.y, 0));
                        triangles.Add(new Vector3(edge.v0.Point.x, edge.v0.Point.y, 0));
                        triangles.Add(new Vector3(edge.MidPoint.x, edge.MidPoint.y, 0));

                        triangles.Add(new Vector3(c.Point.x, c.Point.y, 0));
                        triangles.Add(new Vector3(edge.v1.Point.x, edge.v1.Point.y, 0));
                        triangles.Add(new Vector3(edge.MidPoint.x, edge.MidPoint.y, 0));
                    }

                    // 渲染三角形
                    //if(edge.d0 != null && edge.d1 != null)
                    //{
                    //    GLHelper.DrawLine(new Vector3(edge.d0.Point.x, edge.d0.Point.y, 0), new Vector3(edge.d1.Point.x, edge.d1.Point.y, 0), Color.black);
                    //}
                }

                //Debug.LogFormat("{0}_{1}", c.Moisture, c.Biome);
                //GLHelper.DrawCircle(c.Point.x, c.Point.y, 0, 1f, 0.5f, c.ElevationColor());
                foreach (Corner corner in c.Corners)
                {
                    //GLHelper.DrawCircle(corner.Point.x, corner.Point.y, 0, 1.5f, 0.5f, Color.white);
                }

                Color pColor = Color.white;
                if(c.Biome == null)
                {
                    if(c.Ocean)
                    {
                        pColor = DisplayColor["OCEAN"];
                    }else if(c.Water)
                    {
                        pColor = DisplayColor["RIVER"];
                    }
                }
                else { DisplayColor.TryGetValue(c.Biome, out pColor); }

                // 渲染多边形
                //pColor = c.MoistureColor(); // 海拔图
                GLHelper.DrawTriangles(triangles, pColor);
            }
        }
        
        GL.PopMatrix();

    }

    private static Color ParseColor(string strColor)
    {
        Color result = Color.white;
        ColorUtility.TryParseHtmlString(strColor, out result);
        return result;
    }

    public string GetGridType(float gridX, float gridY)
    {
        float dist = int.MaxValue;
        Center closeCenter = new Center();
        closeCenter.Biome = "OCEAN";
        // walk over each biomeInfo
        for (int z = 0; z < m_Map.Centers.Count; z++)
        {

            float xdiff = m_Map.Centers[z].Point.x - gridX;
            float ydiff = m_Map.Centers[z].Point.y - gridY;

            float cdist = xdiff * xdiff + ydiff * ydiff;

            if (cdist < dist)
            {
                closeCenter = m_Map.Centers[z];
                dist = cdist;
            }
        }
        return closeCenter.Biome;
    }
}
