using System;
using System.Collections.Generic;
using TestAI.Move.Kinematic;

namespace TestAI.Move.Flocking
{
    public class BlendedSteering : SteeringLogic
    {
        //总体最大加速度和旋转
        [AIParam_Float("最大线性加速度")]
        public float maxAcceleration = 50f;
        [AIParam_Float("最大角加速度")]
        public float maxRotation = 10f;

        public List<BehaviorAndWeight> behaviors;
        public override SteeringOutput GetSteeringOut()
        {
            var res = new SteeringOutput();

            //累加所有行为的结果
            foreach (var behaviorAndWeight in behaviors)
            {
                res += behaviorAndWeight.Behavior.GetSteeringOut() * behaviorAndWeight.Weight;
            }

            //裁剪结果
            float accelerationLinear = res.Linear.magnitude;
            float accelerationAngularn = res.Angular;
            if (accelerationLinear >= maxAcceleration) res.Linear = res.Linear.normalized * maxAcceleration;
            if (MathF.Abs(accelerationAngularn) >= maxRotation) res.Angular = maxRotation;

            return res;
        }
    }

}

