using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGen : MonoBehaviour
{
    private Material m_DebugMat;
    private Map m_Map;
    private int m_MapSize = 256;
    private List<Vector2f> points;

    //private string m_IslandType = "Perlin";
    private static uint m_IslandSeedInitial = 85882;
    // Point distribution
    //private string m_PointType = "Relaxed";
    private int m_NumPoints = 2000;
    // Start is called before the first frame update
    void Start()
    {
        m_Map = new Map(m_MapSize);
        m_Map.NewIsLand(IsLandShapeType.Perlin, PointType.Random, m_NumPoints, m_IslandSeedInitial, 0);
        m_Map.Reset();
        m_Map.MapGen();
        points =  m_Map.Points;
        Debug.Log(points);

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPostRender()
    {
        //GL
        if (points == null || points.Count == 0) return;

        GLHelper.InitDebugMat();

        foreach(Vector2f p in points)
        {
            GLHelper.DrawCircle(p.x, p.y, 0, 1f, 0.1f);
        }
    }
}
