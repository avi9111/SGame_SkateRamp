using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public static class GizmosTools
{
    public static Color ColorBlue = UnityEngine.Color.blue;
    public static Color ColorBlue2 = Color.blue;
    public static Color ColorBlue3 = Color.blue;
    public static Color ColorGrey = Color.grey;
    public static Color ColorGreen = Color.green;
    /// <summary>
    /// 粉红色
    /// </summary>
    public static Color ColorMagenta2 = Color.magenta;

    public static void Init()
    {
        ColorUtility.TryParseHtmlString("#12FFFD", out ColorBlue2);
        ColorUtility.TryParseHtmlString("#FF12BC", out ColorMagenta2);        
    }

    /// <summary>
    /// 绘制半圆（中心轴向上画的平面，所以这个方法只能画技能）
    /// </summary>
    public static void DrawWireSemicircle(float radius,int angle,Vector3 direction,Vector3 origin,int density=10)
    {
        DrawWireSemicircle(radius,angle,Vector3.up,direction,origin,density); //
    }

    // public static void DrawWireSemicircle(float radius, Vector3 axis, Vector3 leftDir, Vector3 rightDir, Vector3 origin,
    //     int desnsity = 5, Color? color = null)
    // {
    //     
    // }
    /// <summary>
    /// 绘制半圆（扇形）
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    /// <param name="axis"></param>
    /// <param name="direction">一般：Vector3.up</param>
    /// <param name="origin"></param>
    /// <param name="density"></param>
    /// <param name="isDirectionHalf"></param>
    /// <param name="color"></param>
    public static void DrawWireSemicircle(float radius, int angle, Vector3 axis, Vector3 direction,Vector3 origin,
        int density = 10, bool isDirectionHalf = true, Color? color = null)
    {
        if (angle == 0) return;
        Vector3 leftdir = Vector3.zero;
        Vector3 rightdir = Vector3.zero;
        if (isDirectionHalf)
        {
            leftdir = Quaternion.AngleAxis(-angle / 2, axis) * direction;
            rightdir = Quaternion.AngleAxis(angle / 2, axis) * direction;
        }
        else
        {
            leftdir = Quaternion.AngleAxis(0, axis) * direction;
            rightdir = Quaternion.AngleAxis(angle, axis) * direction;
        }

        DrawWireSemicircle(radius, axis, leftdir, rightdir,angle>0, origin, density, color);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="axis"></param>
    /// <param name="leftdir"></param>
    /// <param name="rightdir"></param>
    /// <param name="isPlus">left和rightDir 是否正角度（正旋转）</param>
    /// <param name="origin"></param>
    /// <param name="desnsity"></param>
    /// <param name="color"></param>
    public static void DrawWireSemicircle(float radius, Vector3 axis, Vector3 leftdir, Vector3 rightdir,bool isPlus, Vector3 origin,
        int desnsity = 10, Color? color = null)
    {
        if (leftdir == rightdir)
        {
            Gizmos.DrawLine(origin, origin + leftdir * radius);
            return;
        }
        // Vector3 leftdir = Vector3.zero;
        // Vector3 rightdir = Vector3.zero;
        // if (isDirectionHalf)
        // {
        //     leftdir = Quaternion.AngleAxis(-angle / 2, axis) * direction;
        //     rightdir = Quaternion.AngleAxis(angle / 2, axis) * direction;
        // }
        // else
        // {
        //     leftdir = Quaternion.AngleAxis(0, axis) * direction;
        //     rightdir = Quaternion.AngleAxis(angle, axis) * direction;
        // }

        var defColor = Gizmos.color;
        if (color != null)
        {
            Gizmos.color = color.Value;
        }
        Vector3 currentP = origin + leftdir * radius;
        Vector3 oldP;
        //if (angle!=360)
        {
            Gizmos.DrawLine(origin,currentP);
        }
        //因为 l,和r 的长度”模“等于1，实际公式是这样的，就不需要格外计算了
        //本来Acos（） 前需要除以 l“模”x r"模“ 的。。
        //var angleValue = Mathf.Rad2Deg * Mathf.Acos((Vector3.Dot(leftdir, rightdir)/(leftdir.magnitude*rightdir.magnitude)) / desnsity;
        var angleValue = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(leftdir, rightdir));
        //上面代码做动态，这里需要一个“等量”渐变
        //var count = desnsity;
        //angleValue = angleValue / count;
        var count = Mathf.FloorToInt(angleValue / 3);
        angleValue = 3;
        if (isPlus == false)
        {
            angleValue *= -1;
        }
        for (int i = 0; i < count; i++)//整个循环是画一个弧线
        {
            Vector3 dir= Quaternion.AngleAxis(angleValue*(i+1), axis)*leftdir;
            oldP = currentP;
            currentP=origin + dir * radius;
            Gizmos.DrawLine(oldP,currentP);
            
            //画弧线之余，也画体积线
            Gizmos.DrawLine(origin,currentP);
        }
        oldP = currentP;
        currentP=origin + rightdir * radius;
        Gizmos.DrawLine(oldP,currentP);
        //if (angle!=360)
        {
            Gizmos.DrawLine(currentP,origin);
        }

        Gizmos.color = defColor;
    }
    /// <summary>
    /// 基于小哥的基础方法，增加一些功能，如位置平移
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    /// <param name="axis"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Mesh SemicircleMesh(float radius,int angle,Vector3 axis,Vector3? forward =null, Vector3? pos = null)
    {
        if (forward == null)
            forward = Vector3.forward;//一般可能（大概率）跟随人，用的是传入的forward，而不需要这个默认的forwward
        Vector3 leftdir = Quaternion.AngleAxis(-angle/2, axis)*forward.Value;
        Vector3 rightdir = Quaternion.AngleAxis(angle/2, axis)*forward.Value;
        int pcount = angle / 10;
        //顶点
        Vector3[] vertexs = new Vector3[3+pcount];
        vertexs[0] = Vector3.zero;
        if (pos != null)
            vertexs[0] += pos.Value;
        int index = 1;
        vertexs[index] = leftdir * radius;
        if(pos!=null)
            vertexs[index] += pos.Value;
        index++;
        // Matrix4x4 matrix = Matrix4x4.identity;
        // if (pos != null)
        // {
        //     matrix.m03 = pos.Value.x;
        //     matrix.m13 = pos.Value.y;
        //     matrix.m23 = pos.Value.z;
        // }

        for (int i = 0; i < pcount; i++)
        {
            Vector3 dir= Quaternion.AngleAxis(10*i, axis)*leftdir;
            vertexs[index]= dir * radius;

          //  vertexs[index] = matrix * vertexs[index];
            if(pos!=null)
                vertexs[index] += pos.Value;
            index++;
        }
        vertexs[index] = rightdir * radius;
        if(pos!=null)
            vertexs[index] += pos.Value;
        //三角面 (改成双面了 6 *）
        int[] triangles=new int[6*(1+pcount)];
        for (int i = 0; i < 1+pcount; i++)
        {
            triangles[6 * i] = 0;
            triangles[6 * i + 1] = i+1;
            triangles[6 * i + 2] = i+2;

            triangles[6 * i + 3] = 0;
            triangles[6 * i + 4] = i + 2;
            triangles[6 * i + 5] = i + 1;
        }
        
        Mesh mesh=new Mesh();
        mesh.vertices = vertexs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, float thickness)
    {
        DrawLine(p1, p2, thickness, Color.red);
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, float thickness,Color color)
    {
        Handles.DrawBezier(p1,p2,p1,p2, color,null,thickness);
    }

    public static void DrawWireSphere(Vector3 center,Color setColor,float radius=0.1f)
    {
        var defColor = Gizmos.color;
        Gizmos.color = setColor;
        Gizmos.DrawWireSphere(center,radius);
        Gizmos.color = defColor;
    }
}