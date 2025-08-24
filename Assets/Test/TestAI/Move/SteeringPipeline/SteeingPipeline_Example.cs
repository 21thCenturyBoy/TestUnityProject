using System;
using TestAI.Move.Kinematic;
using UnityEngine;

namespace TestAI.Move.SteeringPipeline
{
    [Serializable]
    public struct Pipeline_Goal : IPipeline_Goal
    {
        //通道数据
        public bool hasPositionData;
        public bool hasOrientationData;
        public bool hasVelocityData;
        public bool hasRotationData;

        public float orientation;
        public float rotation;
        public Vector3 position;
        public Vector3 velocity;

        public void UpdateChannels(Pipeline_Goal o)
        {
            if (o.hasOrientationData)
                orientation = o.orientation;
            if (o.hasRotationData)
                rotation = o.rotation;
            if (o.hasPositionData)
                position = o.position;
            if (o.hasVelocityData)
                velocity = o.velocity;
        }
    }
    public class Targeter : IPipeline_Targeter
    {
        public virtual IPipeline_Goal GetSumGoal(IKinematicEntity entity)
        {
            throw new NotImplementedException();
        }
    }
    public class Decomposer : IPipeline_Decomposer
    {
        public virtual IPipeline_Goal Decompose(IKinematicEntity entity, IPipeline_Goal sumGoal)
        {
            throw new NotImplementedException();
        }
    }

    public class Constraint : IPipeline_Constraint
    {
        public virtual bool IsViolated(IPath path)
        {
            throw new NotImplementedException();
        }
        public virtual IPipeline_Goal Suggest(IKinematicEntity entity, IPipeline_Goal goal, IPath path)
        {
            throw new NotImplementedException();
        }
    }
    public class Actuator : IPipeline_Actuator
    {
        public virtual IPath GetPath(IKinematicEntity entity, IPipeline_Goal goal)
        {
            throw new NotImplementedException();
        }
        public virtual SteeringOutput GetOutput(IKinematicEntity entity, IPipeline_Goal goal, IPath pathl)
        {
            throw new NotImplementedException();
        }
    }

    public class SteeingPipeline_Example : MonoBehaviour
    {
        public int constraintSteps = 3;
        Targeter[] targeters;
        Decomposer[] decomposers;
        Constraint[] constraints;
        Actuator actuator;

        protected IKinematicEntity entity;

        //死锁行为
        protected SteeringLogic deadlockBehavior;

        protected void Start()
        {
            Init();
        }

        protected virtual void Init()
        {
            // TODO : 这里可以根据需要初始化实体和死锁行为

        }

        private void FixedUpdate()
        {
            if (entity == null) return;
            Tick();
        }

        protected virtual void Tick()
        {
            entity.FixedUpdate(GetSteering(), float.MaxValue, KinematicLogic.FixedDeltaTime);
        }

        public virtual SteeringOutput GetSteering()
        {

            Pipeline_Goal goal = new Pipeline_Goal();
            TargeterHandle(goal);
            DecomposerHandle(goal);
            return ConstraintActuatorHandle(goal);
        }

        protected virtual void TargeterHandle(Pipeline_Goal goal)
        {
            // 合并所有目标生成器的目标（首要目标）
            foreach (Targeter targeter in targeters)
            {
                Pipeline_Goal targetGoal = (Pipeline_Goal)targeter.GetSumGoal(entity);
                goal.UpdateChannels(targetGoal);
            }
        }
        protected virtual void DecomposerHandle(Pipeline_Goal goal)
        {
            // 分解所有目标分解器的目标（次要目标）
            foreach (Decomposer targeter in decomposers)
            {
                Pipeline_Goal targetGoal = (Pipeline_Goal)targeter.Decompose(entity, goal);
                goal.UpdateChannels(targetGoal);
            }
        }
        protected virtual SteeringOutput ConstraintActuatorHandle(Pipeline_Goal goal)
        {
            Path_LineSegment path;

            // 循环遍历所有约束，直到没有违反的约束或达到最大迭代次数
            for (int i = 0; i < constraintSteps; i++)
            {
                //从执行器获取路径
                path = actuator.GetPath(entity, goal) as Path_LineSegment;

                //检查每个约束
                foreach (Constraint constraint in constraints)
                {
                    //如果路径违反约束，获取建议的目标并重新计算路径
                    if (constraint.IsViolated(path))
                    {
                        goal = (Pipeline_Goal)constraint.Suggest(entity, goal, path);
                        break;//重新计算路径
                    }
                    return actuator.GetOutput(entity, goal, path);
                }
            }

            return deadlockBehavior.GetSteeringOut();
        }
    }
}
