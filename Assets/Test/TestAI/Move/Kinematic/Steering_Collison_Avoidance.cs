using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_避免碰撞")]
    public class Steering_Collison_Avoidance : SteeringLogic
    {
        public IKinematicEntity currentEntity;

        //潜在的目标列表
        public IKinematicEntity[] targetEntitys;

        [AIParam_Float("最大加速度")]
        public float maxAcceleration = -10f; // 最大加速度

        [AIParam_Float("角色的碰撞半径")]
        public float radius = 1f; //碰撞半径


        public virtual SteeringOutput Separation()
        {
            var res = new SteeringOutput();

            //找到最接近的碰撞目标
            var shortestTime = float.PositiveInfinity;

            //存储碰撞目标相关数据
            IKinematicEntity firstTarget = null;
            float firstMinSeparation = float.PositiveInfinity;
            float firstDistance = float.PositiveInfinity;
            Vector3 firstRelativePos = Vector3.positiveInfinity;
            Vector3 firstRelativeVel = Vector3.zero;

            Vector3 relativePos = Vector3.positiveInfinity;
            //这里简单粗暴直接遍历所有目标实体（TODO空间规划进行优化）
            //----------------------------------------------------------
            for (var i = 0; i < targetEntitys.Length; i++)
            {
                var targetEntity = targetEntitys[i];
                relativePos = targetEntity.Position - currentEntity.Position;
                Vector3 relativeVel = currentEntity.Velocity - targetEntity.Velocity;//注意这里有问题
                float relativeSpeed = relativeVel.magnitude;
                float timeToCollision = (Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed));

                float distance = relativePos.magnitude;
                float minSeparation = distance - relativeSpeed * timeToCollision;
                if (minSeparation > 2 * radius)
                {
                    continue;
                }
                if (timeToCollision > 0 && timeToCollision < shortestTime)
                {
                    shortestTime = timeToCollision;
                    firstTarget = targetEntity;
                    firstMinSeparation = minSeparation;
                    firstDistance = distance;
                    firstRelativePos = relativePos;
                    firstRelativeVel = relativeVel;
                }

            }
            //----------------------------------------------------------
            //没有可能碰撞目标
            if (firstTarget == null) return res;

            if (firstMinSeparation <= 0 || firstDistance < 2 * radius)
            {
                relativePos = firstTarget.Position- currentEntity.Position;
            }
            else
            {
                relativePos = firstRelativePos + firstRelativeVel * shortestTime;
            }

            SteeringOutput result = new SteeringOutput();
            relativePos.Normalize();
            res.Linear = relativePos * maxAcceleration;
            res.Angular = 0;

            return res;
        }
        protected override void OnFixedUpdate()
        {
            var res = Separation();
            currentEntity.FixedUpdate(res, FixedDeltaTime);
        }


        protected override void OnStart()
        {
            targetEntitys = new IKinematicEntity[20];

            //绕圆一周计算一个间隔10弧度创建一个物体
            float angleStep = 2 * Mathf.PI / targetEntitys.Length;

            float startAngle = 0;
            float areaRadius = 40;
            for (int i = 0; i < targetEntitys.Length; i++)
            {
                Navigation_AI_Item item = UtilsTool.CreateNavigation_AI() as Navigation_AI_Item;
                float x = areaRadius * Mathf.Cos(startAngle + i * angleStep);
                float z = areaRadius * Mathf.Sin(startAngle + i * angleStep);
                item.SetPosition(new Vector3(x, 0, z));
                //计算朝向，看向(0，0)
                item.SetOrientation(UtilsTool.ComputeOrientation(-new Vector3(x, 0, z)));
                item.SetColor(Color.red);
                item.AllowDrag(true);
                item.AutoMove(true);
                item.gameObject.name = "Target_" + i;
                targetEntitys[i] = item;
            }

            Navigation_AI_Item current = UtilsTool.CreateNavigation_AI() as Navigation_AI_Item;
            currentEntity = current;
            StaticStae stae = new StaticStae();
            currentEntity.SetStaticStae(stae);
            currentEntity.SetColor(Color.green);

            current.StartCoroutine(WaitForInitialization(current));
        }

        private IEnumerator WaitForInitialization(Navigation_AI_Item current)
        {
            yield return new WaitForSeconds(1f);
            current.Velocity = Vector3.forward * 5f; // 设置初始速度
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