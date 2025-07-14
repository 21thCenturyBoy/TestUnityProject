using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_分离")]
    public class Steering_Separation : SteeringLogic
    {
        public IKinematicEntity currentEntity;

        //潜在的目标列表
        public IKinematicEntity[] targetEntitys;

        [AIParam_Float("分离最大加速度(可负)")]
        public float maxAcceleration = -10f; // 最大加速度

        [AIParam_Float("触发分离半径阈值")]
        public float threshold = 5f; // 分离半径

        [AIParam_Float("衰减系数")]
        public float decayCoefficient = 5f; // 分离半径

        public virtual SteeringOutput Separation()
        {
            var res = new SteeringOutput();

            //遍历目标（这里应该可以用空间划分技术优化如四叉树、八叉树等）
            for (int i = 0; i < targetEntitys.Length; i++)
            {
                var targetEntity = targetEntitys[i];
                if (targetEntity == null) continue;

                //检查目标是否靠近
                var direction = targetEntity.Position - currentEntity.Position;

                var distance = direction.magnitude;

                if (distance < threshold)
                {

                    //线性计算方式
                    //var strength = maxAcceleration * (threshold - distance) / threshold;

                    //平方反比计算
                    var strength = Mathf.Min(decayCoefficient / (distance * distance), maxAcceleration);

                    //添加分离力
                    res.Linear += direction.normalized * strength;
                }
            }
            return res;
        }
        protected override void OnFixedUpdate()
        {
            var res = Separation();
            currentEntity.FixedUpdate(res, FixedDeltaTime);
        }
        protected override void OnStart()
        {
            targetEntitys = new IKinematicEntity[5];

            //创建多个目标实体
            Vector2 range = new Vector2(50, 50);
            for (int i = 0; i < targetEntitys.Length; i++)
            {
                Navigation_AI_Item item = UtilsTool.CreateNavigation_AI() as Navigation_AI_Item;
                item.SetStaticStae(UtilsTool.CreateRandomStaticStae(range));
                item.SetColor(Color.red);
                item.AllowDrag(true);
                targetEntitys[i] = item;
            }

            currentEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);
        }

        protected override void OnStop()
        {
            if (targetEntitys != null)
            {
                foreach (var item in targetEntitys)
                {
                    if (item != null) item.Destroy();
                }
            }
            if (currentEntity != null) currentEntity.Destroy();
        }
    }
}