using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using ResKaifa.GameKit;
using RSG;
using SnowGu.FSM;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using Debug = UnityEngine.Debug;

//v1.1 已改为 e 加速，a,w改朝向
public partial class MyIK : MonoBehaviour,ICharacterController
{

    public enum CharacterState
    {
        Default,
        Skate,
    }
    // public enum WhileOnAirMode
    // {
    //     Up,
    //     GroundOutterNormal
    // }
    public bool isGroundUpdating;

    // public enum OrientationMethod
    // {
    //     TowardsCamera,
    //     TowardsMovement,
    // }
    public Vector3 position => Motor.transform.position;
    public KinematicCharacterMotor Motor;
    public Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    public Vector3 Gravity = new Vector3(0, ASkateConst.GravitySensive, 0);
    public Vector3 GravityInSlide =  new Vector3(0, ASkateConst.GravitySensive, 0) * 3;
    /// <summary>
    /// 真正决定“平地"移动速度的参数
    /// </summary>
    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    /// <summary>
    /// 摩檫力系数（其实只在当前速度，和最大速度之间，做一个lerp即实现了，默认==15f。则接近于真实世界摩檫力，若==1f，则比较光滑）
    /// </summary>
    public float StableMovementSharpness = 15f;
    
    private bool _jumpRequested = false;
    /// <summary>
    /// jump计数，注意在 AfterCharacterUpdate（）清 0
    /// </summary>
    private bool _jumpConsumed = false;
    private bool _jumpedThisFrame = false;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;
    /// <summary>
    /// 后溜的时候是否可以跳。。。。。（后溜在哪里??)
    /// </summary>
    [Header("Jumping")]
    public bool AllowJumpingWhenSliding = false;
    public float JumpUpSpeed = 10f;
    public float JumpScalableForwardSpeed = 10f;
    public float JumpPreGroundingGraceTime = 0f;
    public float JumpPostGroundingGraceTime = 0f;
    
    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 15f;
    public float AirAccelerationSpeed = 15f;
    public float Drag = 0.1f;
    /// <summary>
    /// 旋转的朝向（镜头|移动前方）-摄像机的平面投影，没有高低角
    /// </summary>
    [Header("Rotation")] 
    public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;
    public float OrientationSharpness = 10f;
    public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
    public float BonusOrientationSharpness = 10f;

        // public WhileOnAirMode OnAirRotateMode;

  //  [Range(0.8f,1)]
//    public float OrientationFlyFixed = 0.92f;

    //public float OrientationFlyFixedAngle = 10f;

    public float RevertCamVelocityCap = 0.09f;
    /// <summary>
    /// 到了最高点，翻转镜头用
    /// </summary>
    public bool RevertCamTrigger = false;
    // //滑板速度
    // //private bool skateSpeedConsumed = false;
    // private Vector3 skateSpeedLastVelocity;
    // private Vector3 skateSpeedLastMoveRight;
    //public CharacterState CurrentCharacterState { get; private set; }
    [Header("二次开发，采用 Skate 的移动状态")]
    public CharacterState CurrentCharacterState ;//TODO:去掉。。。
    
    public bool debugLog = false;
    
    /// <summary>
    /// //////////////////////// Debg 变量 //////////////////////////////////////////
    /// </summary>
    //private Vector3 _logCurrentVelocity;
    // /// <summary>
    // /// 现在应该，没用了，这个参数
    // /// </summary>
    // private Vector3 _logFixedVelocity;
    /// <summary>
    /// 离地的角度修复（PostGround 时设为 true，UpdateVelocity()时只使用一次
    /// </summary>
    public bool _logFixedConsumed = true;
    /// <summary>
    /// 落地时保证速度                   
    /// </summary>
    public bool _logFixGroundedConsumed = true;

