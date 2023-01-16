using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.SceneManagement;

public class GuAliGame : MonoBehaviour
{
    public List<GameObject> CharacterObjs = new List<GameObject>();
    private MyPlayer _mainPlayer;
    [Header("关卡")]
    public int _startPoint = 1;
    public GameObject _startPointContainer;

    private void Awake()
    {
        _mainPlayer = GetComponent<MyPlayer>();
        if (_mainPlayer == null)
        {
            Debug.LogError("_mainPlayer == null 请检查，卡能造成切换场景时错误");
        }

        if (_startPointContainer != null)
        {
            //_mainPlayer是 Bridge 而已，Ctl才是实体
            var startPoint = _startPointContainer.transform.Find("StartCube" + _startPoint);
            //这个设置方法只能放在Awake，放在Start（）会和 Motor，MyIK等抢Position 控制权
            _mainPlayer.CtlIK.transform.position = startPoint.transform.position;
            _mainPlayer.CtlIK.transform.rotation = startPoint.transform.rotation;
        }
        else
        {
            Debug.LogWarning("_startPointContainer == null 请检查 in GuAliGame.cs");
        }
    }

    private void Start()
    {
        foreach (var cha in CharacterObjs)
        {
            DontDestroyOnLoad(cha);
        }
        
        
    
    }

    public void GoToVilliageScene()
    {
        StartCoroutine(StartLoadVilliage());
    }

    IEnumerator StartLoadVilliage()
    {
        _mainPlayer.isPauseUpdate = true;
        var ret =  SceneManager.LoadSceneAsync("SnowGuScene", LoadSceneMode.Single);
        while (!ret.isDone)
        {
            Debug.LogError("loading gu scene");
            yield return new WaitForSeconds(0.1f);
        }
        
        
        //配置镜头 
        // --- 场景1
        //var myCam = GameObject.Find("Camera").GetComponent<ExampleCharacterCamera>();
        var myCam = Camera.main.GetComponent<ExampleCharacterCamera>();
        _mainPlayer.Cam = myCam;
        _mainPlayer.isPauseUpdate = false;
        
        _mainPlayer.ReDoStart();
        //选择出生点 2选1 或 3选1


    }
}
