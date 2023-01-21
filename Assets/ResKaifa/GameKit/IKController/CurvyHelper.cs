using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurvyHelper:SpecSingleton<CurvyHelper>
{
    public bool IsPointInLine(Vector3 p1, Vector3 p2, Vector3 pos)
    {
        return (p1 - pos).normalized == -(p2 - pos).normalized;
    }

    public Vector3 CalNeastPointFixed(Vector3 p1, Vector3 p2, Vector3 pos)
    {
        var p1p2 = p2 - p1;
        var pBp1 = pos - p1;
        var len = Vector3.Dot(p1p2, pBp1) / p1p2.magnitude;//这里要除的是，被投影的矢量长度（注意！！不要搞混）
        
        return p1 + p1p2.normalized * len;
    }
    
    /// <summary>
    /// 有错的方法
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector3 CalNeastPoint(Vector3 p1, Vector3 p2, Vector3 pos)
    {
        var p2p1 = p2 - p1;
        var len = Vector3.Dot(p2p1, pos) / pos.magnitude;
        return p1 + p2p1.normalized * len;
    }
}
