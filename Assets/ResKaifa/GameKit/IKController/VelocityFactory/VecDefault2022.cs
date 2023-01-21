using System.Collections.Generic;
using UnityEngine;

namespace ResKaifa.GameKit
{
    /// <summary>
    /// 2022写的代码，有一个小跳！！！
    /// </summary>
    public class VecDefault2022:VecBase
    {
        public override void HandleUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            var Motor = _ik.Motor;

             //in ground Movement
             if (Motor.GroundingStatus.IsStableOnGround)
             {
                 //从空中回到滑行轨道 或地面
//                     if (!_logFixGroundedConsumed)
//                     {
//                         
//                         _logFixGroundedConsumed = true;
//                         var beforVelocity = Motor.Velocity;
//                         currentVelocity = _logCurrentVelocity;//这个速度更高(落地时可获得一个加速)
// //                        currentVelocity = Motor.Velocity;//这个速度好像有点不靠谱
// //                        Debug.LogWarning($"落地修正后速度：{currentVelocity}{currentVelocity.sqrMagnitude} - {beforVelocity}");
//                         _controller.OnEnterGround(currentVelocity);
//                     }

                // //待排查。。。。。。。。。。。导致没有了向上的速度。。。。
                //
                //  SkateMove(ref currentVelocity, deltaTime);
                //  
                 Debug.LogError("OnAir 结束");
              //   MyPlayer.Inst.m_IsStopInput = false;
             }
             //air Movement
             else
             {
             //    Motor.ForceUnground(0.2f);
                 Debug.LogError("OnAir 1 向上速度 y=" + currentVelocity);
              //   MyPlayer.Inst.m_IsStopInput = true;
                 //只要 on Air ，则只有上升速度
                 //currentVelocity = new Vector3(0, currentVelocity.y, 0);//只搞上升速度
                 //currentVelocity = Vector3.zero;
                 
                 
                 //修正离地时的速度（vec3)
                 //MyPlayer.Inst.FixLeaveGroundVel ocity(ref currentVelocity);
                 
                 //注意！！这个离地的速度调整，和跳跃离地同时会触发
                 //所以会把向上跳的速度修正掉（现在 jumpDir = up * 2)
                 // if (!_ik._logFixedConsumed)//离地时刻
                 // {
                 //     currentVelocity = Vector3.up * 22;//离地时，加一个很大的加速度（向上）
                 //     
                 //     _ik.RevertCamTrigger = true;
                 //     _ik.onAirPaths = new List<Vector3>();
                 //     _ik._logFixedConsumed = true;
                 //     // _ik._logSpeedTangent = currentVelocity;
                 //     // //_logSpeedProjection = Vector3.Project(currentVelocity, Vector3.up);
                 //     // //currentVelocity += Vector3.up * currentVelocity.magnitude + Vector3.Project(currentVelocity, Vector3.up);
                 //     // _ik._logSpeedProjection = Vector3.ProjectOnPlane(currentVelocity, Vector3.up);
                 //     // // 减去向前的冲力是不严谨的，并不能处理内环轨道这种
                 //     // // 但只要模型轨道末端的面片做成垂直向上或外翻一点都可支持（不内卷)
                 //     // if (_ik.OnAirRotateMode == MyIK.WhileOnAirMode.Up)
                 //     // {
                 //     //     // 测试用,固定3个单位的高度，暂定:Vector3.up * 3f 
                 //     //     currentVelocity += Vector3.up * 3f - Vector3.ProjectOnPlane(currentVelocity, Vector3.up);
                 //     // }
                 //     // else if (_ik.OnAirRotateMode == MyIK.WhileOnAirMode.GroundOutterNormal)
                 //     // {
                 //     //
                 //     //     // forwardProj 和 Motor.CharacterForward 是 forward 是向前平面的投影，而 CharacterForward 是实时的世界矢量（动态的）
                 //     //     //var forwardProj = Vector3.ProjectOnPlane(currentVelocity, Vector3.up);
                 //     //     //跟进中。。。。。。               
                 //     //     currentVelocity +=
                 //     //         Vector3.ProjectOnPlane(currentVelocity, _ik.DumpGroundEdgeNormal())
                 //     //             .normalized * 1.2f
                 //     //         - Vector3.ProjectOnPlane(currentVelocity, Vector3.up);
                 //     // }
                 //
                 //     //_ik._logSpeedFixMoment = currentVelocity;
                 //     MyPlayer.Inst.OnLeaveGround(currentVelocity);
                 // }
                 // else
                 // {
                 //     
                 //     // Gravity
                 //     currentVelocity += _ik.Gravity * deltaTime;    
                 //     
                 // }
                 Debug.LogError($"离地速度, {currentVelocity} total={currentVelocity.sqrMagnitude}");
                 
                 if (_ik.onAirPaths == null)
                     _ik.onAirPaths = new List<Vector3>();
                 _ik.onAirPaths.Add(MyPlayer.Inst.Position);
                 
      
             }
             
