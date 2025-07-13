using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_速度匹配")]
    public class Steering_VelocityMatch : SteeringLogic
    {
        public IKinematicEntity targetEntity;
        public IKinematicEntity currentEntity;

        [AIParm_Float("最大加速度")]
        public float maxAcceleration = 50f;

        [AIParm_Float("到达目标速度的时间")]
        public float timeToTarget = 10f;

        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public virtual SteeringOutput VelocityMatch()
        {
            var res = new SteeringOutput();

            //加速尝试匹配目标速度
            res.Linear = targetEntity.Velocity - currentEntity.Velocity;
            res.Linear /= timeToTarget; //除以到达时间，得到加速度

            //检查加速度是否超过最大加速度
            var acceleration = res.Linear.magnitude;
            if (acceleration > maxAcceleration)
            {
                //如果加速度超过最大加速度，则归一化并乘以最大加速度
                res.Linear = res.Linear.normalized;
                res.Linear *= maxAcceleration;
            }

            res.Angular = 0;

            return res;
        }

        protected override void OnFixedUpdate()
        {
            var res = VelocityMatch();
            currentEntity.FixedUpdate(res, FixedDeltaTime);
        }

        protected override void OnStart()
        {
            targetEntity = UtilsTool.CreateNavigation_AI();
            Vector2 range = new Vector2(50, 50);
            targetEntity.SetStaticStae(UtilsTool.CreateRandomStaticStae(range));
            targetEntity.SetColor(Color.red);
            targetEntity.AllowDrag(true);

            currentEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);
        }

        [AITest_Button("设置目标的速度为前方")]
        public void ChangeTargetOrientation()
        {
            targetEntity.Velocity = targetEntity.GetStaticStae().OrientationToVector()*5f;
        }

        protected override void OnStop()
        {
            targetEntity.Destroy();
            currentEntity.Destroy();
        }
    }
}

