using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_漫游")]
    public class Steering_Wander : Steering_Seek
    {
        [AIParm_Float("漫游圆圈半径")]
        public float wanderRadius =2.0f; // 漫游半径
        [AIParm_Float("前向偏移")]
        public float wanderOffset = 5.0f; // 偏移

        [AIParm_Float("漫游方向变化最大比率(弧度)")]
        public float wanderRate = 3f; // 漫游角度变化

        [AIParm_Info("漫游目标当前方向")]
        public float wanderOrientation = 0.0f; // 当前漫游角度

        //-------------------------------------------------------
        Steering_Face m_steering_Face = new Steering_Face();

        [AIParm_Float("最大旋转加速度（弧度）")]
        public float maxAngularAcceleration { get => m_steering_Face.maxAngularAcceleration; set { m_steering_Face.maxAngularAcceleration = value; } }

        [AIParm_Float("最大旋转速度（弧度）")]
        public float maxRotate { get => m_steering_Face.maxRotate; set { m_steering_Face.maxRotate = value; } }

        [AIParm_Float("到达目标的时间")]
        public float arrive_time { get => m_steering_Face.arrive_time; set { m_steering_Face.arrive_time = value; } }
        [AIParm_Float("朝向减缓区间（弧度）")]
        public float slowRadius { get => m_steering_Face.slowRadius; set { m_steering_Face.slowRadius = value; } }
        [AIParm_Float("朝向最小近似值（弧度）")]
        public float targetRadius { get => m_steering_Face.targetRadius; set { m_steering_Face.targetRadius = value; } }
        //-------------------------------------------------------
        public override SteeringOutput Seek()
        {
            SteeringOutput res = base.Seek(); // 调用基类的Seek方法

            res.Angular = m_steering_Face.Align().Angular; // 对齐目标方向

            return res;
        }

        /// <summary>
        /// 获取目标位置(非预测)
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetTargetPos()
        {
            var currentState =  currentEntity.GetStaticStae();
            var disVector = currentState.Position - targetEntity.GetStaticStae().Position;
            if (disVector.sqrMagnitude <2.5f)
            {
                //如果距离小于1，重新设置目标位置
                var targetNewPos =  ComputeTarget();
                targetEntity.SetPosition(targetNewPos);
            }
            return targetEntity.GetStaticStae().Position;
        }

        protected Vector3 ComputeTarget() {

            var currentState = currentEntity.GetStaticStae();

            wanderOrientation += UtilsTool.GetRandomOne() * wanderRate; // 随机漫游角度变化
            var targetOrientation = currentState.Orientation + wanderOrientation;

            //计算漫游圈中心
            Vector3 target = currentState.Position + wanderOffset * currentState.OrientationToVector();// 前向偏移

            //计算漫游目标位置
            target += wanderRadius * UtilsTool.OrientationToVector(targetOrientation); // 漫游半径

            return target;
        }


        protected override void OnStart()
        {
            currentEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);

            targetEntity = UtilsTool.CreateNavigation_Point();
            targetEntity.SetPosition(ComputeTarget());
            targetEntity.SetColor(Color.red);

            m_steering_Face.targetEntity = targetEntity;
            m_steering_Face.currentEntity = currentEntity;
        }

        protected override void OnStop()
        {
            targetEntity.Destroy();
            currentEntity.Destroy();
        }

    }
}

