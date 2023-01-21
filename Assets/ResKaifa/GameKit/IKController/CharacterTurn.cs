using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using EasyButtons;
using JetBrains.Annotations;
using KinematicCharacterController;
using ResKaifa.GameKit;
using Unity.Mathematics;
using UnityEngine;

public class CharacterTurn : MonoBehaviour
{
    public static CharacterTurn Inst;
    [NotNull]
    public Transform characterT;
    private void Awake()
    {
        Inst = this;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="motor">地表的 y 坐标轴需要为.y== 0</param>
    /// <returns></returns>
    public static VecBase.VecINSlope CheckForward(Vector3 velocity,KinematicCharacterMotor motor)
    {
        if (Mathf.Abs(velocity.sqrMagnitude) < 0.01 && motor.GroundingStatus.GroundPoint.y==0) return VecBase.VecINSlope.InGround;

        var rad = Vector3.Dot(velocity, motor.CharacterForward);
        if (rad == 0) return VecBase.VecINSlope.InGround;
        
        if (rad > 0) return VecBase.VecINSlope.Up;
        
        return VecBase.VecINSlope.Down;
        
    }

    public static void TurnLeft()
    {
        if (Inst == null) return;
        Inst.LeftTurn();
    }

    public static void TurnRight()
    {
        if (Inst == null) return;
        Inst.RightTurn();
    }

    public static void TurnForward()
    {
        if (Inst == null) return;
        Inst.ResetTurn();
    }

    Vector3 GetCharacterUP
    {
        get
        {

           // if (MyPlayer.Inst == null)
                return Vector3.up;
            //return MyPlayer.Inst.Motor.CharacterUp;
        }
    }

    Vector3 GetCharacterForward {
        get
        {
            //if (MyPlayer.Inst == null)
                return Vector3.zero;
            //return MyPlayer.Inst.Motor.CharacterForward;
        }
    }

    [Button]
    public void LeftTurn()
    {
        //應該用local.....因為在子节点 Transform
        characterT.localRotation = Quaternion.AngleAxis(-90, GetCharacterUP);
        //characterT.rotation = Quaternion.AngleAxis(-90,GetCharacterUP);
    }
    [Button]
    public void RightTurn()
    {
        //如果不用 localRoation，则要自行计算，和 KinematicCC 同步计算角度
        characterT.localRotation = Quaternion.AngleAxis(90, GetCharacterUP);
        //characterT.rotation = Quaternion.AngleAxis(90, GetCharacterUP);
    }
    [Button]
    public void ResetTurn()
    {
        //也不是计算不出全局，角度，就是比较麻烦，所以还是用 localRotation
        characterT.localRotation = Quaternion.Euler(GetCharacterForward);
        //characterT.rotation= Quaternion.Euler( GetCharacterForward);
    }
}
