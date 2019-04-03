// **********************************************************************
// Author: vanCopper
// Date: 2019/3/29 13:59:51
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GLHelper
{
    private static Material m_DebugMat;

    public static void InitDebugMat()
    {
        //if (m_DebugMat != null) return;
        Shader shader = Shader.Find("Hidden/Internal-Colored");

        m_DebugMat = new Material(shader);
        m_DebugMat.hideFlags = HideFlags.HideAndDontSave;
        m_DebugMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m_DebugMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m_DebugMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        m_DebugMat.SetInt("_ZWrite", 0);
        m_DebugMat.SetPass(0);
    }

    public static void DrawLine(Vector3 startPoint, Vector3 endPoint, Color color)
    {
        //GL.LoadOrtho();
        //GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(color);
        GL.Vertex(new Vector3(startPoint.x / Screen.width, startPoint.y / Screen.height, 0));
        GL.Vertex(new Vector3(endPoint.x / Screen.width, endPoint.y / Screen.height, 0));
        GL.End();
        //GL.PopMatrix();
    }

    public static void DrawCircle(float x, float y, float z, float r, float accuracy, Color color)
    {
        //GL.Clear(false, true, Color.blue);
        GL.PushMatrix();
        //绘制2D图像    
        GL.LoadOrtho();

        float stride = r * accuracy;
        float size = 1 / accuracy;
        float x1 = x, x2 = x, y1 = 0, y2 = 0;
        float x3 = x, x4 = x, y3 = 0, y4 = 0;

        double squareDe;
        squareDe = r * r - System.Math.Pow(x - x1, 2);
        squareDe = squareDe > 0 ? squareDe : 0;
        y1 = (float)(y + System.Math.Sqrt(squareDe));
        squareDe = r * r - System.Math.Pow(x - x1, 2);
        squareDe = squareDe > 0 ? squareDe : 0;
        y2 = (float)(y - System.Math.Sqrt(squareDe));
        for (int i = 0; i < size; i++)
        {
            x3 = x1 + stride;
            x4 = x2 - stride;
            squareDe = r * r - System.Math.Pow(x - x3, 2);
            squareDe = squareDe > 0 ? squareDe : 0;
            y3 = (float)(y + System.Math.Sqrt(squareDe));
            squareDe = r * r - System.Math.Pow(x - x4, 2);
            squareDe = squareDe > 0 ? squareDe : 0;
            y4 = (float)(y - System.Math.Sqrt(squareDe));

            //绘制线段
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex(new Vector3(x1 / Screen.width, y1 / Screen.height, z));
            GL.Vertex(new Vector3(x3 / Screen.width, y3 / Screen.height, z));
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex(new Vector3(x2 / Screen.width, y1 / Screen.height, z));
            GL.Vertex(new Vector3(x4 / Screen.width, y3 / Screen.height, z));
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex(new Vector3(x1 / Screen.width, y2 / Screen.height, z));
            GL.Vertex(new Vector3(x3 / Screen.width, y4 / Screen.height, z));
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex(new Vector3(x2 / Screen.width, y2 / Screen.height, z));
            GL.Vertex(new Vector3(x4 / Screen.width, y4 / Screen.height, z));
            GL.End();

            x1 = x3;
            x2 = x4;
            y1 = y3;
            y2 = y4;
        }
        GL.PopMatrix();
    }
}
