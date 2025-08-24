using System;
using System.IO;
using UnityEngine;

namespace TestAI
{
    /// <summary>
    /// 静态状态
    /// </summary>
    public struct StaticStae
    {
        //位置
        public Vector3 Position;
        //方向（弧度）
        public float Orientation;
    }

    /// <summary>
    /// 转向输出加速度
    /// </summary>
    public struct SteeringOutput
    {
        //线加速度
        public Vector3 Linear;
        //角加速度
        public float Angular;

        //重写运算符
        public static SteeringOutput operator *(float weight, SteeringOutput output)
        {
            SteeringOutput result = new SteeringOutput();
            result.Linear = output.Linear * weight;
            result.Angular = output.Angular * weight;
            return result;
        }
        public static SteeringOutput operator *(SteeringOutput output, float weight)
        {
            return weight * output;
        }
        public static SteeringOutput operator +(SteeringOutput a, SteeringOutput b)
        {
            SteeringOutput result = new SteeringOutput();
            result.Linear = a.Linear + b.Linear;
            result.Angular = a.Angular + b.Angular;
            return result;
        }
        public static SteeringOutput operator -(SteeringOutput a, SteeringOutput b)
        {
            SteeringOutput result = new SteeringOutput();
            result.Linear = a.Linear - b.Linear;
            result.Angular = a.Angular - b.Angular;
            return result;
        }
    }

    /// <summary>
    /// 运动学输出速度
    /// </summary>
    public struct KinematicOutput
    {
        //线速度
        public Vector3 Velocity;
        //角速度
        public float Rotation;
    }

    public interface IKinematicLogic
    {
        void Start();
        void FixedUpdate();
        void Stop();
    }

    public interface IPath
    {
        public float GetParam(Vector3 position, float lastParam);

        public Vector3 GetPosition(float param);
    }
    public interface IPoint
    {
        void SetPosition(Vector3 pos);
        Vector3 GetPosition();
    }

    public interface IKinematicEntity
    {
        //位置
        public Vector3 Position { get; }
        //方向（弧度）
        public float Orientation { get; }
        //线速度
        public Vector3 Velocity { get; set; }
        //角速度（弧度每秒）
        public float Rotation { get; set; }
        StaticStae GetStaticStae();
        void SetStaticStae(StaticStae stae);
        void SetOrientation(float orientation);
        void SetPosition(Vector3 pos);
    }

    /// <summary> 目标接口 </summary>
    public interface IPipeline_Goal { }
    /// <summary> 目标生成器接口 </summary>
    public interface IPipeline_Targeter
    {
        public IPipeline_Goal GetSumGoal(IKinematicEntity entity);
    }
    /// <summary> 目标分解器接口 </summary>
    public interface IPipeline_Decomposer
    {
        public IPipeline_Goal Decompose(IKinematicEntity entity, IPipeline_Goal sumGoal);
    }
    /// <summary> 约束接口 </summary>
    public interface IPipeline_Constraint
    {
        //判断路径是否会违反约束
        public bool IsViolated(IPath path);

        //根据路径建议一个目标
        public IPipeline_Goal Suggest(IKinematicEntity entity, IPipeline_Goal goal,IPath path);
    }
    /// <summary> 执行器接口 </summary>
    public interface IPipeline_Actuator 
    {
        //最终路径
        public IPath GetPath(IKinematicEntity entity, IPipeline_Goal goal);

        //根据路径和目标计算输出加速度
        public SteeringOutput GetOutput(IKinematicEntity entity, IPipeline_Goal goal, IPath pathl);
    }

    public static class Utils_Move
    {
        /// <summary>
        /// 运动学应用
        /// </summary>
        /// <param name="stae"></param>
        /// <param name="steeringOutput"></param>
        public static void SteeringOutputApply(this ref StaticStae stae, SteeringOutput steeringOutput)
        {
            // 应用新的速度向量到位置
            stae.Position += steeringOutput.Linear;
            // 更新朝向
            stae.Orientation += steeringOutput.Angular;
        }


        /// <summary>
        /// 更新（标准运动学）手动更新方法(帧率低)，由于帧率低，可能会导致物体跳跃式移动。
        /// </summary>
        /// <param name="steering"></param>
        /// <param name="deltaTime"></param>
        public static void ForceUpdate(this IKinematicEntity entity, SteeringOutput steering, float deltaTime)
        {
            //加速度与位移公式：(vt²-v0²)=2as，s=v0t+at²/2，s2-s1=aT²。
            //更新位置
            float half_t = 0.5f * deltaTime * deltaTime;
            StaticStae staticStae = entity.GetStaticStae();
            staticStae.Position += entity.Velocity * deltaTime + steering.Linear * half_t;
            //更新方向
            staticStae.Orientation += entity.Rotation * deltaTime + steering.Angular * half_t;
            entity.SetStaticStae(staticStae);

            //更新线速度
            entity.Velocity += steering.Linear * deltaTime;
            //更新角速度
            entity.Rotation += steering.Angular * deltaTime;
        }

        /// <summary>
        /// 更新（运动学）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="kinematicOutput"></param>
        /// <param name="deltaTime"></param>
        public static void FixedUpdate(this IKinematicEntity entity, KinematicOutput kinematicOutput, float deltaTime)
        {
            //更新位置
            StaticStae staticStae = entity.GetStaticStae();
            staticStae.Position += entity.Velocity * deltaTime;
            //更新方向
            staticStae.Orientation += entity.Rotation * deltaTime;
            entity.SetStaticStae(staticStae);

            //更新线速度
            entity.Velocity = kinematicOutput.Velocity;
            //更新角速度
            entity.Rotation = kinematicOutput.Rotation;
        }

        /// <summary>
        /// 更新（转向行为）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="steering"></param>
        /// <param name="deltaTime"></param>
        public static void FixedUpdate(this IKinematicEntity entity, SteeringOutput steering, float deltaTime)
        {
            //更新位置
            StaticStae staticStae = entity.GetStaticStae();
            staticStae.Position += entity.Velocity * deltaTime;
            //更新方向
            staticStae.Orientation += entity.Rotation * deltaTime;
            entity.SetStaticStae(staticStae);

            //更新线速度
            entity.Velocity += steering.Linear * deltaTime;
            //更新角速度
            entity.Rotation += steering.Angular * deltaTime;
        }

        /// <summary>
        /// 更新（转向行为）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="steering"></param>
        /// <param name="deltaTime"></param>
        public static void FixedUpdate(this IKinematicEntity entity, SteeringOutput steering, float maxSpeed, float deltaTime)
        {
            //更新位置
            StaticStae staticStae = entity.GetStaticStae();
            staticStae.Position += entity.Velocity * deltaTime;
            //更新方向
            staticStae.Orientation += entity.Rotation * deltaTime;
            entity.SetStaticStae(staticStae);

            //更新线速度
            entity.Velocity += steering.Linear * deltaTime;
            //更新角速度
            entity.Rotation += steering.Angular * deltaTime;

            //检查速度是否超过最大速度
            if (entity.Velocity.magnitude > maxSpeed)
            {
                //如果超过最大速度，则归一化并乘以最大速度
                entity.Velocity = entity.Velocity.normalized * maxSpeed;
            }
        }
    }
}
