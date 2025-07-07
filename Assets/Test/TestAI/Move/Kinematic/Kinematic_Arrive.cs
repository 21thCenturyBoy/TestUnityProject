using UnityEngine;

namespace TestAI.Move.Kinematic
{
    public class Kinematic_Arrive : KinematicLogic
    {
        private IKinematicEntity targetEntity;
        private IKinematicEntity currentEntity;

        [AIParm_Float]
        public float maxSpeed = 0.5f;
        [AIParm_Float]
        public float slowRadius = 5;//减速范围
        [AIParm_Float]
        public float targetRadius = 1;//目标半径范围
        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public SteeringOutputVelocity Arrive()
        {
            var res = new SteeringOutputVelocity();
            //获取目标的方向
            res.Line = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;
            //计算距离
            float distance = res.Line.magnitude;
            if (distance < targetRadius)
            {
                res.Line = Vector3.zero;
                return res;
            }
            //这里可以使用线性插值来计算速度，也可以根据时间来计算速度
            //如果在减速范围内，计算速度
            if (distance < slowRadius)
            {
                res.Line = res.Line.normalized * maxSpeed * (distance / slowRadius);
            }
            else
            {
                res.Line = res.Line.normalized * maxSpeed;
            }
            //面向要移动的方向
            var current_stae = currentEntity.GetStaticStae();
            float currentOrientation = current_stae.Orientation;
            res.Angular = 0;
            float targetOrientation = UtilsTool.NewOrientation(currentOrientation, res.Line);
            current_stae.Orientation = targetOrientation;

            current_stae.SteeringOutputApply(res);

            currentEntity.SetStaticStae(current_stae);
            return res;
        }
        protected override void OnFixedUpdate()
        {
            Arrive();
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
        protected override void OnStop()
        {
            targetEntity.Destroy();
            currentEntity.Destroy();
        }
    }
}
