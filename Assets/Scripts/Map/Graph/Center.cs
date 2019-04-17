// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 17:00:09
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Center
{

    public int Index;
    public Vector2f Point;      // 位置
    public bool Water;          // 湖或者海洋
    /// <summary>
    /// 海洋
    /// </summary>
    public bool Ocean;          
    /// <summary>
    /// 海岸线
    /// </summary>
    public bool Coast;          
    public bool Border;         // 是否为地图边缘
    public string Biome;        // 生态群类型
    public double Elevation;     // 海拔 0.0 - 1.0
    public double Moisture;      // 湿度 0.0 - 1.0

    public List<Center> Neighbors;
    public List<Edge> Borders;
    public List<Corner> Corners;

    public void Clear()
    {
        if (Neighbors != null) Neighbors.Clear();
        if (Borders != null) Borders.Clear();
        if (Corners != null) Corners.Clear();
    }

    public Color ElevationColor()
    {
        float g = (float)Elevation;
        return new Color(g, g, g);
    }

    public Color MoistureColor()
    {
        float g = (float)Moisture;
        if(Mathf.Approximately(g, 1.0f))
        {
            return new Color(0.2f, 0.2f, 0.4f);
        }
        return new Color(1.0f - g, 1.0f, 1.0f - g);
    }
}
