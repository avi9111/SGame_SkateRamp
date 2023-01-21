using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
/// <summary>
/// 还是不能用了，因为必须满足4个条件：
/// 1 碰撞双方必须是碰撞体
///2 碰撞的主动方必须是刚体，注意我的用词是主动方，而不是被动方
///3 刚体不能勾选IsKinematic
///4 碰撞体不能够勾选IsTigger（对于 OnCollisionEnter（）方法）
/// </summary>
public class CurvyCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnCollisionEnter(Collider collider)
    {
        Debug.LogError(collider.gameObject.name);
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.LogError("Trigger " + other.gameObject.name);
    }
}
