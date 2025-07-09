using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TestAI.Move.Kinematic
{
    public class Steering_Seek : KinematicLogic
    {
        private IKinematicEntity targetEntity;
        private IKinematicEntity currentEntity;

        [AIParm_Float]
        public float maxSpeed = 0.5f;

        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public SteeringOutput Seek()
        {
            var res = new SteeringOutput();

            //获取目标的方向
            res.Line = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;

            //沿着此方向全速前进
            res.Line = res.Line.normalized;//归一化
            res.Line *= maxSpeed;

            res.Angular = 0;

            return res;
        }

        protected override void OnFixedUpdate()
        {
            var res =  Seek();


            var current_stae = currentEntity.GetStaticStae();

            current_stae.SteeringOutputApply(res);
            currentEntity.SetDynamicStae(res);


        }

        public void FixedUpdate(SteeringOutput steering, float maxSpeed, float deltaTime)
        {

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
