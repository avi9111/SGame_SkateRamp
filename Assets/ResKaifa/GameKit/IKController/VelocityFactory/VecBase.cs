using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace ResKaifa.GameKit
{
    public class VecBase
    {
        protected MyIK _ik;

        public enum VecINSlope {
            Up,
            Down,
            InGround
        }

        /// <summary>
        /// 需要 PreSimulatePhysic（）配合
        /// </summary>
        /// <returns></returns>
        public VecINSlope stateRunUp
        {
            get
            {
                if (RunUpCount == 0)
                    return VecINSlope.InGround;
                
                if(RunUpCount > 0)
                    return VecINSlope.Up;
                
                return VecINSlope.Down;
            }
        }
        
        protected float RunUpCount;
    

        public virtual void Init(MyIK ik)
        {
            this._ik = ik;
        }
    
        protected Vector3 DumpFixedFlyVec(Vector3 vec)
        {
            //var vecProject = Vector3.ProjectOnPlane(vec, Vector3.right);//这个  Vector3.right 不准确，实际上是垂直于轨道切面的法线，所以也就是 CharacterUp
            var vecProject = Vector3.ProjectOnPlane(vec, _ik.GetRampTangentNormal());
            vec += vecProject - vec;
            return vec;
        }
        /// <summary>
        /// 投射到斜坡(Ramp)的平面速度
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        protected Vector3 DumpProjectVec(Vector3 vec)
        {
            return Vector3.ProjectOnPlane(vec, _ik.GetRampTangentNormal());
        }
        /// <summary>
        /// 投影计算参考：https://www.cnblogs.com/graphics/archive/2010/08/03/1791626.html
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public float CalProjectLength(Vector3 vec, Vector3 axis)
        {
            //var v = (axis.normalized * vec.magnitude);
            //var len = Vector3.Dot(vec, v) / v.magnitude;
            var len = Vector3.Dot(vec, axis) / vec.magnitude;
            
            //Debug.LogError("速度9？？？" + Vector3.Dot(Vector3.up, Vector3.up * 3));
            //Debug.LogError("速度" + len + " 乘积=" + Vector3.Dot(vec, v) + " vec=" + vec + " v=" + v + " vl=" +
            //               v.magnitude);
            return len; 
            //方向 == 。。。 return (Vector3.Dot(vec, axis) / axis);

        }

        public virtual void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public virtual void AfterCharacterUpdate(float deltaTime)
        {
            
        }

        public virtual void HandleUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
        }

        public virtual void HandlePostGround()
        {
            var Motor = _ik.Motor;
            if (Motor.LastGroundingStatus.IsStableOnGround != _ik._isLastLand)
            {
                Debug.LogError("测试引擎的lastGround和自定义last 不同 MotorLast=" + Motor.LastGroundingStatus.IsStableOnGround+" Motor=" + Motor.GroundingStatus.IsStableOnGround);
            }

    //         //离地时
    //         //if (_ik._isLastLand == true && Motor.GroundingStatus.IsStableOnGround == false)
    //         if(_ik.Motor.LastGroundingStatus.IsStableOnGround && _ik.Motor.GroundingStatus.IsStableOnGround == false)
    //         {
    //         //    _ik._logSpeedGround = Motor.GroundingStatus;
    //          //   _ik._logSpeedGroundInner = Motor.GroundingStatus.InnerGroundNormal;
    //          //   _ik._logSpeedGroundOutter = Motor.GroundingStatus.OuterGroundNormal;
    //          //   _ik._logSpeedMotorPosition = Motor.GroundingStatus.GroundPoint;
    //           //  _ik._logSpeedLastGroundPos = Motor.LastGroundingStatus.GroundPoint;
    //             _ik._logFixedConsumed = false;
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
    //             
    //             //MyPlayer.Inst.m_StartLeaveGround =true;
    //             Debug.LogWarning("设 m_StartLeaveGround 一次");
    //         //    Motor.ForceUnground(0.2f);
    //         }
    //         //接触地面时
    //         //else if (_ik._isLastLand == false && Motor.GroundingStatus.IsStableOnGround == true)
    //         else if (_ik.Motor.LastGroundingStatus.IsStableOnGround== false && _ik.Motor.GroundingStatus.IsStableOnGround)
    //         {
    //             _ik._logFixGroundedConsumed = false;
    //             //landing....
    //             //这里启动 awake()时也会触发一次
    // //            Debug.LogError("接触地面时 ,打印落地shi速度：" + Motor.Velocity);
    //         }

            _ik._isLastLand = Motor.GroundingStatus.IsStableOnGround;
        }
    }
}
