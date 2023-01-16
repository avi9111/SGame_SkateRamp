using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using PigeonCoopToolkit.Effects.Trails;
using ResKaifa.GameKit;
using Unity.Mathematics;
using UnityEngine;
/// <summary>
/// 应该是参考 Kinermatic 插件的 MyPlayer,但其实很多速度等功能都是“内聚”因为 MyIK.cs挺多处理的（Kinematic本身没做封装，只提供底层逻辑）;
/// 现在多了些功能，例如 Presistant 记录调试好的速度，一些实现等。。。
/// </summary>
public class MyPlayer : MonoBehaviour
{
    public enum CameraType
    {
        默认镜头,
        跟随镜头
    }

    public class MyPlayerPath
    {
        public bool isGround;
        public Vector3 pos;
        public bool isGroundLogic;
    }
    [Header("滑板的物理调试")]
    public float UpSensitive = 3f;
    public List<MyPlayerPath> lstOfPath = new List<MyPlayerPath>();

    public ExampleCharacterCamera Cam;
    public MyIK CtlIK;
    public KinematicCharacterMotor Motor => CtlIK.Motor;

    public Transform CamFollowPoint;
    private const string MouseXInput = "Mouse X";
    private const string MouseYInput = "Mouse Y";
    private const string MouseScrollInput = "Mouse ScrollWheel";
    private const string HorizontalInput = "Horizontal";
    private const string VerticalInput = "Vertical";
    /// <summary>
    /// 现在MyIK.cs越来愈完善，这个很少用了，，（原Sample控制器，参考用
    /// </summary>
    [Header("留空，测试用")]
    public ExampleCharacterController Character;
    /// <summary>
    /// VerticalSensity 会影响转向，但是不会影响速度，无法加速
    /// </summary>
    [Header("输入速度控制")]
    public float VerticalSensity = 1f;
    public float AltMultiply = 1f;
    private float defaultMaxMoveSpeed;
    private float defaultMoveSharpness;

    [Header("镜头控制？？")]
    public CameraType _camType;

    public float CamFollowSharpness = 3f;
    /// <summary>
    /// 停止左右控制，貌似之前在切换场景时用
    /// </summary>
    public bool isPauseUpdate;

    private bool isDeadReset = true;
    private float _lastDeadTime;
    [Header("死亡控制")]
    [Tooltip("死亡重新最开始事件，可能默认+0.5f停顿")]
    public float _reboneTime = 1.0f;

    [Header("音效")] public AudioSource _audio;
    public AudioClip _clipFall;
    public AudioClip _clipNormalSkate;
    [Header("特效")]
    [Tooltip("Trail == 痕迹 轨道")]
    public Trail _trail;

    public static MyPlayer Inst;
    public bool m_IsStopInput;
    public Vector3 m_lastGroundPos;
    //记录碰撞 OnMovementHit点，应该可以去掉了。。。
    public string HitSceneStr;
    public Vector3 HitScenePoint;
    private float _lastInputTime;
    /// <summary>
    /// 玩家按了AWSD其中一个按键，即开始；只要玩家没有操作3秒，即停止
    /// </summary>
    public bool IsControlling { get; private set; }

    public Vector3 Position => CtlIK.transform.position;//可用自己的tf，也可以之后改用Ctrl.tf
    private void Awake()
    {
        Inst = this;
    }

    /// <summary>
    /// Setup Camera
    /// </summary>
    void Start()
    {
        DelayHelper.Inst.Clear();//每次启动游戏，要清理一下（计数需要重新算）
        //暂时不知道这个什么作用
        Cursor.lockState = CursorLockMode.Locked;
        
        ReDoStart();

     //   Ctl.InitController(this);

    }
    /// <summary>
    /// 切换场景时，人物没消除，但是cam 已经切换，会为Null;所以每次切换场景后，都要调用这个方法
    /// </summary>
    public void ReDoStart()
    {
        // Tell camera to follow transform
        Cam.SetFollowTransform(CamFollowPoint);
        Debug.Log("[Init]CameraFollowPointSet");

        // Ignore the character's collider(s) for camera obstruction checks
        Cam.IgnoredColliders.Clear();
        //可以把自己加上，避免碰撞（向前凸凸移动镜头, 现在屏蔽了好像也没啥影响）
        //Cam.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
    }

