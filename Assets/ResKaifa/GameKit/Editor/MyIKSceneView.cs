using System.Collections;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using ResKaifa.GameKit;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

/// <summary>
/// 为了SkateRamp游戏，做的一些 SceneView 扩展（主要是个人爽，作用一般）;一些功可能做了，也可能没做的参考如下：
/// gui控制： http://t.zoukankan.com/littlebao-p-12359258.html;
/// sceneView功能小记：https://www.jianshu.com/p/361cf5c643e1；
/// </summary>
public class MyIKSceneView
{
    private static int currDrawHitCursor = -1;
    static GUIStyle styleLabel = new GUIStyle();
    [InitializeOnLoadMethod]
    public static void EditorInit () {
        //SceneView.beforeSceneGui += BeforeSceneGUI;
        SceneView.duringSceneGui += DuringSceneGUI;
        // EditorSceneManager.activeSceneChangedInEditMode += (sceneA, sceneB) => {
        //     if (EditingRenderer != null) {
        //         EndEdit();
        //     }
        // };
        EditorApplication.update += OnSceneUpdate;
        //EditorApplication.hierarchyWindowItemOnGUI += HierarchyGUI;
        //Undo.undoRedoPerformed += UndoRedoPerformed;
        RemoveAllMyEditingRoot();
        
        //ToolCount = System.Enum.GetNames(typeof(JujubeTool)).Length;

        styleLabel.fontSize = 33;
    }

    static void RemoveAllMyEditingRoot()
    {
        // if(DelayHelper.Inst!=null)
        //     DelayHelper.Inst.Clear();
    }

    static void OnSceneUpdate()
    {
     
    }

    private static void DuringSceneGUI(SceneView sceneView)
    {
        //if(ap)
         //Event.current.mousePosition
         DrawGUI(sceneView);
         DrawCursor(sceneView);

         if (currDrawHitCursor >= 0)
         {
             var hit = MyPlayer.Inst.CtlIK.m_MovementHits[currDrawHitCursor];
             var endPos = hit.pos + hit.pos + hit.normal * 3;
             Handles.DrawLine(hit.pos, endPos, 2.1f);
             //Handles.DrawLine(hit.pos, hit.pos+ Vector3.up * 6 );
             
             Handles.Label(endPos, "" + currDrawHitCursor + "/" + MyPlayer.Inst.CtlIK.m_MovementHits.Count);
         }
         
         //画线平地，或是否离地等
         DrawPlayerPath();
    }

    static void DrawPlayerPath()
    {
        if (MyPlayer.Inst == null) return;
        var defColor = Handles.color;

        for (int i = 0; i < MyPlayer.Inst.lstOfPath.Count - 1; i++)
        {
            var pathInfo = MyPlayer.Inst.lstOfPath[i];
            var p1 = pathInfo.pos;
            var p2 = MyPlayer.Inst.lstOfPath[i + 1].pos;
            if (pathInfo.isGround)
                if (pathInfo.isGroundLogic)
                    Handles.color = GizmosTools.ColorBlue2;
                else
                    Handles.color = Color.green;
            else 
                if (pathInfo.isGroundLogic)
                    Handles.color = GizmosTools.ColorMagenta2;
                else
                    Handles.color = Color.red;
            Handles.DrawLine(p1,p2);
        }
        
        Handles.color = defColor;
    }

    static void DrawGUI(SceneView view)
    {
        Handles.BeginGUI();
        // GUILayout.BeginArea(sceneView.position); // 规定显示区域为屏幕大小
        GUILayout.BeginArea(new Rect(20, 320, 150, 60));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<"))
        {
            MoveNextCollider(0);
        }

        if (GUILayout.Button(">"))
        {
            MoveNextCollider(1);
        }

        GUILayout.EndHorizontal();
        //
        // // -----------------------------------------------
        // //在分割线内，添加代码
        // var rect = new Rect(point, new Vector2(100, 28));
        // if(GUI.Button(rect,"Hello World"))
        // {
        //     Debug.Log("Hello world");
        // }
        // // -----------------------------------------------
        //
        GUILayout.EndArea();
        
        
        
        Handles.EndGUI();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="next">0-上一个;1-下一个</param>
    static void MoveNextCollider(int next)
    {
        //if (!(next == 0 || next != 1))
        if(next!=0 && next!=1)
        {
            Debug.LogError("只接收参数1,0");
            return;
        }

        if (MyPlayer.Inst != null && MyPlayer.Inst.CtlIK != null)
        {
            if (MyPlayer.Inst.CtlIK.m_MovementHits.Count>0)
            {
                int cursor = currDrawHitCursor;
                if (next == 1)
                {
                    if (cursor < MyPlayer.Inst.CtlIK.m_MovementHits.Count)
                    {
                        cursor++;
                    }
                    else
                    {
                        cursor = 0;
                    }
                }
                else
                {
                    if (cursor <= 0)
                    {
                        cursor = MyPlayer.Inst.CtlIK.m_MovementHits.Count - 1;
                    }
                    else
                    {
                        cursor--;
                    }
                }

                currDrawHitCursor = cursor;

           
            }
        }
    }

    static void DrawCursor(SceneView sceneView)
    {
//         // 当前屏幕坐标，左上角是（0，0）右下角（camera.pixelWidth，camera.pixelHeight）
//         Vector2 mousePosition = Event.current.mousePosition;
//
//         // Retina 屏幕需要拉伸值
//         float mult = 1;
// #if UNITY_5_4_OR_NEWER
//         mult = EditorGUIUtility.pixelsPerPoint;
// #endif
//
//         // 转换成摄像机可接受的屏幕坐标，左下角是（0，0，0）右上角是（camera.pixelWidth，camera.pixelHeight，0）
//         mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y * mult;
//         mousePosition.x *= mult;
//
//         // 近平面往里一些，才能看得到摄像机里的位置
//         Vector3 fakePoint = mousePosition;
//         fakePoint.z = 20;
//         Vector3 point = sceneView.camera.ScreenToWorldPoint(fakePoint);

        
        //画线
        //Handles.DrawLine(point, point + Vector3.up * 3);
        //画球
        //Handles.SphereHandleCap();


        if (MyPlayer.Inst != null)
        
            Handles.Label(MyPlayer.Inst.HitScenePoint, MyPlayer.Inst.HitSceneStr,styleLabel);{
            
        }

        
    
        
  
        
       // Handles.SphereCap(0, point, Quaternion.identity, 2);

        // 刷新界面，才能让球一直跟随
        //sceneView.Repaint();
        //HandleUtility.Repaint();

    }
}