    // //public Vector3 _logSpeedTangentCurr;
    // /// <summary>
    // /// 打印
    // /// </summary>
    // public Vector3 _logSpeedTangent;
    // public Vector3 _logSpeedTangentPrev;
    // /// <summary>
    // /// 记录修正速度？？（OnAir 的过程），同样忘了有什么作用。。。。
    // /// </summary>
    // public Vector3 _logSpeedFixMoment;
    // /// <summary>
    // /// 记录上升平面的投影，忘了怎么用了。。。
    // /// </summary>
    // public Vector3 _logSpeedProjection;
    // /// <summary>
    // /// 离地时接触点
    // /// </summary>
    // public Vector3? _logSpeedMotorPosition;
    // /// <summary>
    // /// 离地时上一个状态点
    // /// </summary>
    // public  Vector3? _logSpeedLastGroundPos;
    // public Vector3 _logSpeedGroundInner;
    // public Vector3 _logSpeedGroundOutter;
    // public CharacterGroundingReport? _logSpeedGround;
    public List<Vector3> onAirPaths;
    [Tooltip("认为，已停止的速度值")]
    public float groundAudioStopInterval = 0.2f;
    /// <summary>
    /// 动画状态
    /// </summary>
    public IState fsm;
    // /// <summary>
    // /// 人物在地面的高度（0.75f)
    // /// </summary>
    // public float _standHeight = 0.75f;
    [Tooltip("撞击地面还能保持的角度")]
    public float _standHitRad = 0.8f;
    [Header("必须设置主角动画")]
    public Animator _animator;

    private VecBase _vecHandle;
    
    private void Start()
    {
        InitGizmos();
        GizmosTools.Init();
        EventDispatcher.Inst.RegistFn<Vector3,Vector3>(EventMazeGame.OnLeaveGround, OnUpdateVeolcityTrigger);
        EventDispatcher.Inst.Regist<bool>(EventMazeGame.OnVelocityUpdating,OnFactoryVelocityUpdating);
        _vecHandle = VecFactory.CreateVec(ASkateConst.VeclocityHandleKey);
        _vecHandle.Init(this);
        
        Motor = GetComponent<KinematicCharacterMotor>();
        Motor.CharacterController = this;
        Debug.Log("[Init] Motor.CharacterController Done");

        #region FSM of Player
        fsm = new StateMachineBuilder() 
            .State<StateIdle>("Idle").Enter(state=>
            {
                _animator.Play("Idle");
            }).Exit(state =>
            {
                
            })
            .End()
            .State<StateRun>("Run").Enter(state =>
            {
                _animator.Play("RunForward");
            }).Exit(state =>
            {
            })
            .End()
            .State<StateRunCrouchRaise>("RunCrouchRaise").Enter(state =>
            {
                _animator.Play("RunCrouchRaise");
            }).End()
            .State<StateDead>("Dead").Enter(state =>
            {
                state.OnEnter(ref _animator);
                Debug.LogError("Dead 死亡状态 --- after chk" + _animator.enabled);
            }).Exit(state =>
            {
                state.OnExit(_animator);
            })
            .End()
            .State<StateRebone>("Reborn").Enter(state =>
            {
                state.OnEnter(_animator.transform);
            })
            .End()
            .Build();
        
        fsm.ChangeState("Idle");
        //fsm.ChangeState("Dead");
        #endregion

    }

    private void OnDestroy()
    {
        EventDispatcher.Inst.UnRegistFn<Vector3,Vector3>(EventMazeGame.OnLeaveGround, OnUpdateVeolcityTrigger);
        EventDispatcher.Inst.UnRegist<bool>(EventMazeGame.OnVelocityUpdating,OnFactoryVelocityUpdating);
    }

