using System;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Kinematic_漫游")]
    public class Kinematic_Wander : KinematicLogic
    {
        public IKinematicEntity currentEntity;
        [AIParam_Float("最大速度")]
        public float maxSpeed = 5f;
        [AIParam_Float("最大旋转速度")]
        public float maxRotate = MathF.PI;//最大旋转
        protected override void OnStart()
        {
            currentEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);
        }
        public KinematicOutput Wander()
        {
            var res = new KinematicOutput();

            var current_stae = currentEntity.GetStaticStae();

            //从方向的向量形式获取速度(朝向获取速度)
            res.Velocity = maxSpeed * current_stae.OrientationToVector();//获取当前方向的速度向量
            res.Rotation = UnityEngine.Random.Range(-1f, 1f) * maxRotate;//随机旋转
         
            return res;
        }
        protected override void OnFixedUpdate()
        {
            KinematicOutput res = Wander();
            currentEntity.FixedUpdate(res, FixedDeltaTime);
        }

        protected override void OnStop()
        {
            currentEntity.Destroy();
        }
    }
}
