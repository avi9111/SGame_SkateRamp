using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using Unity.Mathematics;
using UnityEngine;
/// <summary>
/// MagentMover.cs(待改名）
/// </summary>
public class MagentMotor : KinematicCharacterMotor
{
   // private Rigidbody rigidbody;
    protected override void Start()
    {
        //rigidbody = GetComponent<Rigidbody>();
    }
    /*TODO 应该实现 继承 Simulate1 或 Simulate2
     但是真的不熟悉Motor,所以还是算了。。。。另外再写一个好了。。。。磁力“上杆”
     */
    // public override void VelocityUpdate(float deltaTime)
    // {
    //     SelfUpdateMovement(out var initialSimulationPosition, out var initialSimulationRotation, deltaTime);
    //
    //     if (deltaTime > 0f)
    //     {
    //         Rigidbody.velocity = (TransientPosition - initialSimulationPosition) / deltaTime;
    //                             
    //         Quaternion rotationFromCurrentToGoal = TransientRotation * (Quaternion.Inverse(initialSimulationRotation));
    //         Rigidbody.angularVelocity = (Mathf.Deg2Rad * rotationFromCurrentToGoal.eulerAngles) / deltaTime;
    //     }
    // }
    //
    // void SelfUpdateMovement(out Vector3 position, out Quaternion rot,float deltaTime)
    // {
    //     position = Vector3.zero;
    //     rot = Quaternion.identity;
    //     
    //     
    // }
}
