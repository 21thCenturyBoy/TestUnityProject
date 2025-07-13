using UnityEngine;

namespace TestAI.Move.Kinematic
{
    public class Kinematic_Arrive : KinematicLogic
    {
        public IKinematicEntity targetEntity;
        public IKinematicEntity currentEntity;

        [AIParam_Float("最大速度")]
        public float maxSpeed = 10f;
        [AIParam_Float("到达目标的时间")]
        public float arrive_time = 0.5f;//到达目标的时间
        [AIParam_Float("目标半径范围")]
        public float targetRadius = 2.5f;//目标半径范围
        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public KinematicOutput Arrive()
        {
            var res = new KinematicOutput();
            //获取目标的方向
            res.Velocity = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;

            ////检查是否在目标半径范围内
            if (res.Velocity.magnitude < targetRadius)
            {
                res.Velocity = Vector3.zero;
                //不请求转向
                return res;
            }

            //需要移动到目标位置
            res.Velocity /= arrive_time;

            //如果该速度太快，则限制速度
            if (res.Velocity.magnitude > maxSpeed)
            {
                res.Velocity = res.Velocity.normalized * maxSpeed;
            }

            //改变朝向
            //面向要移动的方向
            float targetOrientation = UtilsTool.NewOrientation(currentEntity.GetStaticStae().Orientation, res.Velocity);
            currentEntity.SetOrientation(targetOrientation);

            res.Rotation = 0;

            return res;
        }
        protected override void OnFixedUpdate()
        {
            KinematicOutput res = Arrive();
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
        protected override void OnStop()
        {
            targetEntity.Destroy();
            currentEntity.Destroy();
        }
    }
}
