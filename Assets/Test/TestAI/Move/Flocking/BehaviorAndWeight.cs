using TestAI.Move.Kinematic;

namespace TestAI.Move.Flocking
{
    public class BehaviorAndWeight
    {
        public SteeringLogic Behavior;
        public float Weight;

        public BehaviorAndWeight(SteeringLogic behavior, float weight)
        {
            Behavior = behavior;
            Weight = weight;
        }
    }
}