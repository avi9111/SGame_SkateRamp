using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEditor;
//using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public partial class MyIK : MonoBehaviour,ICharacterController
{
    /// <summary>
    /// 记录离地点，所用，红色球
    /// </summary>
    private Vector3? _logSpeedLastGroundPos;
    private Color hightlightYellow;
    public List<Vector3> _logSwitchPoints = new List<Vector3>();
    public List<Vector3> _logSwitchGreenPoints = new List<Vector3>();
    private Vector3 beforeUp;
    private Vector3 beforePosition;
    public int beforeUpAccumulate;
    public Vector3 _logSlopeDir;

    public float _logFlyHeight;
    public Vector3 _logFlyStartPoint;
    public Vector3 _logFlyStartVeclocityFixed;//修正速度
    public Vector3 _logFlyStartVeclocity;//原速度
    public void InitGizmos()
    {
        _logSwitchPoints.Clear();
        _logSwitchGreenPoints.Clear();
        if (ColorUtility.TryParseHtmlString("#FFC04C", out hightlightYellow) == false)
        {
            Debug.LogError("颜色 获取错误，请检查，必须带#号");
        }

     //   hightlightYellow = Color.blue;
    }

    public void AddLogSwitchPoint(Vector3 vec)
    {
        if (_logSwitchPoints.Contains(vec) == false)
        {
            _logSwitchPoints.Add(vec);
        }
    }

    public void AddLogGreenPoint(Vector3 vec)
    {
        if (_logSwitchGreenPoints.Contains(vec) == false)
            _logSwitchGreenPoints.Add(vec);
    }

    #region -------------- 统计信息（可视化辅助调试） ---------------
    void OnDrawGizmosSelected()
    {
        // //输出速度
        // if (_logCurrentVelocity.sqrMagnitude > 0)
        // {
        //     Debug.DrawLine(transform.position, transform.position + (_logCurrentVelocity*20),Color.yellow);//黄色，个人的当前速度（vec3)
        // }
    }


    Vector3 GetRampTangentNormal()
    {
        //return Motor.CharacterUp;
        //if not ok(whild testing)

        return Vector3.right;

    }

    void OnDrawGizmos()
     {
         if (enabled == false) return;
         //------------------------- 上上 CharacterUp的谈黄色扇形） --------------------
         if (Motor != null)
         {
             //Gizmos.DrawFrustum();//TODO:是什么功能？DrawFrustum 
             var transform1 = Motor.transform;
             
            // Gizmos.DrawMesh(GizmosTools.SemicircleMesh(0.9f, 65, transform1.right,transform1.up, transform1.position));
            //GizmosTools.DrawWireSemicircle(0.9f, 65,transform1.right, transform1.up, transform1.position,9,false,Color.yellow);
            // //var l = Vector3.up;
            // var l = Quaternion.AngleAxis(0, transform1.right)*Vector3.up;
            var l = Vector3.up;
            var r = Motor.CharacterUp;
            // //var r = transform1.up;
            // var r = Quaternion.AngleAxis(100, transform1.right)*Vector3.up;
        
            
            
            // //int density = Mathf.FloorToInt( (Mathf.Rad2Deg*Vector3.Dot(l, r)) / 10);
            // int density = Mathf.FloorToInt(Vector3.Dot(l, r) * 10);// *10 和 /0.1 一样
            int density = -1;
            //var lp = Vector3.ProjectOnPlane(l, Vector3.up);//错误
            //var rp = Vector3.ProjectOnPlane(r, Vector3.up);//错误
            //var isPlus = Vector3.Cross(lp, rp).z > 0;//错误的方法
            var cc = Vector3.Cross(l, r);
            var cf = Vector3.Cross(cc, transform1.up);
            var isPlus = Vector3.Dot(cf, Motor.CharacterForward) > 0.8f;
            
//            Debug.Log("角度 isPlus=" + isPlus + " cf=" +cf +"|forward=" +Motor.CharacterForward);//注意transform1.forward有问题，transform1.right却是对的，应该能和.CharacterRight匹配
            GizmosTools.DrawWireSemicircle(0.9f, -cc, l, r, isPlus, transform1.position, density,Color.yellow);
            //GizmosTools.DrawWireSemicircle(0.9f, Motor.CharacterRight, l, r, isPlus, transform1.position, density,
              //  Color.yellow);

            // var defColor = Gizmos.color;
            // Gizmos.color = hightlightYellow;
            //
            // //Gizmos.DrawLine(transform1.position,transform1.position+l*0.9f);
            //
            // Gizmos.color = default;
            GizmosTools.DrawLine(transform1.position, transform1.position+Motor.CharacterForward, 10, Color.red);
            if(cc!=Vector3.zero)
                GizmosTools.DrawLine(transform1.position,transform1.position+cc,10,GizmosTools.ColorBlue);
            if(cf!=Vector3.zero)
                GizmosTools.DrawLine(transform1.position,transform1.position+cf,2,GizmosTools.ColorGreen);
           // GizmosTools.DrawLine(transform1.position, transform1.position + lp, 10, Color.blue);
            //GizmosTools.DrawLine(transform1.position, transform1.position + rp, 10, Color.blue);
            GizmosTools.DrawLine(transform1.position, transform1.position + l * 0.9f, 10);
            GizmosTools.DrawLine(transform1.position, transform1.position + r * 0.9f, 10);
         }
         //---------------- 转折很大的点 ------------------
         foreach (var point in _logSwitchPoints)
         {
             GizmosTools.DrawWireSphere(point,Color.red);
         }

         foreach (var point in _logSwitchGreenPoints)
         {
             GizmosTools.DrawWireSphere(point,Color.green);
         }
         
                      
         //------------------ 斜坡方向投影 -------------------------
         // if(Motor!=null)
         // {
         //     var tf = Motor.transform;
         //     //----------------- 测试 --------------
         //     
         //     // if (beforeUp != Vector3.zero)
         //     // {
         //     //     GizmosTools.DrawLine(tf.position, tf.position + beforeUp * 2, 11);
         //     //     GizmosTools.DrawLine(tf.position, tf.position + afterUp * 3, 7,Color.green);
         //     // }
         //     
         //
         //     if (_logSlopeDir != Vector3.zero)
         //     {
         //         GizmosTools.DrawLine(tf.position, tf.position + _logSlopeDir*4, 2, Color.red);
         //     }
         // }
         //
         
         //------------------ 飞出轨道(离开轨道） ---------------------------- 
         if (_logFlyHeight != 0)
         {
             if (Motor != null)
             {
                 var planeRight = Vector3.Cross(Motor.CharacterUp, Vector3.up);
                 var planeAixs = Vector3.Cross(planeRight, Vector3.up);
                 //var planeNorm = planeRight;
                 GizmosTools.DrawLine(_logFlyStartPoint, _logFlyStartPoint + Vector3.up * 2, 3,Color.red);
                 var pUp = _logFlyStartPoint + new Vector3(0, _logFlyHeight, 0);
                 GizmosTools.DrawLine(_logFlyStartPoint, pUp, 5,
                     Color.gray);
         
         
                 //var rPoint = Vector3.ProjectOnPlane(pUp + Vector3.right, planeAixs);
                 //GizmosTools.DrawLine(pUp,rPoint *3.2f,5,Color.gray);//投影射线？？
                 //GizmosTools.DrawLine(_logFlyStartPoint,rPoint,3,Color.magenta);//测试投影线（临时）
                 GizmosTools.DrawLine(pUp, pUp + _logFlyStartVeclocity, 3, Color.magenta); //测试速度
                 
                 //GizmosTools.DrawLine(pUp, pUp + _logFlyStartVeclocity, 3,Color.black);//速度射线
                 var veclocityProject = Vector3.ProjectOnPlane(_logFlyStartVeclocity, GetRampTangentNormal());//这个  Vector3.right(面 Normal) 不准确，实际上是垂直于轨道切面的法线，所以也就是 CharacterUp
                 GizmosTools.DrawLine(pUp, pUp + veclocityProject, 3, Color.black);//速度投影
                 GizmosTools.DrawLine(pUp+veclocityProject,pUp+_logFlyStartVeclocity,3,Color.blue);//速度 -抵消
                 GizmosTools.DrawLine(pUp,pUp+Vector3.right,3,Color.black);//原射线
                 //GizmosTools.DrawLine(pUp + Vector3.right, rPoint, 3, Color.blue);//指向
                 
                 GizmosTools.DrawWireSphere(_logFlyStartPoint, Color.grey, 1f);
                 
                 GizmosTools.DrawWireSemicircle(1,135,planeAixs,Vector3.down,pUp);
             }
         }
         
         // //    var position = transform.position;
         // bool isDraw = false;
         // // if (_logSpeedTangent != Vector3.zero)
         // // {
         // //     //Gizmos.color = Color.black;
         // //   //  isDraw = true;
         // //     Debug.DrawLine(position, position + (_logSpeedTangent*20),Color.black);
         // // }
         // //
         // // if (_logSpeedFixMoment != Vector3.zero)
         // // {
         // //     //Gizmos.color = Color.blue;
         // //  //   isDraw = true;
         // //     
         // //     Debug.DrawLine(position, position + (_logSpeedFixMoment*4),Color.blue);
         // // }
         //
         //    //测试 character 的向上法线，确实如预期
         // //Debug.DrawLine(position,position + Motor.CharacterUp*3,Color.red);
         //
         // // if (_logSpeedTangentPrev != Vector3.zero)
         // // {
         // //     Debug.DrawLine(position ,position + _logSpeedTangentPrev * 10 , Color.green);
         // // }
         //
         //
         // // //速度前进方向，在水平面的投影
         // // if (_logSpeedProjection != Vector3.zero)
         // // {
         // //     Debug.DrawLine(position, position + _logSpeedProjection.normalized*2, Color.cyan);//靛青色，离开轨道时的速度（Vec3)
         // // }
         // //            
         // // // if(Motor!=null)
         // // //    Debug.DrawLine(position,position+Motor.CharacterForward);//靛青色 - 向前，即使人有旋转，角度也不变(注意不是Motro.forward);
         // //
         // //
         // // if (_logSpeedMotorPosition != null)
         // // {
         // //     isDraw = true;
         // //     Gizmos.DrawWireSphere(_logSpeedMotorPosition.Value, 1f);//原色球，上是一个时间点
         // //     Vector3 posInEnd = _logSpeedMotorPosition.Value + _logSpeedGroundInner;
         // //     Debug.DrawLine(_logSpeedMotorPosition.Value,posInEnd,Color.black );//黑色 - 离地内切角
         // //     //Handles.Label(posInEnd, "IN");
         // //     Vector3 posOutEnd = _logSpeedMotorPosition.Value + _logSpeedGroundOutter;
         // //     Debug.DrawLine(_logSpeedMotorPosition.Value,posOutEnd,Color.blue );//蓝色 - 离地外切角
         // //     //Handles.Label(posOutEnd,"OUT");
         // //     if(_logSpeedGround!=null)
         // //        Debug.DrawLine(_logSpeedMotorPosition.Value,_logSpeedMotorPosition.Value + _logSpeedGround.Value.GroundNormal,Color.cyan);//靛青色 - 地面切线
         // // }
         //
         // if (_logSpeedLastGroundPos !=null)
         // {
         //     isDraw = true;
         //     Gizmos.color = Color.red;
         //     Gizmos.DrawWireSphere(_logSpeedLastGroundPos.Value,1f);//红色球
         // }
         //
         //
         //
         // //注意！！！会还原白色
         // if(isDraw)
         //     Gizmos.color = Color.white;

         
         //--------------------- ????----------------------------
         // if (onAirPaths != null && onAirPaths.Count > 1)
         // {
         //     for (int i = 0; i < onAirPaths.Count - 1; i++)
         //     {
         //         if (Vector3.Dot(onAirPaths[i + 1] - onAirPaths[i], Vector3.up) > 0)
         //         {
         //             Debug.DrawLine(onAirPaths[i],onAirPaths[i+1]);
         //         }
         //         else
         //         {
         //            Debug.DrawLine(onAirPaths[i],onAirPaths[i+1],Color.gray);
         //         }
         //
         //         
         //     }
         // }

     }
     
     
          
     #endregion
}
