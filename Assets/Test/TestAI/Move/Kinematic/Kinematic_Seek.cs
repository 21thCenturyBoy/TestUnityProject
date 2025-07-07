using UnityEngine;

namespace TestAI.Move.Kinematic
{

    public class Kinematic_Seek : KinematicLogic
    {
        private IKinematicEntity targetEntity;
        private IKinematicEntity currentEntity;

        [AIParm_Float]
        public float maxSpeed = 0.5f;

        /// <summary>
        /// 获取到目标转向
        /// （逃离反转Velocity）
        /// </summary>
        /// <returns></returns>
        public SteeringOutputVelocity Seek()
        {

            var res = new SteeringOutputVelocity();

            //获取目标的方向
            res.Line = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;

            //沿着此方向全速前进
            res.Line = res.Line.normalized;//归一化
            res.Line *= maxSpeed;

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
            Seek();
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