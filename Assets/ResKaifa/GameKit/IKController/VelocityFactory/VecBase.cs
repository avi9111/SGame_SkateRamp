using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace ResKaifa.GameKit
{
    public class VecBase
    {
        protected MyIK _ik;

        public virtual void Init(MyIK ik)
        {
            this._ik = ik;
        }

        public virtual void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public virtual void AfterCharacterUpdate(float deltaTime)
        {
            
        }

        public virtual void HandleUpdate(ref Vector3 currentVelocity, float deltaTime)
        {
        }

        public virtual void HandlePostGround()
        {
            var Motor = _ik.Motor;
            if (Motor.LastGroundingStatus.IsStableOnGround != _ik._isLastLand)
            {
                Debug.LogError("测试引擎的lastGround和自定义last 不同 MotorLast=" + Motor.LastGroundingStatus.IsStableOnGround+" Motor=" + Motor.GroundingStatus.IsStableOnGround);
            }

            //离地时
            //if (_ik._isLastLand == true && Motor.GroundingStatus.IsStableOnGround == false)
            if(_ik.Motor.LastGroundingStatus.IsStableOnGround && _ik.Motor.GroundingStatus.IsStableOnGround == false)
            {
            //    _ik._logSpeedGround = Motor.GroundingStatus;
             //   _ik._logSpeedGroundInner = Motor.GroundingStatus.InnerGroundNormal;
             //   _ik._logSpeedGroundOutter = Motor.GroundingStatus.OuterGroundNormal;
             //   _ik._logSpeedMotorPosition = Motor.GroundingStatus.GroundPoint;
              //  _ik._logSpeedLastGroundPos = Motor.LastGroundingStatus.GroundPoint;
                _ik._logFixedConsumed = false;
                
                // //_logFixedVelocity = Quaternion.Euler(0f,Vector3.Angle(Vector3.up, _logCurrentVelocity), 0) * Vector3.right;
                // var angle = Vector3.Angle(_logCurrentVelocity,Vector3.up); //这是一个负数？？？
                // _logFixedVelocity = Quaternion.AngleAxis(-angle, Vector3.left) * _logCurrentVelocity;
                // //和上面的方法是一样的。。。。。
                // // var angle = Vector3.Angle(Vector3.up, _logCurrentVelocity);
                // // _logFixedVelocity = Quaternion.AngleAxis(angle, Vector3.right) * _logCurrentVelocity;
                // _logFixedVelocity *= 1.3f;
                // //unground....
                // Debug.LogError("leave ground " + Vector3.Angle(Vector3.up,_logCurrentVelocity));
                
                //MyPlayer.Inst.m_StartLeaveGround =true;
                Debug.LogWarning("设 m_StartLeaveGround 一次");
            //    Motor.ForceUnground(0.2f);
            }
            //接触地面时
            //else if (_ik._isLastLand == false && Motor.GroundingStatus.IsStableOnGround == true)
            else if (_ik.Motor.LastGroundingStatus.IsStableOnGround== false && _ik.Motor.GroundingStatus.IsStableOnGround)
            {
                _ik._logFixGroundedConsumed = false;
                //landing....
                //这里启动 awake()时也会触发一次
    //            Debug.LogError("接触地面时 ,打印落地shi速度：" + Motor.Velocity);
            }

            _ik._isLastLand = Motor.GroundingStatus.IsStableOnGround;
        }
    }
}
