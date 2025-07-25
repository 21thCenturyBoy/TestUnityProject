using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_逃离")]
    public class Steering_Flee : Steering_Seek
    {
        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public override SteeringOutput Seek()
        {
            var res = new SteeringOutput();

            //获取目标的方向
            res.Linear = currentEntity.GetStaticStae().Position - GetTargetPos();

            //沿着此方向全速前进
            res.Linear = res.Linear.normalized;//归一化
            res.Linear *= maxAcceleration;

            res.Angular = 0;

            return res;
        }
    }

    [AILogicType("Steering_寻找")]
    public class Steering_Seek : SteeringLogic
    {
        public IKinematicEntity targetEntity;
        public IKinematicEntity currentEntity;

        [AIParam_Float("最大加速度")]
        public float maxAcceleration = 50f;

        [AIParam_Float("最大速度")]
        public float maxSpeed = 10f;

        /// <summary>
        /// 获取目标位置(非预测)
        /// </summary>
        /// <returns></returns>
        public virtual Vector3 GetTargetPos()
        {
            return targetEntity.GetStaticStae().Position;
        }

        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public virtual SteeringOutput Seek()
        {
            var res = new SteeringOutput();

            //获取目标的方向
            res.Linear = GetTargetPos() - currentEntity.GetStaticStae().Position;

            //沿着此方向全速前进
            res.Linear = res.Linear.normalized;//归一化
            res.Linear *= maxAcceleration;

            res.Angular = 0;

            return res;
        }

        protected override void OnFixedUpdate()
        {
            var res = Seek();

            currentEntity.FixedUpdate(res, maxSpeed, FixedDeltaTime);
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

        protected override void OnStop()
        {
            targetEntity.Destroy();
            currentEntity.Destroy();
        }
    }

}
