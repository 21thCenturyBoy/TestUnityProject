using System;
using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Kinematic_寻找")]
    public class Kinematic_Seek : KinematicLogic
    {
        public IKinematicEntity targetEntity;
        public IKinematicEntity currentEntity;

        [AIParam_Float("最大速度")]
        public float maxSpeed = 10f;

        /// <summary>
        /// 获取到目标转向
        /// </summary>
        /// <returns></returns>
        public KinematicOutput Seek()
        {

            var res = new KinematicOutput();

            //获取目标的方向速度
            res.Velocity = targetEntity.GetStaticStae().Position - currentEntity.GetStaticStae().Position;

            //沿着此方向全速前进
            res.Velocity = res.Velocity.normalized;//归一化
            res.Velocity *= maxSpeed;

            //面向要移动的方向
            float targetOrientation = UtilsTool.NewOrientation(currentEntity.GetStaticStae().Orientation, res.Velocity);
            currentEntity.SetOrientation(targetOrientation);

            res.Rotation = 0;

            return res;
        }

        protected override void OnFixedUpdate()
        {
            KinematicOutput res = Seek();
  
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