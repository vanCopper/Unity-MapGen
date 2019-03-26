// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 17:07:06
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Corner
{
    public int Index;
    public Vector2f Point;      // 位置
    public bool Water;          // 湖或者海洋
    public bool Ocean;          // 海洋
    public bool Coast;          // 海岸线
    public bool Border;         // 是否为地图边缘
    public string Biome;        // 生态群类型
    public double Elevation;     // 海拔 0.0 - 1.0
    public double Moisture;      // 湿度 0.0 - 1.0

    public List<Center> Touches;
    public List<Edge> Protrudes;
    public List<Corner> Adjacent;

    public int River;   //0不是河流/ 河流的大小
    public Corner Downslope;    // 水流方向的邻角
    public Corner Watershed;    // 靠近海洋的邻角
    public int WatershedSize;   //

    public void Clear()
    {
        if (Touches != null) Touches.Clear();
        if (Protrudes != null) Protrudes.Clear();
        if (Adjacent != null) Adjacent.Clear();

        Downslope = null;
        Watershed = null;
    }
}