    /// <summary>
    /// How Input transform into movement
    /// </summary>
    void Update()
    {
        if (isPauseUpdate) return;

        if (Input.GetMouseButtonDown(0))
            Cursor.lockState = CursorLockMode.Locked;

        if (_camType == CameraType.跟随镜头)
           HandleSkateCameraAutoInput();
        else
           HandleCameraInput();

        HandleCharacterInput();
        HandleHotKey();

        HandleDead();
    }

    // void SaveInGroundPos()
    // {
    //     Debug.LogError("SaveIngroundPos " + Ctl.IsWorldGround);
    //     if(Ctl.IsWorldGround)
    //         m_lastGroundPos = Ctl.transform.position;
    // }

    /// <summary>
    /// 死亡判定 Loop
    /// </summary>
    void HandleDead()
    {
        // //每5秒记录一次平地，重生用
        // if(m_IsStopInput==false)
        //     DelayHelper.Inst.Update("saveGroundPos",ASkateConst.SaveLastPosInterval,SaveInGroundPos);
        
        if (CtlIK.fsm == null) return;
        if (CtlIK.fsm.IsState("Dead"))
        {
            if (isDeadReset)
            {
                isDeadReset = false;
                _lastDeadTime = Time.realtimeSinceStartup;
            }
            
            
        }

        if (isDeadReset == false && Time.realtimeSinceStartup - _lastDeadTime > _reboneTime)
        {
            CtlIK.fsm.ChangeState("Reborn");
            isDeadReset = true;
        }
    }

    void HandleHotKey()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            defaultMaxMoveSpeed = CtlIK.MaxStableMoveSpeed;
            defaultMoveSharpness = CtlIK.StableMovementSharpness;
            CtlIK.MaxStableMoveSpeed = defaultMaxMoveSpeed * AltMultiply;
            CtlIK.StableMovementSharpness = defaultMoveSharpness * AltMultiply;
            //Character.max  
            
        }
        else if(Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
        {
            CtlIK.MaxStableMoveSpeed = defaultMaxMoveSpeed;
            CtlIK.StableMovementSharpness = defaultMoveSharpness;
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GetComponent<GuAliGame>().GoToVilliageScene();
        }

    }

    /// <summary>
    /// 输入镜头 Loop
    /// </summary>
    void HandleSkateCameraAutoInput()
    {
        //var targetDir = Vector3.ProjectOnPlane(Ctl.Motor.CharacterForward, Vector3.up);
        //var currDir = Vector3.ProjectOnPlane(Cam.Transform.forward, Vector3.up);
        var targetDir = Vector3.ProjectOnPlane(CtlIK.Motor.CharacterForward, CtlIK.Motor.CharacterUp);
        var currDir = Vector3.ProjectOnPlane(Cam.Transform.forward, CtlIK.Motor.CharacterUp);
        Vector3 mouseMove = Vector3.zero;
        float scrollInput = -Input.GetAxis(MouseScrollInput);
        currDir = Vector3.Lerp(currDir, targetDir, 1 - Mathf.Exp(-CamFollowSharpness * Time.deltaTime));
        //var finalDir = Vector3.ProjectOnPlane(currDir, Vector3.up);
        //Cam.transform.rotation = quaternion.Euler(finalDir);
        float followAngle = 27 - Mathf.Rad2Deg * Vector3.Dot( Vector3.up,Vector3.ProjectOnPlane( CtlIK.Motor.CharacterForward,Vector3.right));
        
        Cam.UpdateWithInput(Time.deltaTime,scrollInput,mouseMove,currDir);

        
        //Cam.transform.rotation = Quaternion.LookRotation(currDir, Vector3.up);

    }

    private void HandleCameraInput()
    {
        // Create the look input vector for the camera
        float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
        float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
        Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

        // Prevent moving the camera while the cursor isn't locked
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            lookInputVector = Vector3.zero;
        }

        // Input for zooming the camera (disabled in WebGL because it can cause problems)
        float scrollInput = -Input.GetAxis(MouseScrollInput);