             /// //Handle jumping
             
             _ik.HandleJump(ref currentVelocity,deltaTime);
             
         
             //_ik._logCurrentVelocity = currentVelocity;
            //处理，启动，停止 TODO:现在不是太准确，这个暂时实现声音功能而已
            // if (Motor.GroundingStatus.IsStableOnGround)
            // {
            //     if (currentVelocity.sqrMagnitude >= groundAudioStopInterval)
            //     {
            //         _controller.OnStartGround();
            //     }
            //     else
            //     {
            //         _controller.OnStopGround();
            //     }
            // }
            // //处理Tail
            // if (Motor.GroundingStatus.IsStableOnGround == false)
            // {
            //     _controller.RemoveTrail();
            // }else if (IsWorldGround == false)
            // {
            //     //TODO:加速时，才需要Tail
            //     _controller.EmmitTrail();
            // }
            // else
            // {
            //     //TODO:从轨道滑下来，速度还是比较大，能延迟一点 Remove Tail
            //     _controller.RemoveTrail();
            // }
        }

        public override void HandlePostGround()
        {
            base.HandlePostGround();
        }
        
        public void SkateMove(ref Vector3 currentVelocity, float deltaTime)
        {
            var Motor = _ik.Motor;
            
            float currentVelocityMagnitude = currentVelocity.magnitude;
            /////////////////// 地球（外沿 内轨）球面移动 /////////////////////////////
            Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
            if (currentVelocityMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
            {
                // Take the normal from where we're coming from
                Vector3 groundPointToCharacter = Motor.TransientPosition - Motor.GroundingStatus.GroundPoint;
                if (Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f)
                {
                    effectiveGroundNormal = Motor.GroundingStatus.OuterGroundNormal;
                }
                else
                {
                    effectiveGroundNormal = Motor.GroundingStatus.InnerGroundNormal;
                }
            }
            /////////////////// 地球（外沿 内轨）球面移动 End /////////////////////////////
            
            /////////////////////// 滑板的斜坡下滑 ////////////////////////////////////
            // if (!IsWorldGround)
            // {
            //     if(_moveInputVector.sqrMagnitude==0)
            //         _moveInputVector += Gravity * deltaTime;
            // }
            // else
            // {
            //   
            // }
            /// /////////////////////// 滑板的斜坡下滑 End////////////////////////////////////
            
            ///////////////////////// 沿着表面移动 /////////////////////////////////////
            // Reorient velocity on slope
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
            
            //做一个修正吧，接近于向上的角度，就保持直上直下
            //float upAngleDeg = Vector3.Dot(currentVelocity.normalized, Vector3.up);
            
            //测试结果：Vector3.Dot（）的值肯定可以超过1，网上很多说】-1,1】只是因为都用了归一，以及都是光线光强（范围【0，1】）居多
            // if(upAngleDeg!=0)
            
            
            
            // //if ( upAngleDeg> OrientationFlyFixed)
            // float upAngle = Vector3.Angle(currentVelocity, Vector3.up);
            // if(upAngle> - OrientationFlyFixedAngle && upAngle<OrientationFlyFixedAngle)
            // {
            //     currentVelocity = Vector3.up * currentVelocity.magnitude;
            //     Debug.LogError("<color=green>upAngle="+upAngleDeg + " anlge=</color>" + Vector3.Angle(currentVelocity,Vector3.up));
            // }
            // else
            // {
            //     Debug.LogError("upAngle="+upAngleDeg + " anlge=" + Vector3.Angle(currentVelocity,Vector3.up));
            // }
        
            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(_ik._moveInputVector, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _ik._moveInputVector.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * _ik.MaxStableMoveSpeed;
            // float targetAngle = Vector3.Angle(targetMovementVelocity, Vector3.up);
            // if(targetAngle>-OrientationFlyFixedAngle && targetAngle<OrientationFlyFixedAngle)
            // //if (Vector3.Dot(targetMovementVelocity, Vector3.up) > OrientationFlyFixed)
            // {
            //     targetMovementVelocity = Vector3.up * targetMovementVelocity.magnitude;
            // }
        
            // Smooth movement Velocity (平滑地增加速度)
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-_ik.StableMovementSharpness * deltaTime));
            ///////////////////////// 沿着表面移动 End /////////////////////////////////////
        
          
        }
    }
}