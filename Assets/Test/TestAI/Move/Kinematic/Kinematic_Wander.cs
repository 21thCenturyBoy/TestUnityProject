namespace TestAI.Move.Kinematic
{
    public class Kinematic_Wander : KinematicLogic
    {
        private IKinematicEntity currentEntity;
        [AIParm_Float]
        public float maxSpeed = 0.2f;
        [AIParm_Float]
        public float maxRotate = 0.1f;//最大旋转
        protected override void OnStart()
        {
            currentEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);
        }
        protected override void OnFixedUpdate()
        {
            Wander();
        }
        public SteeringOutputVelocity Wander()
        {
            var res = new SteeringOutputVelocity();

            var current_stae = currentEntity.GetStaticStae();

            //从方向的向量形式获取速度
            res.Line = maxSpeed * current_stae.OrientationToVector();//获取当前方向的速度向量
            res.Angular = UnityEngine.Random.Range(-1f, 1f) * maxRotate;//随机旋转

            //更新实体状态
            current_stae.SteeringOutputApply(res);

            currentEntity.SetStaticStae(current_stae);

            return res;
        }
        protected override void OnStop()
        {
            currentEntity.Destroy();
        }
    }
}