#if UNITY_WEBGL
        scrollInput = 0f;
#endif

        // Apply inputs to the camera
        Cam.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);

        // Handle toggling zoom level
        if (Input.GetMouseButtonDown(1))
        {
            Cam.TargetDistance = (Cam.TargetDistance == 0f) ? Cam.DefaultDistance : 0f;
        }
    }
    /// <summary>
    /// 输入移动 Loop
    /// </summary>
    private void HandleCharacterInput()
    {
        if (m_IsStopInput) return;
        PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

        // Build the CharacterInputs struct
        characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput) * VerticalSensity;
        characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
        characterInputs.CameraRotation = Cam.Transform.rotation;
        characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
        characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);//居然后蹲下？？？
        characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);

        if (CtlIK != null)
        {
            // Apply inputs to character
            CtlIK.SetInputs(ref characterInputs);
        }
        else
        {
            Character.SetInputs(ref characterInputs);
        }
        //////////////////// scenve view 显示用 ////////////////////////////////
        if (characterInputs.MoveAxisForward != 0 || characterInputs.MoveAxisRight != 0)
        {
            _lastInputTime = Time.realtimeSinceStartup;
            if (IsControlling == false)
            {
                lstOfPath.Clear();//操作第一下才清空，上一次记录（保持）
            }
            IsControlling = true;
            
        }
        else
        {
            if (Time.realtimeSinceStartup - _lastInputTime >= 3)
            {
                IsControlling = false;

            }
        }

        if (IsControlling)
        {
            var isGround = Motor.GroundingStatus.IsStableOnGround;
            bool y = CtlIK.isGroundUpdating;
            lstOfPath.Add(new MyPlayerPath(){pos= MyPlayer.Inst.Position,isGround = isGround,isGroundLogic=y});
        }
    }

    #region 一些回调事件，暂时只有音效

    public void OnLeaveGround(Vector3 velocity)
    {
//        Debug.LogError("OnLeaveGround");
        if(_audio==null) return;
        //_audio.clip = _clipNormalSkate;
        //_audio.Play();
        _audio.Stop();
    }

    public void OnEnterGround(Vector3 velocity)
    {
//        Debug.LogError("OnEnterGround");
        if(_audio==null) return;
        if (_audio.clip == _clipNormalSkate && _audio.isPlaying) return;
        _audio.clip = _clipNormalSkate;
        _audio.Play();
    }

    public void OnStartGround()
    {
        
  //      Debug.LogError("OnStartGround");
        if(_audio==null) return;
        if (_audio.clip == _clipNormalSkate && _audio.isPlaying) return;
        _audio.clip = _clipNormalSkate;
        _audio.Play();
    }

    public void OnStopGround()
    {
     //   Debug.LogError("OnStopGround");
        if(_audio==null) return;
        _audio.Stop();
    }

    #endregion

    public void RemoveTrail()
    {
        if (_trail != null) _trail.Emit = false;
    }

    public void EmmitTrail()
    {
        if (_trail != null) _trail.Emit = true;
    }

    public bool m_StartLeaveGround = false;

    /// <summary>
    /// updateGround()第一调用-》记录；updateVelocity() 时fixed 一次
    /// </summary>
    public void FixLeaveGroundVelocity(ref Vector3 velocity)
    {
        if (m_StartLeaveGround)
        {
            velocity.x = 0;
            velocity.z = 0;
            m_StartLeaveGround = false;
        }
        
    }

    public void StartKinematic()
    {
        CtlIK.enabled = true;
        //Ctl.Motor.enabled = true;
        m_IsStopInput = false;
    }

    public void StopKinematic()
    {
        //Ctl.Motor.enabled = false;
        CtlIK.enabled = false;
        m_IsStopInput = true;
    }
}
