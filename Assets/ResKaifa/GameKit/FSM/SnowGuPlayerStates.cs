using System;
using System.Collections;
using System.Collections.Generic;
using RagdollMecanimMixer;
using RSG;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace SnowGu.FSM
{
    /// <summary>
    /// 空的类
    /// </summary>
    public class SnowGuStates
    {
    }
    /// <summary>
    /// 本来 SnowGu 是冬奥会 谷爱玲 项目，后改为“雪谷”项目，再后来雪实在太难做了，先做个SkateRamp项目
    /// </summary>
    public class StateDead : AbstractState
    {
        public void OnEnter(ref Animator anim)
        {
            //Debug.LogWarning("fff " + anim.enabled);
            anim.enabled = false;
            //Debug.LogWarning("eeee " + anim.enabled);
            var com = anim.GetComponent<RamecanMixer>();
            if(com!=null)
                com.enabled = true;

            MyPlayer.Inst.StopKinematic();
        }

        public void OnExit(Animator anim)
        {
            anim.enabled = true;
            
            var ragdoll = anim.GetComponent<RamecanMixer>();
            
            if(ragdoll!=null)
                ragdoll.enabled = false;
        }
    }
    

    public class StateRebone : AbstractState
    {
        public void OnEnter(Transform trans)
        {
            //闪烁

            //重置
            //必须先找一个平地（已完成）

            // 因为Motor 和 IK的强制性，所以先要停一下，，才能重置
            //DelayInvoke.StartTimmer(0.1f, () =>
            {
                
                MyPlayer.Inst.CtlIK.Motor.SetPosition(MyPlayer.Inst.m_lastGroundPos);
                MyPlayer.Inst.CtlIK.Motor.enabled = false;
               // MyPlayer.Inst.Ctl.transform.position = MyPlayer.Inst.m_lastGroundPos;
                //  trans.rotation = Quaternion.identity;
            
               
                DelayInvoke.StartTimmer(0.1f, () =>
                {

                 
                    var rot = MyPlayer.Inst.CtlIK.transform.rotation;
                    var vec3 = rot.eulerAngles;
                    var y = rot.eulerAngles.y;
                    //不能自己，强行修改咯
                    //MyPlayer.Inst.Ctl.transform.rotation = Quaternion.Euler(0, y, 0);
                    MyPlayer.Inst.CtlIK.Motor.SetRotation(Quaternion.Euler(0, y, 0));
                    Debug.LogError($"After Rebone rot {vec3}=>" +MyPlayer.Inst.CtlIK.transform.rotation.eulerAngles
                                                                +"|pos=" +MyPlayer.Inst.CtlIK.transform.position);
                    
                    DelayInvoke.StartTimmer(0.1f, () =>
                    {
                        Debug.LogError("dfldskafldksjf enable ------------------------------");
                        MyPlayer.Inst.CtlIK.Motor.enabled = true;
                        MyPlayer.Inst.StartKinematic();    
                    });
                    
                });

                

            }
            //);
            //进入Idle
            Parent.ChangeState("Idle");


        }
    }

    public class StateIdle :AbstractState
    {
        
    }

    public class  StateRun:AbstractState
    {
        
    }
    
    public class StateRunCrouchRaise : AbstractState
    {
        //在轨道，又不在平地的状态
        
    }
}
