using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestAI.Move.Kinematic
{    
    [AILogicType("Steering_到达")]
    public class Steering_Arrive : SteeringLogic
    {
        public IKinematicEntity targetEntity;
        public IKinematicEntity currentEntity;

        [AIParam_Float("最大加速度")]
        public float maxAcceleration = 50f;

        [AIParam_Float("最大速度")]
        public float maxSpeed = 10f;

        [AIParam_Float("到达目标的时间")]
        public float arrive_time = 0.5f;//到达目标的时间

        [AIParam_Float("目标半径范围")]
        public float targetRadius = 2.5f;//目标半径范围

        [AIParam_Float("减速半径")]
        public float slowRadius = 5f; //减速半径

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
        public override SteeringOutput GetSteeringOut()
        {
            var res = new SteeringOutput();

            var direction = GetTargetPos() - currentEntity.GetStaticStae().Position;
            var distance = direction.magnitude; //归一化

            ////检查是否在目标半径范围内
            if (distance <= targetRadius) return res; //如果在目标半径范围内，则不需要转向

            float targetSpeed = 0;
            if (distance > slowRadius)
            {
                //如果距离大于减速半径，则全速前进
                targetSpeed = maxSpeed; //目标速度
            }
            else
            {
                //在减速半径内，速度按比例缩小
                targetSpeed = maxSpeed * distance / slowRadius;
            }

            //目标速度将组合速率和方向
            var targetVelocity = direction.normalized * targetSpeed;//归一化方向并乘以目标速度

            //计算线性加速度
            res.Linear = targetVelocity - currentEntity.Velocity; //当前速度与目标速度的差值
            res.Linear /= arrive_time; //除以到达时间，得到加速度

            //检查加速度是否超过最大加速度
            if (res.Linear.magnitude > maxAcceleration)
            {
                //如果加速度超过最大加速度，则归一化并乘以最大加速度
                res.Linear = res.Linear.normalized * maxAcceleration;
            }

            res.Angular = 0;

            return res;
        }
        protected override void OnFixedUpdate()
        {
            SteeringOutput res = GetSteeringOut();
            if (res.Linear == Vector3.zero) return; //如果没有转向，则不更新

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

