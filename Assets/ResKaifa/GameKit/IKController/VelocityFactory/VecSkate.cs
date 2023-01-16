using System.Collections.Generic;
using ResKaifa.GameKit;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace ResKaifa.GameKit
{
    /// <summary>
    /// 试验失败1
    /// </summary>
    public class VecSkate:VecBase
    {
        public override void HandleUpdate(ref Vector3 currentVelocity, float deltaTime)
        {
            //in ground Movement
            if (_ik.Motor.GroundingStatus.IsStableOnGround)
            {
                float currentVelocityMagnitude = currentVelocity.magnitude;
                var Motor = _ik.Motor;
                /////////////////// 地球（外沿 内轨）球面移动 /////////////////////////////
                Vector3 effectiveGroundNormal = _ik.Motor.GroundingStatus.GroundNormal;
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
                ///////////////////////// 沿着表面移动 /////////////////////////////////////
                // Reorient velocity on slope
                currentVelocity = _ik.Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                                  currentVelocityMagnitude;
                var moveInput = _ik._moveInputVector;
                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(moveInput, _ik.Motor.CharacterUp);
                Vector3 reorientedInput =
                    Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInput.magnitude;
                Vector3 targetMovementVelocity = reorientedInput * _ik.MaxStableMoveSpeed;

                // Smooth movement Velocity (平滑地增加速度)
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                    1f - Mathf.Exp(-_ik.StableMovementSharpness * deltaTime));
            }
            //air Movement
            else
            {
                if(MyPlayer.Inst.IsControlling)
                    Debug.LogError("OnAir 1");
                // ///////// 游戏开始时需要一个重力下落到地面 //////////////////
                // ///////// 必须要加重力（否则可能初始化还在浮空）/////////////
                // if( _ik._logFixGroundedConsumed==true)
                //     currentVelocity += _ik.Gravity * deltaTime;
                // else
                // {
                //  //   currentVelocity.x = 0;
                //  //   currentVelocity.z = 0;
                // }
                
                currentVelocity += _ik.Gravity * deltaTime;
                
                /////////// 记录路径（SceneView 绘制用） //////////////////////
                if (_ik.onAirPaths == null)
                     _ik.onAirPaths = new List<Vector3>(); 
                _ik.onAirPaths.Add(_ik.transform.position);
            }
            
            _ik.HandleJump(ref currentVelocity,deltaTime);
        }

        public override void HandlePostGround()
        {
            
            if (_ik.Motor.LastGroundingStatus.IsStableOnGround && _ik.Motor.GroundingStatus.IsStableOnGround == false)
            {
                //_logSpeedGround = Motor.GroundingStatus;
                //_logSpeedGroundInner = Motor.GroundingStatus.InnerGroundNormal;
                //_logSpeedGroundOutter = Motor.GroundingStatus.OuterGroundNormal;
                //_logSpeedMotorPosition = Motor.GroundingStatus.GroundPoint;
                //_logSpeedLastGroundPos = Motor.LastGroundingStatus.GroundPoint;
                _ik._logFixedConsumed = false;
                
                Debug.LogWarning("设 m_StartLeaveGround 一次");
            }
            //接触地面时
            else if (_ik.Motor.LastGroundingStatus.IsStableOnGround== false && _ik.Motor.GroundingStatus.IsStableOnGround)
            {
                _ik._logFixGroundedConsumed = false;
                //landing....
                //这里启动 awake()时也会触发一次
//            Debug.LogError("接触地面时 ,打印落地shi速度：" + Motor.Velocity);
            }

          //  _isLastLand = Motor.GroundingStatus.IsStableOnGround;
        }
    }
}
