using TestAI.Move.Kinematic;

namespace TestAI.Move.Flocking
{
    public class BehaviorAndWeight
    {
        public SteeringLogic Behavior;
        public float Weight;
    }

    public class BlendedSteering : SteeringLogic
    {
        //总体最大加速度和旋转
        public float maxAcceleration = 50f;
        public float maxRotation = 10f;

        private BehaviorAndWeight[] m_Behaviors = new BehaviorAndWeight[0];
        public override SteeringOutput GetSteeringOut()
        {
            var res = new SteeringOutput();

            //累加所有行为的结果
            foreach (var behaviorAndWeight in m_Behaviors)
            {
                res += behaviorAndWeight.Behavior.GetSteeringOut() * behaviorAndWeight.Weight;
            }
            return res;
        }
    }
}

