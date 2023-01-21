using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace ResKaifa.GameKit
{
    /// <summary>
    /// 又玩坏了。。。。
    /// </summary>
    public class VecHistoryBk:VecBase
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

                //待排查。。。。。。。。。。。导致没有了向上的速度????。。。。
                //_ik.SkateMove(ref currentVelocity, deltaTime);
                StartGroundMove(ref currentVelocity, deltaTime);
             }
             //air Movement
             else
             {
                 // currentVelocity += _ik.Gravity * deltaTime;                // Gravity add+
                 
                 //Debug.LogError($"离地速度, {currentVelocity} total={currentVelocity.sqrMagnitude}");
                 
                 if (_ik.onAirPaths == null)
                     _ik.onAirPaths = new List<Vector3>();
                 _ik.onAirPaths.Add(MyPlayer.Inst.Position);
             }
             
             currentVelocity += _ik.Gravity * deltaTime;                // Gravity add+
             
             /// //Handle jumping
             
           //  _ik.HandleJump(ref currentVelocity,deltaTime);
             
        }
        
        public override void HandlePostGround()
        {
            //base.HandlePostGround();
            //离地时
            if(_ik.Motor.LastGroundingStatus.IsStableOnGround && _ik.Motor.GroundingStatus.IsStableOnGround == false)
            {
                //_ik._logFixedConsumed = false;
            }
            //落地，接触地面时
            else if (_ik.Motor.LastGroundingStatus.IsStableOnGround== false && _ik.Motor.GroundingStatus.IsStableOnGround)
            {
                //_ik._logFixGroundedConsumed = false;
            }
        }
       
        void StartGroundMove(ref Vector3 currentVelocity,float deltaTime)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;
            var Motor = _ik.Motor;
            /////////////////// 地球（外沿 内轨）球面移动 /////////////////////////////
            Vector3 effectiveGroundNormal = _ik.Motor.GroundingStatus.GroundNormal;
            if (currentVelocityMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
            {
                Debug.LogError("测试一个 sis-580 SnappingPrevented");
            
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

            /////////////////////// 滑板的斜坡下滑（不稳定，容易造成其他问题） ////////////////////////////////////
            var moveInput = _ik._moveInputVector;
            // if (!_ik.IsWorldGround) //下面这段。。。一个错误的下压力，反而使得[上坡顶点]的离地，没有这段代码反而不能离地
            // {
            //     if(moveInput.sqrMagnitude==0)
            //         moveInput +=_ik.Gravity * deltaTime;
            // }
            
            ///////////////////////// 沿着表面移动 /////////////////////////////////////
            
            // Reorient velocity on slope
            var lastVelocity = currentVelocity;
            currentVelocity = _ik.Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                              currentVelocityMagnitude;
            if (IsDirectionUpCollapse(lastVelocity,currentVelocity) && Vector3.Dot(lastVelocity, currentVelocity)>0.3f)//超过一定范围则飞出
            {
               // currentVelocity = new Vector3(0, lastVelocity.y + MyPlayer.Inst.UpSensitive, 0);
            }

            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(moveInput, _ik.Motor.CharacterUp);
            Vector3 reorientedInput =
                Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInput.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * _ik.MaxStableMoveSpeed;

            // Smooth movement Velocity (平滑地增加速度)
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1f - Mathf.Exp(-_ik.StableMovementSharpness * deltaTime));
        }

        bool IsDirectionUpCollapse(Vector3 last,Vector3 vec)
        {
            if (last == vec) return false;
            var vl = Vector3.Dot(last,Vector3.up);
            var vc = Vector3.Dot(vec, Vector3.up);
            Debug.LogError($"vl={vl} vc={vc} offset={Vector3.Dot(last, vec)}(last={last} v={vec}");
            return vl > vc;
        }

        bool IsUp(Vector3 vec)
        {
            return Vector3.Dot(Vector3.up, vec) >0.7f;
        }
 
    }

}