using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_对齐")]
    public class Steering_Align : SteeringLogic
    {
        public IKinematicEntity targetEntity;
        public IKinematicEntity currentEntity;

        [AIParam_Float("最大旋转加速度（弧度）")]
        public float maxAngularAcceleration = 6f;

        [AIParam_Float("最大旋转速度（弧度）")]
        public float maxRotate = 1f;

        [AIParam_Float("到达目标的时间")]
        public float arrive_time = 0.2f;//到达目标的时间
        [AIParam_Float("朝向减缓区间（弧度）")]
        public float slowRadius = 0.4f; //减速半径
        [AIParam_Float("朝向最小近似值（弧度）")]
        public float targetRadius = 0.05f;
        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public override SteeringOutput GetSteeringOut()
        {
            var res = new SteeringOutput();

            // 获取方向差（弧度）
            var rotation = GetTargetOrientation() - currentEntity.GetStaticStae().Orientation;

            //限制在-180到180度之间
            if (rotation >= Mathf.PI) rotation -= 2 * Mathf.PI; //如果大于180度，则减去360度
            if (rotation < -Mathf.PI) rotation += 2 * Mathf.PI; //如果小于-180度，则加上360度

            //旋转弧度的绝对值
            var roatationAbs = Mathf.Abs(rotation);

            //如果朝向已经近似，则不需要转向
            if (roatationAbs < targetRadius) return res;

            //开始计算旋转加速度
            var targetRotae = 0f; //目标旋转速度
            if (roatationAbs < slowRadius)
            {
                //如果在减速半径内，速度按比例缩小
                targetRotae = maxRotate * roatationAbs / slowRadius; //按比例缩小
            }
            else
            {
                //如果距离大于减速半径，则全速前进
                targetRotae = maxRotate; //目标速度
            }

            //最终的目标旋转速度将组合速率和方向
            targetRotae = targetRotae * rotation / roatationAbs; //归一化方向并乘以目标速度

            //加速尝试到达目标旋转速度
            res.Angular = targetRotae - currentEntity.Rotation; //当前速度与目标速度的差值
            res.Angular /= arrive_time; //除以到达时间，得到加速度

            //检查加速度是否超过最大加速度
            var angularAcceleration = Mathf.Abs(res.Angular);
            if (angularAcceleration > maxAngularAcceleration)
            {
                //如果加速度超过最大加速度，则归一化并乘以最大加速度
                res.Angular /= angularAcceleration;
                res.Angular *= maxAngularAcceleration;
            }

            res.Linear = Vector3.zero; //线性加速度为0，因为这是对齐行为

            return res;
        }

        /// <summary>
        /// 获取目标位置(非预测)
        /// </summary>
        /// <returns></returns>
        public virtual float GetTargetOrientation()
        {
            return targetEntity.GetStaticStae().Orientation;
        }

        protected override void OnFixedUpdate()
        {
            SteeringOutput res = GetSteeringOut();

            currentEntity.FixedUpdate(res, FixedDeltaTime);
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

        [AITest_Button("随机目标")]
        public void ChangeTargetOrientation()
        {
            Vector2 range = new Vector2(50, 50);
            targetEntity.SetStaticStae(UtilsTool.CreateRandomStaticStae(range));
        }

        protected override void OnStop()
        {
            targetEntity.Destroy();
            currentEntity.Destroy();
        }
    }
}

