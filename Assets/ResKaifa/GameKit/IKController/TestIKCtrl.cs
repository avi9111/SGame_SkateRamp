using UnityEngine;
using System;
using System.Collections;
 
[RequireComponent(typeof(Animator))]
public class TestIKCtrl : MonoBehaviour {
    //动画控制
    protected Animator animator;
    //是否开始IK动画
    public bool ikActive = false;
    //右手子节点参考的目标
    public Transform rightHandObj = null;
 
    void Start () 
    {
        //得到动画控制对象
        animator = GetComponent<Animator>();
    }
 
    //a callback for calculating IK
    //它是回调访法。
    //前提是在Unity导航菜单栏中打开Window->Animator打开动画控制器窗口，在这里必须勾选IK Pass！！！
    void OnAnimatorIK()
    {
        if(animator) 
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            //即或IK动画后开始让右手节点寻找参考目标。 
            if(ikActive) 
            {
                //weight = 1.0 for the right hand means position and rotation will be at the IK goal (the place the character wants to grab)
                //设置骨骼的权重，1表示完整的骨骼，如果是0.5哪么骨骼权重就是一半呢，可移动或旋转的就是一半，大家可以修改一下参数试试。
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,1f);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,1f);
 
                //set the position and the rotation of the right hand where the external object is
                if(rightHandObj != null) 
                {
                    //设置右手根据目标点而旋转移动父骨骼节点
                    animator.SetIKPosition(AvatarIKGoal.RightFoot,rightHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot,rightHandObj.rotation);
                }					
 
            }
            //if the IK is not active, set the position and rotation of the hand back to the original position
            //如果取消IK动画，哪么重置骨骼的坐标。
            else 
            {			
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,0);				
            }
        }
    }	  
}