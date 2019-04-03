using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGen : MonoBehaviour
{
    private Material m_DebugMat;
    private Map m_Map;
    private int m_MapSize = 512;
    private List<Vector2f> m_Points;
    private List<Center> m_Centers;

    //private string m_IslandType = "Perlin";
    private static uint m_IslandSeedInitial = 85882;
    // Point distribution
    //private string m_PointType = "Relaxed";
    private int m_NumPoints = 1000;
    // Start is called before the first frame update
    void Start()
    {
        m_Map = new Map(m_MapSize);
        m_Map.NewIsLand(IsLandShapeType.Perlin, PointType.Random, m_NumPoints, m_IslandSeedInitial, 0);
        m_Map.Reset();
        m_Map.MapGen();
        m_Points =  m_Map.Points;
        m_Centers = m_Map.Centers;
        Debug.Log(m_Points);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPostRender()
    {
        //GL
        if (m_Points != null && m_Points.Count != 0)
        {
            GLHelper.InitDebugMat();

            foreach (Vector2f p in m_Points)
            {
                GLHelper.DrawCircle(p.x, p.y, 0, 1f, 0.5f, Color.red);
            }
        }

        if(m_Centers != null && m_Centers.Count != 0)
        {
            GLHelper.InitDebugMat();
            GL.LoadOrtho();
            GL.PushMatrix();
            foreach (Center c in m_Centers)
            {
                foreach(Edge edge in c.Borders)
                {
                    if(edge.v0 != null && edge.v1 != null)
                    {
                        GLHelper.DrawLine(new Vector3(edge.v0.Point.x, edge.v0.Point.y, 0), new Vector3(edge.v1.Point.x, edge.v1.Point.y, 0), Color.green);
                    }
                }
                GLHelper.DrawCircle(c.Point.x, c.Point.y, 0, 1f, 0.5f, Color.red);

                foreach(Corner corner in c.Corners)
                {
                    GLHelper.DrawCircle(corner.Point.x, corner.Point.y, 0, 1.5f, 0.5f, Color.white);
                }
            }
            GL.PopMatrix();
            //m_Centers = null;
        }

    }
}
