using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGen : MonoBehaviour
{
    private Map m_Map;
    private int m_MapSize = 256;

    //private string m_IslandType = "Perlin";
    private static uint m_IslandSeedInitial = 85882;
    // Point distribution
    //private string m_PointType = "Relaxed";
    private int m_NumPoints = 2000;
    // Start is called before the first frame update
    void Start()
    {
        m_Map = new Map(m_MapSize);
        m_Map.NewIsLand(IsLandShapeType.Perlin, PointType.Square, m_NumPoints, m_IslandSeedInitial, 0);
        m_Map.Reset();
        m_Map.MapGen();
        List<Vector2f> points =  m_Map.Points;
        Debug.Log(points);

        Image image = GetComponent<Image>();
        Canvas canvas = GetComponent<Canvas>();
        //image.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
