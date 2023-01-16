using System;
using System.Collections;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UIElements;

namespace ResKaifa.GameKit
{

    
    /// <summary>
    /// 2023年重新做过一个（之前的实验都坏了）
    /// </summary>
    public class Vec2023:VecBase
    {
        private Vector3 beforePosition;
        public int playerLayer = 8;//Character
        public override void BeforeCharacterUpdate(float deltaTime)
        {
            beforePosition = _ik.position;
        }

        public override void HandleUpdate(ref Vector3 currentVelocity, float deltaTime)
        {
            Debug.LogError("比较pos差异 " + beforePosition + " curr=" + _ik.position);
            var Motor = _ik.Motor;
            var _moveInputVector = _ik._moveInputVector;
              // Ground movement
            if (Motor.GroundingStatus.IsStableOnGround && MyPlayer.Inst.CtlIK._logFlyHeight==0)
            {
                //EventDispatcher.Inst.DispatchEvent(EventMazeGame.OnVelocityUpdating, true);
                MyPlayer.Inst.CtlIK.isGroundUpdating = true;
                float currentVelocityMagnitude = currentVelocity.magnitude;

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

                // Reorient velocity on slope
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                Vector3 targetMovementVelocity = reorientedInput * _ik.MaxStableMoveSpeed;
            
                
                // //因为有斜坡，还是加一个重力处理
                // // Gravity
                targetMovementVelocity += _ik.GravityInSlide * deltaTime;
                
                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-_ik.StableMovementSharpness * deltaTime));

                
//                if (IsSlopeRaycast())
                if(IsSlope())
                {
                    // if (_ik.triggerLeaveTurnalConsume)//TODO:不是很稳定的方法，待研发
                    // {
                    //     currentVelocity += new Vector3(0, ASkateConst.GravityJumpUp, 0);
                    //     _ik.triggerLeaveTurnalConsume=false;
                    //     _ik.beforeUpAccumulate = 0;
                    // }
                    // else
                    // {
                        //在斜坡必然有，这个力，所以注意，上下离开轨道的力，需要抵消这个力
                        currentVelocity += _ik.Gravity * deltaTime;
                    //}
                }

            }
            // Air movement
            else
            {
                //EventDispatcher.Inst.DispatchEvent(EventMazeGame.OnVelocityUpdating, false);
                MyPlayer.Inst.CtlIK.isGroundUpdating = false;
                // Add move input
                // if (_moveInputVector.sqrMagnitude > 0f)
                // {
                //     Vector3 addedVelocity = _moveInputVector * _ik.AirAccelerationSpeed * deltaTime;
                //
                //     Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);
                //
                //     // Limit air velocity from inputs
                //     if (currentVelocityOnInputsPlane.magnitude < _ik.MaxAirMoveSpeed)
                //     {
                //         // clamp addedVel to make total vel not exceed max vel on inputs plane
                //         Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, _ik.MaxAirMoveSpeed);
                //         addedVelocity = newTotal - currentVelocityOnInputsPlane;
                //     }
                //     else
                //     {
                //         // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                //         if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                //         {
                //             addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                //         }
                //     }
                //
                //     // Prevent air-climbing sloped walls
                //     if (Motor.GroundingStatus.FoundAnyGround)
                //     {
                //         if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                //         {
                //             Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                //             addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                //         }
                //     }
                //
                //     // Apply added velocity
                //     currentVelocity += addedVelocity;
                // }
                

                
                // Gravity
                currentVelocity += _ik.Gravity * deltaTime;
                
                // Drag
                currentVelocity *= (1f / (1f + (_ik.Drag * deltaTime)));
                
               
            }
            
            // Take into account additive velocity
            // if (_internalVelocityAdd.sqrMagnitude > 0f)
            // {
            //     currentVelocity += _internalVelocityAdd;
            //     _internalVelocityAdd = Vector3.zero;
            // }
            
