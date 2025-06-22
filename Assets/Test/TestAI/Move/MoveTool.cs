using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace TestAI.Move
{

    public class Kinematic
    {
        //位置
        public Vector3 Position { get; set; }
        //方向（弧度）
        public float Orientation { get; set; }
        //线速度
        public Vector3 Velocity { get; set; }
        //角速度（弧度每秒）
        public float Rotation { get; set; }

        /// <summary>
        /// 手动更新方法(帧率低)，由于帧率低，可能会导致物体跳跃式移动。
        /// </summary>
        /// <param name="steering"></param>
        /// <param name="deltaTime"></param>
        public void ForceUpdate(SteeringOutput steering, float deltaTime)
        {
            //加速度与位移公式：(vt²-v0²)=2as，s=v0t+at²/2，s2-s1=aT²。
            //更新位置
            float half_t = 0.5f * deltaTime * deltaTime;
            Position += Velocity * deltaTime + steering.Velocity * half_t;
            //更新方向
            Orientation += Rotation * deltaTime + steering.Angular * half_t;

            //更新线速度
            Velocity += steering.Velocity * deltaTime;
            //更新角速度
            Rotation += steering.Angular * deltaTime;
        }


        public void FixedUpdate(SteeringOutput steering, float deltaTime)
        {
            //更新位置
            Position += Velocity * deltaTime;
            //更新方向
            Orientation += Rotation * deltaTime;

            //更新线速度
            Velocity += steering.Velocity * deltaTime;
            //更新角速度
            Rotation += steering.Angular * deltaTime;
        }
    }


    public static class MoveTool
    {

    }
}

