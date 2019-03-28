using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGen : MonoBehaviour
{
    private Material m_DebugMat;
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

        Shader shader = Shader.Find("Hidden/Internal-Colored");

        m_DebugMat = new Material(shader);
        m_DebugMat.hideFlags = HideFlags.HideAndDontSave;
        m_DebugMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m_DebugMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m_DebugMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        m_DebugMat.SetInt("_ZWrite", 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPostRender()
    {
        //GL
        m_DebugMat.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.LINES);

        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0.1f, 0.1f, 0);

        GL.End();
        GL.PopMatrix();
    }
}
