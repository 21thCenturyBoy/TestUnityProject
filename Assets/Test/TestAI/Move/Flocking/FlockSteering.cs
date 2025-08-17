using System.Collections.Generic;
using TestAI.Move.Kinematic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace TestAI.Move.Flocking
{
    public class FlockSteering : BlendedSteering
    {
        public IKinematicEntity CenterEntity;
        public IKinematicEntity TargetEntity;

        public IKinematicEntity Entity;
        public FlockSteering[] SimilarFlockerEntitys;



        protected override void OnStart()
        {
            //分离
            Steering_Separation separate = new Steering_Separation();
            separate.currentEntity = Entity;
            separate.targetEntitys = new IKinematicEntity[SimilarFlockerEntitys.Length];

            //将相似实体添加到分离目标列表中
            for (int i = 0; i < SimilarFlockerEntitys.Length; i++)
            {
                separate.targetEntitys[i] = SimilarFlockerEntitys[i].Entity;
            }

            //对齐目标面向重心平均移动方向&寻找目标
            Steering_FaceTargetForward steering_FaceTargetForward = new Steering_FaceTargetForward();
            steering_FaceTargetForward.currentEntity = Entity;
            steering_FaceTargetForward.targetEntity = CenterEntity;

            Steering_Seek seek = new Steering_Seek();
            seek.targetEntity = TargetEntity;
            seek.currentEntity = Entity;

            //聚集重心
            Steering_Arrive cohere = new Steering_Arrive();
            cohere.currentEntity = Entity;
            cohere.targetEntity = CenterEntity;

            Behaviors = new List<BehaviorAndWeight>
            {
                new BehaviorAndWeight(separate, 0),
                new BehaviorAndWeight(steering_FaceTargetForward, 0),
                new BehaviorAndWeight(seek, 0),
                new BehaviorAndWeight(cohere, 0),
            };
        }
    }

    [AILogicType("蜂拥集群管理")]
    public class FlockLogicManager : KinematicLogic
    {
        public IKinematicEntity targetEntity;
        public IKinematicEntity centerEntity;

        public List<FlockSteering> birdEntitys;

        //最大速度
        [AIParam_Float("总最大速度")]
        public float maxSpeed = 5f;

        [AIParam_Float("----分离权重----")]
        public float Separation_Weight = 1f;
        [AIParam_Float("分离最大加速度(可负)")]
        public float Separation_maxAcceleration = -10f; // 最大加速度
        [AIParam_Float("触发分离半径阈值")]
        public float Separation_threshold = 5f; // 分离半径
        [AIParam_Float("衰减系数")]
        public float Separation_decayCoefficient = 5f; // 分离半径

        [AIParam_Float("----对齐面向权重----")]
        public float FaceTargetForward_Weight = 1f;
        [AIParam_Float("最大旋转加速度（弧度）")]
        public float FaceTargetForward_maxAngularAcceleration = 6f;
        [AIParam_Float("最大旋转速度（弧度）")]
        public float FaceTargetForward_maxRotate = 1f;
        [AIParam_Float("到达目标的时间")]
        public float FaceTargetForward_arrive_time = 0.2f;//到达目标的时间
        [AIParam_Float("朝向减缓区间（弧度）")]
        public float FaceTargetForward_slowRadius = 0.4f; //减速半径
        [AIParam_Float("朝向最小近似值（弧度）")]
        public float FaceTargetForward_targetRadius = 0.05f;

        [AIParam_Float("----寻找目标权重----")]
        public float Seek_Weight = 1f;
        [AIParam_Float("最大加速度")]
        public float Seek_maxAcceleration = 50f;
        [AIParam_Float("最大速度")]
        public float Seek_maxSpeed = 10f;

        [AIParam_Float("----聚集权重----")]
        public float Arrive_Weight = 0.7f;
        [AIParam_Float("最大加速度")]
        public float Arrive_maxAcceleration = 50f;
        [AIParam_Float("最大速度")]
        public float Arrive_maxSpeed = 10f;
        [AIParam_Float("到达目标的时间")]
        public float Arrive_arrive_time = 0.5f;//到达目标的时间
        [AIParam_Float("目标半径范围")]
        public float Arrive_targetRadius = 2.5f;//目标半径范围
        [AIParam_Float("减速半径")]
        public float Arrive_slowRadius = 5f; //减速半径

        protected override void OnFixedUpdate()
        {
            //计算中心实体位置
            Vector3 centerPosition = Vector3.zero;
            Vector3 velocity = Vector3.zero;
            for (int i = 0; i < birdEntitys.Count; i++)
            {
                birdEntitys[i].Behaviors[0].Weight = Separation_Weight;
                Steering_Separation separateBehavior = birdEntitys[i].Behaviors[0].Behavior as Steering_Separation;
                separateBehavior.maxAcceleration = Separation_maxAcceleration;
                separateBehavior.threshold = Separation_threshold;
                separateBehavior.decayCoefficient = Separation_decayCoefficient;

                birdEntitys[i].Behaviors[1].Weight = FaceTargetForward_Weight;
                Steering_FaceTargetForward faceTargetForwardBehavior = birdEntitys[i].Behaviors[1].Behavior as Steering_FaceTargetForward;
                faceTargetForwardBehavior.maxAngularAcceleration = FaceTargetForward_maxAngularAcceleration;
                faceTargetForwardBehavior.maxRotate = FaceTargetForward_maxRotate;
                faceTargetForwardBehavior.arrive_time = FaceTargetForward_arrive_time;
                faceTargetForwardBehavior.slowRadius = FaceTargetForward_slowRadius; //朝向减缓区间
                faceTargetForwardBehavior.targetRadius = FaceTargetForward_targetRadius; //朝向最小近似值

                birdEntitys[i].Behaviors[2].Weight = Seek_Weight;
                Steering_Seek seekBehavior = birdEntitys[i].Behaviors[2].Behavior as Steering_Seek;
                seekBehavior.maxAcceleration = Seek_maxAcceleration;
                seekBehavior.maxSpeed = Seek_maxSpeed;

                birdEntitys[i].Behaviors[3].Weight = Arrive_Weight;
                Steering_Arrive arriveBehavior = birdEntitys[i].Behaviors[3].Behavior as Steering_Arrive;
                arriveBehavior.maxAcceleration = Arrive_maxAcceleration;
                arriveBehavior.maxSpeed = Arrive_maxSpeed;
                arriveBehavior.arrive_time = Arrive_arrive_time; //到达目标的时间
                arriveBehavior.targetRadius = Arrive_targetRadius; //目标半径范围
                arriveBehavior.slowRadius = Arrive_slowRadius; //减速半径

                centerPosition += birdEntitys[i].Entity.GetStaticStae().Position;
                velocity += birdEntitys[i].Entity.Velocity;
            }

            velocity /= birdEntitys.Count;
            centerPosition /= birdEntitys.Count;
            centerEntity.SetPosition(centerPosition);
            float centerDirection =  UtilsTool.ComputeOrientation(velocity); //计算平均方向
            centerEntity.SetOrientation(centerDirection);
            centerEntity.Velocity = velocity;

            // 更新群实体
            for (int i = 0; i < birdEntitys.Count; i++)
            {
                SteeringOutput res = birdEntitys[i].GetSteeringOut();
                if (res.Linear == Vector3.zero) continue; //如果没有转向，则不更新

                birdEntitys[i].Entity.FixedUpdate(res, maxSpeed, FixedDeltaTime);
            }
        }
        protected override void OnStart()
        {
            birdEntitys = new List<FlockSteering>();
            int birdCount = 20; //鸟类实体数量
            //创建多个目标实体
            Vector2 range = new Vector2(50, 50);
            for (int i = 0; i < birdCount; i++)
            {
                Navigation_AI_Item item = UtilsTool.CreateNavigation_AI() as Navigation_AI_Item;
                item.SetStaticStae(UtilsTool.CreateRandomStaticStae(range));
                item.SetColor(Color.red);
                item.AllowDrag(true);
                item.gameObject.name = "FlockEntity_" + i;
                birdEntitys.Add(new FlockSteering());
                birdEntitys[i].Entity = item;
            }

            targetEntity = UtilsTool.CreateNavigation_AI();
            StaticStae stae = new StaticStae();
            targetEntity.SetStaticStae(stae);
            targetEntity.SetColor(Color.green);
            targetEntity.AllowDrag(true);

            centerEntity = UtilsTool.CreateNavigation_Center();
            //计算中心实体位置
            Vector3 centerPosition = Vector3.zero;
            for (int i = 0; i < birdEntitys.Count; i++)
            {
                centerPosition += birdEntitys[i].Entity.GetStaticStae().Position;
            }
            stae.Position = centerPosition / birdEntitys.Count;
            centerEntity.SetStaticStae(stae);
            centerEntity.SetColor(Color.green);

            //群实体的初始化
            for (int i = 0; i < birdEntitys.Count; i++)
            {

                birdEntitys[i].TargetEntity = targetEntity;
                birdEntitys[i].CenterEntity = centerEntity;
                birdEntitys[i].SimilarFlockerEntitys = new FlockSteering[birdEntitys.Count - 1];
                int index = 0;
                for (int j = 0; j < birdEntitys.Count; j++)
                {
                    if (i == j) continue; //跳过自己
                    birdEntitys[i].SimilarFlockerEntitys[index] = birdEntitys[j];
                    //将其他实体添加到相似实体列表中   
                    index++;
                }
                birdEntitys[i].Start(); //调用每个实体的OnStart方法进行初始化
            }
        }

        protected override void OnStop()
        {
            if (birdEntitys != null)
            {
                foreach (var item in birdEntitys)
                {
                    if (item != null) item.Entity?.Destroy();
                }
            }
            if (targetEntity != null) targetEntity?.Destroy();
            if (centerEntity != null) centerEntity?.Destroy();
        }
    }
}