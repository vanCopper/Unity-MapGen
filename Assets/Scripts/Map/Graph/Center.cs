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
    public bool Oceam;          // 海洋
    public bool Coast;          // 海岸线
    public bool Border;         // 是否为地图边缘
    public string Biome;        // 生态群类型
    public float Elevation;     // 海拔 0.0 - 1.0
    public float Moisture;      // 湿度 0.0 - 1.0

    public List<Center> Neighbors;
    public List<Edge> Borders;
    public List<Corner> Corners;

    public void Clear()
    {
        if (Neighbors != null) Neighbors.Clear();
        if (Borders != null) Borders.Clear();
        if (Corners != null) Corners.Clear();
    }
}