            if(MyPlayer.Inst.CtlIK._logFlyHeight==0)//一直用，可能还好点。。。。。。
                PreSimulatePhysic(ref currentVelocity);
            //PreSimulateVelocity(ref currentVelocity);
        }
        /// <summary>
        ///( 可能有问题 ) 因为 Kinematic CC已经做了Ground的射线判断，我们就不用再 Raycast了，直接用 Normal;
        ///（观察后）发现虽然不是完全在斜坡上，可获得最陡峭的几个点，而且联系，基本能用 
        /// </summary>
        /// <returns></returns>
        public bool IsSlope()
        {
            //但是要注意，是不是 Kinematic 的这个参数就是这个意思
            return Vector3.Dot(_ik.Motor.CharacterUp, Vector3.up) < 0.8f;
        }
        [System.Obsolete("Raycast 这个判断好像有问题，需要fix")]
        public bool IsSlopeRaycast()
        {
            int layerMask = ~(1 << playerLayer);
            RaycastHit hit;
            //这个判断好像有问题，需要fix
            if (Physics.Raycast(_ik.Motor.transform.position + Vector3.up * 2, Vector3.down,  out hit,2.8f,layerMask))
            {
                Debug.LogError("一直 Name=" + hit.collider.gameObject.name);
                return hit.normal != Vector3.up;
            }
            
            return false;//待实现
        }

        private bool isLeaveTurnnalConsume = false;
        private float lastCharacterRad;
        private Vector3 lastCharacterPos;
        private bool lastCharacterSlopeHeap;
        /// <summary>
        /// TODO:1.观察者角度，更好观察运动（现在是尾随运动镜头);
        /// 2.减半deltaTime，或者更多，可以慢速观察
        /// 3.优化Gizmos,操作后才记录只记录一段（现在是一直记录，直到操作后3秒）
        /// </summary>
        /// <param name="currVelocity"></param>
        void PreSimulatePhysic(ref Vector3 currVelocity)
        {
            // RaycastHit hit;
            // if(Physics.Raycast(_ik.Motor.transform.position*2+ Vector3.up,Vector3.down,out hit,3))
            // {
            //     var normal = hit.normal;
            //     
            // }

            // //测试速度朝向
            // //_ik._logSlopeDir = currVelocity;
            // if (isLeaveTurnnalConsume == true)
            // {
            //     Debug.LogError("一直加速？？");
            //     currVelocity += new Vector3(0, ASkateConst.GravityJumpUp, 0);
            //     isLeaveTurnnalConsume = false;
            // }
            
            var planeRight = Vector3.Cross(_ik.Motor.CharacterUp, Vector3.up);
            var dir = Vector3.ProjectOnPlane(currVelocity, planeRight);
            var characterRad = Vector3.Dot(dir, _ik.Motor.CharacterUp);
            var slopeHeap = IsSlope();
            if (slopeHeap/* && _ik.Motor.GroundingStatus.IsStableOnGround*/)
                //if(IsSlopeRaycast())
            {
                //----------- c测试 IsSlpeRacasy() -------------
                //_ik.AddLogSwitchPoint(_ik.position);
                
                //------------ 测试高低是否连续 ------------------
                // if (lastCharacterPos.y > _ik.position.y)
                // {
                //     _ik.AddLogSwitchPoint(_ik.position);
                // }
                // else
                // {
                //     _ik.AddLogGreenPoint(_ik.position);
                // }
                }
            else
            {
                //----------- c测试 IsSlpeRacasy() -------------
                //----------- c测试 IsSlpeRacasy() -------------
                //----------- c测试 IsSlpeRacasy() -------------
                //_ik.AddLogGreenPoint(_ik.position);
                if(lastCharacterSlopeHeap)
                    if (lastCharacterPos.y <= _ik.position.y)
                    {
                        isLeaveTurnnalConsume = true;
          
                        currVelocity += new Vector3(0, 18, 0);
                        currVelocity = EventDispatcher.Inst.DispatchEventFn<Vector3,Vector3>(EventMazeGame.OnLeaveGround,currVelocity);
                        //临时处理 ！！重要！！！注意！！！保证修正速度后，不再有特殊处理。。。
                        //_ik.Motor.SetMovementCollisionsSolvingActivation(false);
                        //_ik.Motor.InteractiveRigidbodyHandling = false;
                    }
                    else
                    {
                        Debug.LogWarning("Slop判断有问题 <color='red'>请检查</color>");
                    }
            }

            lastCharacterRad = characterRad;
            lastCharacterPos = _ik.position;
            lastCharacterSlopeHeap = slopeHeap;
        }

        //private Vector3 beforeUp;
        // void PreSimulateVelocity(ref Vector3 currVelocity){
        //     if (IsSlope())
        //     {
        //         if (Vector3.Dot(currVelocity, _ik.Motor.CharacterUp) < 0.3f)
        //         {
        //             _ik._logSwitchPoints.Add(_ik.Motor.transform.position);
        //         }
        //
        //         _ik._logSlopeDir = Vector3.ProjectOnPlane(currVelocity, _ik.Motor.CharacterRight);
        //     }
        //
        //
        //     // if (_ik.Motor.transform.position.y > beforePosition.y)
        //     //     beforeUpAccumulate++;
        //     // else
        //     // {
        //     //     beforeUpAccumulate = 0;
        //     // }
        //     //
        //     // beforePosition = Motor.transform.position;
        //     
        //     
        //     // //this.CharacterController.UpdateVelocity(ref BaseVelocity, deltaTime);
        //     // if (BaseVelocity.magnitude < KinematicCharacterMotor.MinVelocityMagnitude)
        //     // {
        //     //     BaseVelocity = Vector3.zero;
        //     // }
        //     //
        //     // #region Calculate Character movement from base velocity   
        //     // // Perform the move from base velocity
        //     // if (BaseVelocity.sqrMagnitude > 0f)
        //     // {
        //     //     if (_solveMovementCollisions)
        //     //     {
        //     //         InternalCharacterMove(ref BaseVelocity, deltaTime);
        //     //     }
        //     //     else
        //     //     {
        //     //         _transientPosition += BaseVelocity * deltaTime;
        //     //     }
        //     // }
        // }
        
        
        
        
        
        
    }
}