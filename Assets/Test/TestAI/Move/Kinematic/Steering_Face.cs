using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_朝向")]
    public class Steering_Face : Steering_Align
    {
        public override SteeringOutput GetSteeringOut()
        {
            //计算出目标方向
            var direction = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;

            //太近了就不需要转向
            m_targetOrientation = UtilsTool.ComputeOrientation(direction);

            return base.GetSteeringOut();
        }

        private float m_targetOrientation;

        public override float GetTargetOrientation()
        {
            return m_targetOrientation;
        }

    }

    [AILogicType("Steering_直视移动方向")]
    public class Steering_FaceTargetForward : Steering_Align
    {
        public override SteeringOutput GetSteeringOut()
        {
            //计算出目标方向
            var direction = targetEntity.Velocity;

            //太近了就不需要转向
            m_targetOrientation = UtilsTool.ComputeOrientation(direction);

            return base.GetSteeringOut();
        }

        private float m_targetOrientation;

        public override float GetTargetOrientation()
        {
            return m_targetOrientation;
        }
        protected override void OnStart()
        {
            base.OnStart();

            targetEntity.AutoMove(true);//给定一个速度
        }
    }
}

