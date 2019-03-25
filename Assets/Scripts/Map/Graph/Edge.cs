// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 17:06:48
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Edge
{
    public int Index;
    public Center d0;   // Delaunay edge
    public Center d1;   // Delaunay edge
    public Corner v0;   // Voronoi edge
    public Corner v1;   // Voronoi edge

    public Vector2f MidPoint; // v0, v1的中点
    public int River;         // 水流大小

    public void Clear()
    {
        d0 = null;
        d1 = null;
        v0 = null;
        v1 = null;
    }
}
