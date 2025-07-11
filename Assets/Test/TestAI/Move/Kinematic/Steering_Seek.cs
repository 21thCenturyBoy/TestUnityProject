using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TestAI.Move.Kinematic
{
    /// <summary>
    /// 逃离
    /// </summary>
    public class Steering_Flee: Steering_Seek
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
            res.Line =  currentEntity.GetStaticStae().Position - targetEntity.GetStaticStae().Position;

            //沿着此方向全速前进
            res.Line = res.Line.normalized;//归一化
            res.Line *= maxAcceleration;

            res.Angular = 0;

            return res;
        }
    }
    /// <summary>
    /// 寻找
    /// </summary>
    public class Steering_Seek : KinematicLogic
    {
        protected IKinematicEntity targetEntity;
        protected IKinematicEntity currentEntity;

        [AIParm_Float]
        public float maxAcceleration = 50f;
        [AIParm_Float]
        public float maxSpeed = 10f;

        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public virtual SteeringOutput Seek()
        {
            var res = new SteeringOutput();

            //获取目标的方向
            res.Line = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;

            //沿着此方向全速前进
            res.Line = res.Line.normalized;//归一化
            res.Line *= maxAcceleration;

            res.Angular = 0;

            return res;
        }

        protected override void OnFixedUpdate()
        {
            var res =  Seek();

            currentEntity.FixedUpdate(res,maxSpeed, FixedDeltaTime);
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