    void OnGUI()
    {
        GUIStyle fontStyle = new GUIStyle();
        fontStyle.normal.background = null;    //设置背景填充
        fontStyle.normal.textColor= new Color(1,0,0);   //设置字体颜色
        fontStyle.fontSize = 40;       //字体大小
        
        //GUILayout.Label($"MyIK.cs打印：vec={_logCurrentVelocity}({_logCurrentVelocity.sqrMagnitude})",fontStyle);
    }


    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if(debugLog) Debug.LogWarning("UpdateRotation");
      //  if (fsm.IsState("Dead")) return;
        switch (CurrentCharacterState)
        {
            case CharacterState.Skate:
            case CharacterState.Default:
                {
                    if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
                    {
                        // Smoothly interpolate from current to target look direction
                        Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
        
                        // Set the current rotation (which will be used by the KinematicCharacterMotor)
                        currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                    }
        
                    Vector3 currentUp = (currentRotation * Vector3.up);
                    if (BonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    }
                    else if (BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);
        
                            Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;
        
                            // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                            Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                        }
                    }
                    else if (BonusOrientationMethod == BonusOrientationMethod.SkateAndAir)
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            DoRotateInGround(ref currentRotation, currentUp,deltaTime);
//                            Debug.LogError("motor <color=gray>velocity=" + Motor.Velocity.sqrMagnitude + "</color>");
                        }
                        else
                        {
  //                          Debug.LogError("motor velocity=" + Motor.Velocity.sqrMagnitude);
                            if (RevertCamTrigger && Motor.Velocity.sqrMagnitude < RevertCamVelocityCap)
                            {
                                //Debug.LogWarning("判断人是否有旋转 motor rotate");
                                currentRotation = Quaternion.LookRotation(new Vector3(0, -1, 0), Motor.CharacterUp);
                                RevertCamTrigger = false;
                            }
                        }
                    }
                    else
                    {
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    }
                    break;
                }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentRotation"></param>
    /// <param name="currup">rotation * Vector3.Up?? 不确定，是否当前旋转，再乘以90度（向上)？而且，和 Motor.CharacterUp 区别是？？ </param>
    public void DoRotateInGround(ref Quaternion currentRotation,Vector3 currUp,float deltaTime)
    {
        Vector3 initialCharacterHemiCenter = Motor.TransientPosition + (currUp * Motor.Capsule.radius);
        Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal,
            1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
        currentRotation = Quaternion.FromToRotation(currUp, smoothedGroundNormal) * currentRotation;
        // Move the position to create a rotation around the bottom hemi center instead of around the pivot
        Motor.SetTransientPosition(initialCharacterHemiCenter + (currentRotation * Vector3.down*Motor.Capsule.radius));
    }
    //
    // /// <summary>
    // /// 是否在平地
    // /// </summary>
    // public bool IsWorldGround
    // {
    //     get
    //     {
    //         return transform.position.y <= _standHeight;
    //     }
    //     
    // }

   

    public void HandleJump(ref Vector3 currentVelocity, float deltaTime)
    {
        _jumpedThisFrame = false;
        _timeSinceJumpRequested += deltaTime;
        if (_jumpRequested)
        {
            // See if we actually are allowed to jump
            if (!_jumpConsumed &&
                ((AllowJumpingWhenSliding
                     ? Motor.GroundingStatus.FoundAnyGround
                     : Motor.GroundingStatus.IsStableOnGround) ||
                 _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
            {
                // Calculate jump direction before ungrounding
                Vector3 jumpDirection = Motor.CharacterUp;
                if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                {
                    jumpDirection = Motor.GroundingStatus.GroundNormal;
                }

                Debug.LogError("jumpDirection " + jumpDirection);
                // Makes the character skip ground probing/snapping on its next update. 
                // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                Motor.ForceUnground();

                // Add to the return velocity and reset jump state
                currentVelocity += (jumpDirection * JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                currentVelocity += (_moveInputVector * JumpScalableForwardSpeed);
                _jumpRequested = false;
                _jumpConsumed = true;
                _jumpedThisFrame = true;
            }
        }
    }

    public Vector3 DumpGroundEdgeNormal()
    {
        var d1 = Vector3.Dot(Vector3.up, Motor.GroundingStatus.OuterGroundNormal);
        var d2 = Vector3.Dot(Vector3.up, Motor.GroundingStatus.InnerGroundNormal);
        if (d1 > d2)
            return Motor.GroundingStatus.OuterGroundNormal;
        else
        {

            return Motor.GroundingStatus.InnerGroundNormal;
        }
    }

    void OnFactoryVelocityUpdating(bool isGroundLogic)
    {
        this.isGroundUpdating = isGroundLogic;
    }

    //private bool _logSwitchFirst;
    Vector3 OnUpdateVeolcityTrigger(Vector3 currentVelocity)
    {
    
        //Motor.SetPosition();
        
        //当速度做出改变，则说明“飞起”
        var h = currentVelocity.y * currentVelocity.y / (2 * Gravity.y); //Gravity 自定义，并且已带方向，所以最后要取绝对值
        h = Mathf.Abs(h);
        _logFlyHeight = h;
        _logFlyStartPoint = Motor.transform.position;
        _logFlyStartVeclocity = currentVelocity;
        _logFlyStartVeclocityFixed = DumpFixedFlyVec(currentVelocity);
        

        return _logFlyStartVeclocityFixed;
    }

    public Vector3 DumpFixedFlyVec(Vector3 vec)
    {
        //var vecProject = Vector3.ProjectOnPlane(vec, Vector3.right);//这个  Vector3.right 不准确，实际上是垂直于轨道切面的法线，所以也就是 CharacterUp
        var vecProject = Vector3.ProjectOnPlane(vec, GetRampTangentNormal());
        vec += vecProject - vec;
        return vec;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if(debugLog) Debug.LogWarning("UpdateVelocity");
        // if(MyPlayer.Inst.IsControlling)
        //     Debug.LogWarning("UpdateVelocity 一次,ground= " + Motor.GroundingStatus.IsStableOnGround + " FixedConsumed=" + _logFixedConsumed);
        
        _vecHandle.HandleUpdate(ref currentVelocity,deltaTime);

        //_logCurrentVelocity = currentVelocity;

        // if (!_logSwitchFirst)
        // {
        //     AddLogSwitchPoint(Motor.transform.position);
        //     _logSwitchFirst = true;
        // }



        // //处理，启动，停止 TODO:现在不是太准确，这个暂时实现声音功能而已
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
        //

    }
    

    

    public bool _isLastLand;
    public void PostGroundingUpdate(float deltaTime)
    {
        if(debugLog) Debug.LogWarning("Update --- PostGroundingUpdate");
        // if(MyPlayer.Inst.IsControlling)
        //     Debug.LogError("PostGrounded一次，_isLastLand=" + _isLastLand  +" stable="+Motor.GroundingStatus.IsStableOnGround);
        
        _vecHandle.HandlePostGround();
//         //离地时
//         if (_isLastLand == true && Motor.GroundingStatus.IsStableOnGround == false)
//         {
//             _logSpeedGround = Motor.GroundingStatus;
//             _logSpeedGroundInner = Motor.GroundingStatus.InnerGroundNormal;
//             _logSpeedGroundOutter = Motor.GroundingStatus.OuterGroundNormal;
//             _logSpeedMotorPosition = Motor.GroundingStatus.GroundPoint;
//             _logSpeedLastGroundPos = Motor.LastGroundingStatus.GroundPoint;
//             _logFixedConsumed = false;
//             
//             // //_logFixedVelocity = Quaternion.Euler(0f,Vector3.Angle(Vector3.up, _logCurrentVelocity), 0) * Vector3.right;
//             // var angle = Vector3.Angle(_logCurrentVelocity,Vector3.up); //这是一个负数？？？
//             // _logFixedVelocity = Quaternion.AngleAxis(-angle, Vector3.left) * _logCurrentVelocity;
//             // //和上面的方法是一样的。。。。。
//             // // var angle = Vector3.Angle(Vector3.up, _logCurrentVelocity);
//             // // _logFixedVelocity = Quaternion.AngleAxis(angle, Vector3.right) * _logCurrentVelocity;
//             // _logFixedVelocity *= 1.3f;
//             // //unground....
//             // Debug.LogError("leave ground " + Vector3.Angle(Vector3.up,_logCurrentVelocity));
//             MyPlayer.Inst.m_StartLeaveGround =true;
//             Debug.LogWarning("设 m_StartLeaveGround 一次");
//             Motor.ForceUnground(0.2f);
//         }
//         //接触地面时
//         else if (_isLastLand == false && Motor.GroundingStatus.IsStableOnGround == true)
//         {
//             _logFixGroundedConsumed = false;
//             //landing....
//             //这里启动 awake()时也会触发一次
// //            Debug.LogError("接触地面时 ,打印落地shi速度：" + Motor.Velocity);
//         }
//
//         _isLastLand = Motor.GroundingStatus.IsStableOnGround;
    }
    
    public static Vector3 RotateRound(Vector3 position, Vector3 center, Vector3 axis, float angle)
    {
        Vector3 point = Quaternion.AngleAxis(angle, axis) * (position - center);
        Vector3 resultVec3 = center + point;
        return resultVec3;
    }

    //private Vector3 _characterLastNormal;

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _vecHandle.BeforeCharacterUpdate(deltaTime);
        // // _characterLastNormal = Motor.CharacterUp;
        // // //_characterLastNormal = Motor.Velocity;
        // beforeUp = Motor.CharacterUp;
        // if (Motor.transform.position.y > beforePosition.y)
        //     beforeUpAccumulate++;
        // else
        // {
        //     beforeUpAccumulate = 0;
        // }
        //
        // beforePosition = Motor.transform.position;
        
    }

    //public bool triggerLeaveTurnalConsume = false;
    public void AfterCharacterUpdate(float deltaTime)
    {
        _vecHandle.AfterCharacterUpdate(deltaTime);

        // var d2 = Vector3.Dot(Motor.CharacterUp, Vector3.up);
        // if (Vector3.Dot(beforeUp, Vector3.up) < d2
        //     //&& d2 > 0.8f
        //     && beforeUpAccumulate>5
        //     && !triggerLeaveTurnalConsume
        //     ) //扇形由大到小
        //     // if (Vector3.Dot(_characterLastNormal,   Motor.CharacterUp)<0.8f)
        // {
        //     Debug.LogError("测 accumulate=" + beforeUpAccumulate);
        //     _logSwitchPoints.Add(Motor.transform.position);
        //     triggerLeaveTurnalConsume = true;
        // }  
        
        

        switch (CurrentCharacterState)
        {
            case CharacterState.Skate:
            case CharacterState.Default:
            {
                if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                {
                    // If we're on a ground surface, reset jumping values
                    if (!_jumpedThisFrame)
                    {
                        _jumpConsumed = false;
                    }
                    _timeSinceLastAbleToJump = 0f;
                }
                else
                {
                    // Keep track of time since we were last able to jump (for grace period)
                    _timeSinceLastAbleToJump += deltaTime;
                }
                break;
            }

                
        }
        
        
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        //throw new System.NotImplementedException();
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        //throw new System.NotImplementedException();
    }
    public class HitResult
    {
        public Collider collider;
        public Vector3 normal;
        public Vector3 pos;
    }

    public List<HitResult> m_MovementHits = new List<HitResult>();
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        
        if(debugLog) Debug.LogWarning("OnMovementHit() 这个也是容易多次触发");
        //if (fsm.IsState("Dead")) return;//所以需要限制条件。。。。\
        if(fsm.IsState("Reborn")) return;
        // //死亡判断- 判断死亡
        // if (Vector3.Dot(transform.up, Vector3.up) >= _standHitRad) //只要人朝上，即正常
        // {
        //     return;
        // }

        int lastCount = DelayHelper.Inst.GetUpdateCount(10);
        int getCount = DelayHelper.Inst.UpdateCount(10);
        if (getCount <= 1)
        {
            MyPlayer.Inst.HitSceneStr = "OnMovementHit ";
        }
        else
        {
            MyPlayer.Inst.HitSceneStr = "OnMovementHit x" +getCount;
            //多于一次时才需要记录
        }
        MyPlayer.Inst.HitScenePoint = hitPoint;
        HitResult hithit = new HitResult();
        hithit.collider = hitCollider;
        hithit.normal = hitNormal;
        hithit.pos = hitPoint;
        //记录(SceneView 用)
        if (lastCount == getCount)
        {
            m_MovementHits.Clear();
        }
        m_MovementHits.Add(hithit);


        if (ASkateConst.UseOldDeadHandle)
        {
            if (Vector3.Dot(Motor.CharacterUp, hitNormal) >= _standHitRad)
            {
                //     Debug.LogError($"OnMovementHit <color=yellow>name={hitCollider.name}</color>");
            }
            else
            {

                Debug.LogError($"{Motor.CharacterUp} dot {hitNormal} = {Vector3.Dot(Motor.CharacterUp, hitNormal)}");
                //    Debug.LogError($"OnMovementHit name={hitCollider.name}");
                if (fsm.IsState("Dead") == false)
                    fsm.ChangeState("Dead");
            }
        }
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
       // throw new System.NotImplementedException();
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
       // throw new System.NotImplementedException();
    }

    //public MyPlayer _controller;
    // public void InitController(MyPlayer controller)
    // {
    //     _controller = controller;
    // }

    /// <summary>
    /// 写的有些“内聚",所以这方法，只能在Update()调用，也确实需要一直 UpdateInput()
    /// </summary>
    /// <param name="inputs"></param>
    public void SetInputs(ref PlayerCharacterInputs inputs)
    {
        // Clamp input
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        switch (CurrentCharacterState)
        {
            case CharacterState.Skate:
            {
                //_moveInputVector = cameraPlanarRotation * moveInputVector;
                _lookInputVector = cameraPlanarRotation * moveInputVector;
                //注意！！这个方法必须在Update()内调用
                //要做摇杆也不好做
                //直接在这里接手柄 插件，也不是不可以，到时再算吧
                if (Input.GetKey(KeyCode.W))
                {
                    if (fsm.IsState("RunCrouchRaise"))
                    {
                        _moveInputVector = Vector3.zero;    //平地蹲下，没有速度
                    }
                    else
                    {
                        _moveInputVector = Motor.CharacterForward;//平地滑行，单脚速度
                    }
                }
                else
                {
                    _moveInputVector = Vector3.zero;    //没有操作，没有速度
                }

                if (inputs.JumpDown)//还原Jump input(暂时其实还没需要滑板+跳跃）
                {
                    _timeSinceJumpRequested = 0f;
                    _jumpRequested = true;
                }

                break;
            }
            case CharacterState.Default:
                {
                    // Move and look inputs
                    _moveInputVector = cameraPlanarRotation * moveInputVector;
                    //Debug.LogError("Update every " +_moveInputVector);
                    switch (OrientationMethod)
                    {
                        case OrientationMethod.TowardsCamera:
                            _lookInputVector = cameraPlanarDirection;
                            break;
                        case OrientationMethod.TowardsMovement:
                            _lookInputVector = _moveInputVector.normalized;
                            break;
                    }

                    // Jumping input
                    if (inputs.JumpDown)
                    {
                        _timeSinceJumpRequested = 0f;
                        _jumpRequested = true;
                    }

                    // // Crouching input
                    // if (inputs.CrouchDown)
                    // {
                    //     _shouldBeCrouching = true;
                    //
                    //     if (!_isCrouching)
                    //     {
                    //         _isCrouching = true;
                    //         Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
                    //         MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
                    //     }
                    // }
                    // else if (inputs.CrouchUp)
                    // {
                    //     _shouldBeCrouching = false;
                    // }

                    break;
                }
        }
        
        //判断是否停止 或开始移动
        if (fsm.IsState("Dead")==false)
        {
            // if (Input.GetKey(KeyCode.W))
            // {
            //     //TODO:判断水平面高度(可能不在原点）
            //     if (transform.position.y < 0.75f && (fsm.IsState("Idle") || fsm.IsState("Run") ||
            //                                          _logCurrentVelocity.sqrMagnitude <= 0.2f))
            //     {
            //         if (fsm.IsState("Run") == false)
            //             fsm.ChangeState("Run");
            //     }
            //     else
            //     {
            //         if (fsm.IsState("RunCrouchRaise") == false)
            //             fsm.ChangeState("RunCrouchRaise");
            //     }
            // }
            // else
            {
                //if (Motor.GroundingStatus.IsStableOnGround == true)
                if (fsm.IsState("Idle") == false)
                {
                    fsm.ChangeState("Idle");
                }
            }
        }
    }
     /// <summary>
     /// This is called every frame by the AI script in order to tell the character what its inputs are
     /// </summary>
     public void SetInputs(ref AICharacterInputs inputs)
     {
         _moveInputVector = inputs.MoveVector;
         _lookInputVector = inputs.LookVector;
     }
}

