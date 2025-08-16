using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_躲避")]
    public class Steering_Evade : Steering_Flee
    {
        [AIParam_Float("最大预测时间")]
        public float maxPredictionTime = 2f; //预测时间
        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public override SteeringOutput GetSteeringOut()
        {
            //获取目标的方向
            var direction =  currentEntity.GetStaticStae().Position - targetEntity.GetStaticStae().Position;
            var distance = direction.magnitude; //计算距离

            //当前角色速度标量
            var speed = currentEntity.Velocity.magnitude;

            var predictionTime = 0f; //计算预测时间
            //检查速度是否给予合理的预测时间
            if (speed <= distance / maxPredictionTime)
            {
                predictionTime = maxPredictionTime; //如果速度小于距离除以最大预测时间，则使用最大预测时间
            }
            else
            {
                predictionTime = distance / speed; //否则使用距离除以速度
            }
            var targetPos = targetEntity.GetStaticStae().Position;
            //预测目标位置
            m_PredictionPos = targetPos + targetEntity.Velocity * predictionTime;

            return base.GetSteeringOut();
        }

        Vector3 m_PredictionPos; //预测位置
        public override Vector3 GetTargetPos()
        {
            //获取目标位置并预测
            var targetState = targetEntity.GetStaticStae();
            return m_PredictionPos;
        }
    }

    [AILogicType("Steering_追逐")]
    public class Steering_Pursue : Steering_Seek
    {
        [AIParam_Float("最大预测时间")]
        public float maxPredictionTime = 2f; //预测时间
        /// <summary>
        /// 获取到目标转向
        /// （追击）
        /// </summary>
        /// <returns></returns>
        public override SteeringOutput GetSteeringOut()
        {
            //获取目标的方向
            var direction = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;
            var distance = direction.magnitude; //计算距离

            //当前角色速度标量
            var speed = currentEntity.Velocity.magnitude;

            var predictionTime =0f; //计算预测时间
            //检查速度是否给予合理的预测时间
            if (speed <= distance / maxPredictionTime)
            {
                predictionTime = maxPredictionTime; //如果速度小于距离除以最大预测时间，则使用最大预测时间
            }else
            {
                predictionTime = distance / speed; //否则使用距离除以速度
            }
            var targetPos = targetEntity.GetStaticStae().Position;
            //预测目标位置
            m_PredictionPos = targetPos + targetEntity.Velocity * predictionTime;

            return base.GetSteeringOut();
        }

        Vector3 m_PredictionPos; //预测位置
        public override Vector3 GetTargetPos()
        {
            //获取目标位置并预测
            var targetState = targetEntity.GetStaticStae();
            return m_PredictionPos;
        }
    }
}