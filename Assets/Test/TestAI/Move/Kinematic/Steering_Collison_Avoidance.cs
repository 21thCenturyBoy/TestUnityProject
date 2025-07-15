using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace TestAI.Move.Kinematic
{
    [AILogicType("Steering_避免碰撞")]
    public class Steering_Collison_Avoidance : SteeringLogic
    {
        public IKinematicEntity currentEntity;

        //潜在的目标列表
        public IKinematicEntity[] targetEntitys;

        [AIParam_Float("最大加速度")]
        public float maxAcceleration = 10f; // 最大加速度

        [AIParam_Float("角色的碰撞半径")]
        public float radius = 0.5f; //碰撞半径

        public virtual SteeringOutput Separation()
        {
            var res = new SteeringOutput();

            //找到最接近的碰撞目标
            var shortestTime = 0f;

            //存储碰撞目标相关数据
            IKinematicEntity firstTarget = null;
            float firstMinSeparation = 0f;
            float firstDistance = 0f;
            Vector3 firstRelativePos = Vector3.zero;
            Vector3 firstRelativeVel = Vector3.zero;

            //这里简单粗暴直接遍历所有目标实体（TODO空间规划进行优化）
            //----------------------------------------------------------
            for (var i = 0; i < targetEntitys.Length; i++)
            {
                var targetEntity = targetEntitys[i];
                //计算碰撞时间
                var relativePos = targetEntity.Position - currentEntity.Position;//目标-->当前
                var relativeVel = targetEntity.Velocity - currentEntity.Velocity;
                var relativeSpeed = relativeVel.magnitude;//相对速度长度

                
                var timeToCollison = Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);

                //检查是否将要完全碰撞
                var distance = relativePos.magnitude;
                var minSeparation = distance - relativeSpeed * timeToCollison;

                if (minSeparation > 2 * radius) continue;


                //检查是否最短时间
                if (timeToCollison > 0 && timeToCollison < shortestTime)
                {

                    //存储时间、目标和其他数据
                    shortestTime = timeToCollison;

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

            //计算转向

            Vector3 relativePosCompute;
            //如果即将碰撞或者已经发生碰撞
            if (firstMinSeparation <= 0 || firstDistance < 2 * radius)
            {
                relativePosCompute = firstTarget.Position - currentEntity.Position;
            }
            else
            {
                relativePosCompute = firstRelativePos + firstRelativeVel * shortestTime;
            }

            //躲避目标
            res.Linear = relativePosCompute.normalized * maxAcceleration;
            res.Angular = 0;

            return res;
        }
        protected override void OnFixedUpdate()
        {
            var res = Separation();
            if (currentEntity.Velocity == Vector3.zero)
            {
                currentEntity.Velocity = Vector3.forward;
            }
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