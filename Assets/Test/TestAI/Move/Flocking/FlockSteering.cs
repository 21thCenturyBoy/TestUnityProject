using System.Collections.Generic;
using TestAI.Move.Kinematic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace TestAI.Move.Flocking
{
    public class FlockSteeringConfig
    {
        private static FlockSteeringConfig m_Instance;

        public static FlockSteeringConfig Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new FlockSteeringConfig();
                }
                return m_Instance;
            }
        }
        //最大速度
        [AIParam_Float("总最大速度")]
        public float maxSpeed = 5f;

        [AIParam_Float("加速度阈值")]
        public float epsilon = 0.1f;

        [AIParam_Float("检查碰撞的最小距离")]
        public float avoidDistance = 10f;

        [AIParam_Float("基于障碍物表面的距离")]
        public float lookAhead = 10f;

        [AIParam_Float("----分离权重----")]
        public float Separation_Weight = 0.8f;
        [AIParam_Float("分离最大加速度(可负)")]
        public float Separation_maxAcceleration = -5f; // 最大加速度
        [AIParam_Float("触发分离半径阈值")]
        public float Separation_threshold = 5f; // 分离半径
        [AIParam_Float("衰减系数")]
        public float Separation_decayCoefficient = 3f; // 分离半径

        [AIParam_Float("----对齐面向权重----")]
        public float FaceTargetForward_Weight = 1f;
        [AIParam_Float("最大旋转加速度（弧度）")]
        public float FaceTargetForward_maxAngularAcceleration = 3.14f;
        [AIParam_Float("最大旋转速度（弧度）")]
        public float FaceTargetForward_maxRotate = 3f;
        [AIParam_Float("到达目标的时间")]
        public float FaceTargetForward_arrive_time = 0.1f;//到达目标的时间
        [AIParam_Float("朝向减缓区间（弧度）")]
        public float FaceTargetForward_slowRadius = 0.4f; //减速半径
        [AIParam_Float("朝向最小近似值（弧度）")]
        public float FaceTargetForward_targetRadius = 0.05f;

        [AIParam_Float("----寻找目标权重----")]
        public float Seek_Weight = 1f;
        [AIParam_Float("最大加速度")]
        public float Seek_maxAcceleration = 5f;
        [AIParam_Float("最大速度")]
        public float Seek_maxSpeed = 10f;

        [AIParam_Float("----聚集权重----")]
        public float Arrive_Weight = 0.7f;
        [AIParam_Float("最大加速度")]
        public float Arrive_maxAcceleration = 10f;
        [AIParam_Float("最大速度")]
        public float Arrive_maxSpeed = 10f;
        [AIParam_Float("到达目标的时间")]
        public float Arrive_arrive_time = 0.5f;//到达目标的时间
        [AIParam_Float("目标半径范围")]
        public float Arrive_targetRadius = 5f;//目标半径范围
        [AIParam_Float("减速半径")]
        public float Arrive_slowRadius = 20f; //减速半径


    }

    public class FlockSteering : BlendedSteering
    {
        public IKinematicEntity CenterEntity;
        public IKinematicEntity TargetEntity;

        public IKinematicEntity Entity;
        public IKinematicEntity[] SimilarFlockerEntitys;

        protected override void OnStart()
        {
            //分离
            Steering_Separation separate = new Steering_Separation();
            separate.currentEntity = Entity;
            separate.targetEntitys = new IKinematicEntity[SimilarFlockerEntitys.Length];

            //将相似实体添加到分离目标列表中
            for (int i = 0; i < SimilarFlockerEntitys.Length; i++)
            {
                separate.targetEntitys[i] = SimilarFlockerEntitys[i];
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

            behaviors = new List<BehaviorAndWeight>
            {
                new BehaviorAndWeight(separate, 0),
                new BehaviorAndWeight(steering_FaceTargetForward, 0),
                new BehaviorAndWeight(seek, 0),
                new BehaviorAndWeight(cohere, 0),
            };
        }

        public void UpdateConfig()
        {

            behaviors[0].Weight = FlockSteeringConfig.Instance.Separation_Weight;
            Steering_Separation separateBehavior = behaviors[0].Behavior as Steering_Separation;
            separateBehavior.maxAcceleration = FlockSteeringConfig.Instance.Separation_maxAcceleration;
            separateBehavior.threshold = FlockSteeringConfig.Instance.Separation_threshold;
            separateBehavior.decayCoefficient = FlockSteeringConfig.Instance.Separation_decayCoefficient;

            behaviors[1].Weight = FlockSteeringConfig.Instance.FaceTargetForward_Weight;
            Steering_FaceTargetForward faceTargetForwardBehavior = behaviors[1].Behavior as Steering_FaceTargetForward;
            faceTargetForwardBehavior.maxAngularAcceleration = FlockSteeringConfig.Instance.FaceTargetForward_maxAngularAcceleration;
            faceTargetForwardBehavior.maxRotate = FlockSteeringConfig.Instance.FaceTargetForward_maxRotate;
            faceTargetForwardBehavior.arrive_time = FlockSteeringConfig.Instance.FaceTargetForward_arrive_time;
            faceTargetForwardBehavior.slowRadius = FlockSteeringConfig.Instance.FaceTargetForward_slowRadius; //朝向减缓区间
            faceTargetForwardBehavior.targetRadius = FlockSteeringConfig.Instance.FaceTargetForward_targetRadius; //朝向最小近似值

            behaviors[2].Weight = FlockSteeringConfig.Instance.Seek_Weight;
            Steering_Seek seekBehavior = behaviors[2].Behavior as Steering_Seek;
            seekBehavior.maxAcceleration = FlockSteeringConfig.Instance.Seek_maxAcceleration;
            seekBehavior.maxSpeed = FlockSteeringConfig.Instance.Seek_maxSpeed;

            behaviors[3].Weight = FlockSteeringConfig.Instance.Arrive_Weight;
            Steering_Arrive arriveBehavior = behaviors[3].Behavior as Steering_Arrive;
            arriveBehavior.maxAcceleration = FlockSteeringConfig.Instance.Arrive_maxAcceleration;
            arriveBehavior.maxSpeed = FlockSteeringConfig.Instance.Arrive_maxSpeed;
            arriveBehavior.arrive_time = FlockSteeringConfig.Instance.Arrive_arrive_time; //到达目标的时间
            arriveBehavior.targetRadius = FlockSteeringConfig.Instance.Arrive_targetRadius; //目标半径范围
            arriveBehavior.slowRadius = FlockSteeringConfig.Instance.Arrive_slowRadius; //减速半径
        }
    }


    public class Steering_ObstacleAvoidance_new : Steering_Arrive
    {
        [AIParam_Float("检查碰撞的最小距离")]
        public float avoidDistance = 10f;

        [AIParam_Float("基于障碍物表面的距离")]
        public float lookAhead = 10f;

        public List<IKinematicEntity> ObstacleEntitys;

        public override Vector3 GetTargetPos()
        {
            //TODO : 预测障碍物位置，计算避开位置(这里是否应该使用寻路？)
            RaycastHit[] hits;
            if (UtilsTool.PhysicsRaycastAll(currentEntity.Position, currentEntity.Velocity, lookAhead, out hits))
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit hit = hits[i];
                    if (hit.collider == null || hit.collider.gameObject == null) continue; //跳过无效的碰撞体
                    //检查是否是障碍物实体
                    IKinematicEntity obstacleEntity = hit.collider.GetComponent<IKinematicEntity>();
                    if (obstacleEntity != null && ObstacleEntitys.Contains(obstacleEntity))
                    {
                        //如果是障碍物实体，则返回避开位置
                        return hit.point + (hit.normal * avoidDistance);
                    }
                }
                return base.GetTargetPos();
            }
            else
            {
                return base.GetTargetPos();
            }
        }
    }

    public class FlockSteering_ObstacleAvoidance : PrioritySteering
    {
        public IKinematicEntity CenterEntity;
        public IKinematicEntity TargetEntity;

        public IKinematicEntity Entity;
        public IKinematicEntity[] SimilarFlockerEntitys;

        public List<IKinematicEntity> ObstacleEntitys;

        protected override void OnStart()
        {
            // 设置集群行为
            FlockSteering flockSteering = new FlockSteering();
            flockSteering.Entity = Entity;
            flockSteering.CenterEntity = CenterEntity;
            flockSteering.TargetEntity = TargetEntity;
            flockSteering.SimilarFlockerEntitys = SimilarFlockerEntitys;

            flockSteering.Start(); //调用每个实体的OnStart方法进行初始化

            //  避免碰撞组
            //  避开障碍物&墙体、躲避其他角色的行为(非族群内部)

            Steering_ObstacleAvoidance_new avoid = new Steering_ObstacleAvoidance_new();
            avoid.currentEntity = Entity;
            avoid.targetEntity = CenterEntity;
            avoid.ObstacleEntitys = ObstacleEntitys;
            BlendedSteering highPrioritySteering = new BlendedSteering();
            highPrioritySteering.behaviors = new List<BehaviorAndWeight>
            {
                new BehaviorAndWeight(avoid, 1)
            };

            //  分离组
            //  躲避过于靠近的其他实体(族群内部)
            Steering_Separation separate = new Steering_Separation();
            separate.currentEntity = Entity;
            separate.targetEntitys = new IKinematicEntity[SimilarFlockerEntitys.Length];

            Steering_Arrive cohere = new Steering_Arrive();
            cohere.currentEntity = Entity;
            cohere.targetEntity = CenterEntity;

            BlendedSteering normalPrioritySteering = new BlendedSteering();
            normalPrioritySteering.behaviors = new List<BehaviorAndWeight>
            {
                new BehaviorAndWeight(separate, 1),
                new BehaviorAndWeight(cohere, 1),
            };


            //  追逐组
            //  寻找目标实体(族群内部)，归位目标的追逐
            //Steering_Arrive seek = new Steering_Arrive();
            //seek.targetEntity = TargetEntity;
            //seek.currentEntity = Entity;

            BlendedSteering lowPrioritySteering = new BlendedSteering();
            lowPrioritySteering.behaviors = new List<BehaviorAndWeight>
            {
                new BehaviorAndWeight(flockSteering, 1),
            };


            groups = new List<BlendedSteering>
            {
                highPrioritySteering,
                normalPrioritySteering,
                lowPrioritySteering,
            };
        }

        public void UpdateConfig()
        {

            //FlockSteering flockSteering = groups[0] as FlockSteering;
            //flockSteering.UpdateConfig();

            BlendedSteering highPrioritySteering = groups[0] as BlendedSteering;
            Steering_ObstacleAvoidance_new avoidBehavior = highPrioritySteering.behaviors[0].Behavior as Steering_ObstacleAvoidance_new;
            avoidBehavior.maxAcceleration = FlockSteeringConfig.Instance.Arrive_maxAcceleration;
            avoidBehavior.maxSpeed = FlockSteeringConfig.Instance.Arrive_maxSpeed;
            avoidBehavior.avoidDistance = FlockSteeringConfig.Instance.avoidDistance; //检查碰撞的最小距离
            avoidBehavior.lookAhead = FlockSteeringConfig.Instance.lookAhead; //基于障碍物表面的距离
            avoidBehavior.arrive_time = FlockSteeringConfig.Instance.Arrive_arrive_time; //到达目标的时间
            avoidBehavior.targetRadius = FlockSteeringConfig.Instance.Arrive_targetRadius; //目标半径范围
            avoidBehavior.slowRadius = FlockSteeringConfig.Instance.Arrive_slowRadius; //减速半径
            highPrioritySteering.behaviors[0].Weight = 1;


            BlendedSteering normalPrioritySteering = groups[1] as BlendedSteering;
            Steering_Separation separateBehavior = normalPrioritySteering.behaviors[0].Behavior as Steering_Separation;
            separateBehavior.maxAcceleration = FlockSteeringConfig.Instance.Separation_maxAcceleration;
            separateBehavior.threshold = FlockSteeringConfig.Instance.Separation_threshold;
            separateBehavior.decayCoefficient = FlockSteeringConfig.Instance.Separation_decayCoefficient;
            normalPrioritySteering.behaviors[0].Weight = FlockSteeringConfig.Instance.Separation_Weight;

            Steering_Arrive arriveBehavior = normalPrioritySteering.behaviors[1].Behavior as Steering_Arrive;
            arriveBehavior.maxAcceleration = FlockSteeringConfig.Instance.Arrive_maxAcceleration;
            arriveBehavior.maxSpeed = FlockSteeringConfig.Instance.Arrive_maxSpeed;
            arriveBehavior.arrive_time = FlockSteeringConfig.Instance.Arrive_arrive_time; //到达目标的时间
            arriveBehavior.targetRadius = FlockSteeringConfig.Instance.Arrive_targetRadius; //目标半径范围
            arriveBehavior.slowRadius = FlockSteeringConfig.Instance.Arrive_slowRadius; //减速半径
            normalPrioritySteering.behaviors[1].Weight = FlockSteeringConfig.Instance.Arrive_Weight;


            BlendedSteering lowPrioritySteering = groups[2] as BlendedSteering;
            FlockSteering flockSteering = lowPrioritySteering.behaviors[0].Behavior as FlockSteering;
            flockSteering.UpdateConfig();
            lowPrioritySteering.behaviors[0].Weight = 1;

            epsilon = FlockSteeringConfig.Instance.epsilon;
        }
    }

    [AILogicType("蜂拥集群管理_Blended")]
    public class FlockLogicManager_BlendedSteering : KinematicLogic
    {

        public IKinematicEntity targetEntity;
        public IKinematicEntity centerEntity;

        public List<FlockSteering> birdEntitys;

        #region 参数配置
        //最大速度
        [AIParam_Float("总最大速度")]
        public float maxSpeed { get => FlockSteeringConfig.Instance.maxSpeed; set => FlockSteeringConfig.Instance.maxSpeed = value; }

        [AIParam_Float("----分离权重----")]
        public float Separation_Weight { get => FlockSteeringConfig.Instance.Separation_Weight; set => FlockSteeringConfig.Instance.Separation_Weight = value; }
        [AIParam_Float("分离最大加速度(可负)")]
        public float Separation_maxAcceleration { get => FlockSteeringConfig.Instance.Separation_maxAcceleration; set => FlockSteeringConfig.Instance.Separation_maxAcceleration = value; } // 最大加速度
        [AIParam_Float("触发分离半径阈值")]
        public float Separation_threshold { get => FlockSteeringConfig.Instance.Separation_threshold; set => FlockSteeringConfig.Instance.Separation_threshold = value; } // 分离半径
        [AIParam_Float("衰减系数")]
        public float Separation_decayCoefficient { get => FlockSteeringConfig.Instance.Separation_decayCoefficient; set => FlockSteeringConfig.Instance.Separation_decayCoefficient = value; } // 分离半径

        [AIParam_Float("----对齐面向权重----")]
        public float FaceTargetForward_Weight { get => FlockSteeringConfig.Instance.FaceTargetForward_Weight; set => FlockSteeringConfig.Instance.FaceTargetForward_Weight = value; }
        [AIParam_Float("最大旋转加速度（弧度）")]
        public float FaceTargetForward_maxAngularAcceleration { get => FlockSteeringConfig.Instance.FaceTargetForward_maxAngularAcceleration; set => FlockSteeringConfig.Instance.FaceTargetForward_maxAngularAcceleration = value; }
        [AIParam_Float("最大旋转速度（弧度）")]
        public float FaceTargetForward_maxRotate { get => FlockSteeringConfig.Instance.FaceTargetForward_maxRotate; set => FlockSteeringConfig.Instance.FaceTargetForward_maxRotate = value; }
        [AIParam_Float("到达目标的时间")]
        public float FaceTargetForward_arrive_time { get => FlockSteeringConfig.Instance.FaceTargetForward_arrive_time; set => FlockSteeringConfig.Instance.FaceTargetForward_arrive_time = value; }//到达目标的时间
        [AIParam_Float("朝向减缓区间（弧度）")]
        public float FaceTargetForward_slowRadius { get => FlockSteeringConfig.Instance.FaceTargetForward_slowRadius; set => FlockSteeringConfig.Instance.FaceTargetForward_slowRadius = value; } //减速半径
        [AIParam_Float("朝向最小近似值（弧度）")]
        public float FaceTargetForward_targetRadius { get => FlockSteeringConfig.Instance.FaceTargetForward_targetRadius; set => FlockSteeringConfig.Instance.FaceTargetForward_targetRadius = value; }

        [AIParam_Float("----寻找目标权重----")]
        public float Seek_Weight { get => FlockSteeringConfig.Instance.Seek_Weight; set => FlockSteeringConfig.Instance.Seek_Weight = value; }
        [AIParam_Float("最大加速度")]
        public float Seek_maxAcceleration { get => FlockSteeringConfig.Instance.Seek_maxAcceleration; set => FlockSteeringConfig.Instance.Seek_maxAcceleration = value; }
        [AIParam_Float("最大速度")]
        public float Seek_maxSpeed { get => FlockSteeringConfig.Instance.Seek_maxSpeed; set => FlockSteeringConfig.Instance.Seek_maxSpeed = value; }

        [AIParam_Float("----聚集权重----")]
        public float Arrive_Weight { get => FlockSteeringConfig.Instance.Arrive_Weight; set => FlockSteeringConfig.Instance.Arrive_Weight = value; }
        [AIParam_Float("最大加速度")]
        public float Arrive_maxAcceleration { get => FlockSteeringConfig.Instance.Arrive_maxAcceleration; set => FlockSteeringConfig.Instance.Arrive_maxAcceleration = value; }
        [AIParam_Float("最大速度")]
        public float Arrive_maxSpeed { get => FlockSteeringConfig.Instance.Arrive_maxSpeed; set => FlockSteeringConfig.Instance.Arrive_maxSpeed = value; }
        [AIParam_Float("到达目标的时间")]
        public float Arrive_arrive_time { get => FlockSteeringConfig.Instance.Arrive_arrive_time; set => FlockSteeringConfig.Instance.Arrive_arrive_time = value; }//到达目标的时间
        [AIParam_Float("目标半径范围")]
        public float Arrive_targetRadius { get => FlockSteeringConfig.Instance.Arrive_targetRadius; set => FlockSteeringConfig.Instance.Arrive_targetRadius = value; }//目标半径范围
        [AIParam_Float("减速半径")]
        public float Arrive_slowRadius { get => FlockSteeringConfig.Instance.Arrive_slowRadius; set => FlockSteeringConfig.Instance.Arrive_slowRadius = value; } //减速半径
        #endregion 参数配置

        protected override void OnFixedUpdate()
        {
            //计算中心实体位置
            Vector3 centerPosition = Vector3.zero;
            Vector3 velocity = Vector3.zero;
            for (int i = 0; i < birdEntitys.Count; i++)
            {

                birdEntitys[i].UpdateConfig();

                centerPosition += birdEntitys[i].Entity.GetStaticStae().Position;
                velocity += birdEntitys[i].Entity.Velocity;
            }

            velocity /= birdEntitys.Count;
            centerPosition /= birdEntitys.Count;
            centerEntity.SetPosition(centerPosition);
            float centerDirection = UtilsTool.ComputeOrientation(velocity); //计算平均方向
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
                birdEntitys[i].SimilarFlockerEntitys = new IKinematicEntity[birdEntitys.Count - 1];
                int index = 0;
                for (int j = 0; j < birdEntitys.Count; j++)
                {
                    if (i == j) continue; //跳过自己
                    birdEntitys[i].SimilarFlockerEntitys[index] = birdEntitys[j].Entity;
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

    [AILogicType("蜂拥集群管理_Avoidance")]
    public class FlockLogicManager_PrioritySteering : KinematicLogic
    {
        public IKinematicEntity targetEntity;
        public IKinematicEntity centerEntity;

        public List<FlockSteering_ObstacleAvoidance> birdEntitys;
        public List<IKinematicEntity> obstacleEntitys;

        #region 参数配置
        //最大速度
        [AIParam_Float("总最大速度")]
        public float maxSpeed { get => FlockSteeringConfig.Instance.maxSpeed; set => FlockSteeringConfig.Instance.maxSpeed = value; }

        [AIParam_Float("加速度阈值")]
        public float epsilon { get => FlockSteeringConfig.Instance.epsilon; set => FlockSteeringConfig.Instance.epsilon = value; }

        [AIParam_Float("检查碰撞的最小距离")]
        public float avoidDistance { get => FlockSteeringConfig.Instance.avoidDistance; set => FlockSteeringConfig.Instance.avoidDistance = value; }

        [AIParam_Float("基于障碍物表面的距离")]
        public float lookAhead { get => FlockSteeringConfig.Instance.lookAhead; set => FlockSteeringConfig.Instance.lookAhead = value; }

        [AIParam_Float("----分离权重----")]
        public float Separation_Weight { get => FlockSteeringConfig.Instance.Separation_Weight; set => FlockSteeringConfig.Instance.Separation_Weight = value; }
        [AIParam_Float("分离最大加速度(可负)")]
        public float Separation_maxAcceleration { get => FlockSteeringConfig.Instance.Separation_maxAcceleration; set => FlockSteeringConfig.Instance.Separation_maxAcceleration = value; } // 最大加速度
        [AIParam_Float("触发分离半径阈值")]
        public float Separation_threshold { get => FlockSteeringConfig.Instance.Separation_threshold; set => FlockSteeringConfig.Instance.Separation_threshold = value; } // 分离半径
        [AIParam_Float("衰减系数")]
        public float Separation_decayCoefficient { get => FlockSteeringConfig.Instance.Separation_decayCoefficient; set => FlockSteeringConfig.Instance.Separation_decayCoefficient = value; } // 分离半径

        [AIParam_Float("----对齐面向权重----")]
        public float FaceTargetForward_Weight { get => FlockSteeringConfig.Instance.FaceTargetForward_Weight; set => FlockSteeringConfig.Instance.FaceTargetForward_Weight = value; }
        [AIParam_Float("最大旋转加速度（弧度）")]
        public float FaceTargetForward_maxAngularAcceleration { get => FlockSteeringConfig.Instance.FaceTargetForward_maxAngularAcceleration; set => FlockSteeringConfig.Instance.FaceTargetForward_maxAngularAcceleration = value; }
        [AIParam_Float("最大旋转速度（弧度）")]
        public float FaceTargetForward_maxRotate { get => FlockSteeringConfig.Instance.FaceTargetForward_maxRotate; set => FlockSteeringConfig.Instance.FaceTargetForward_maxRotate = value; }
        [AIParam_Float("到达目标的时间")]
        public float FaceTargetForward_arrive_time { get => FlockSteeringConfig.Instance.FaceTargetForward_arrive_time; set => FlockSteeringConfig.Instance.FaceTargetForward_arrive_time = value; }//到达目标的时间
        [AIParam_Float("朝向减缓区间（弧度）")]
        public float FaceTargetForward_slowRadius { get => FlockSteeringConfig.Instance.FaceTargetForward_slowRadius; set => FlockSteeringConfig.Instance.FaceTargetForward_slowRadius = value; } //减速半径
        [AIParam_Float("朝向最小近似值（弧度）")]
        public float FaceTargetForward_targetRadius { get => FlockSteeringConfig.Instance.FaceTargetForward_targetRadius; set => FlockSteeringConfig.Instance.FaceTargetForward_targetRadius = value; }

        [AIParam_Float("----寻找目标权重----")]
        public float Seek_Weight { get => FlockSteeringConfig.Instance.Seek_Weight; set => FlockSteeringConfig.Instance.Seek_Weight = value; }
        [AIParam_Float("最大加速度")]
        public float Seek_maxAcceleration { get => FlockSteeringConfig.Instance.Seek_maxAcceleration; set => FlockSteeringConfig.Instance.Seek_maxAcceleration = value; }
        [AIParam_Float("最大速度")]
        public float Seek_maxSpeed { get => FlockSteeringConfig.Instance.Seek_maxSpeed; set => FlockSteeringConfig.Instance.Seek_maxSpeed = value; }

        [AIParam_Float("----聚集权重----")]
        public float Arrive_Weight { get => FlockSteeringConfig.Instance.Arrive_Weight; set => FlockSteeringConfig.Instance.Arrive_Weight = value; }
        [AIParam_Float("最大加速度")]
        public float Arrive_maxAcceleration { get => FlockSteeringConfig.Instance.Arrive_maxAcceleration; set => FlockSteeringConfig.Instance.Arrive_maxAcceleration = value; }
        [AIParam_Float("最大速度")]
        public float Arrive_maxSpeed { get => FlockSteeringConfig.Instance.Arrive_maxSpeed; set => FlockSteeringConfig.Instance.Arrive_maxSpeed = value; }
        [AIParam_Float("到达目标的时间")]
        public float Arrive_arrive_time { get => FlockSteeringConfig.Instance.Arrive_arrive_time; set => FlockSteeringConfig.Instance.Arrive_arrive_time = value; }//到达目标的时间
        [AIParam_Float("目标半径范围")]
        public float Arrive_targetRadius { get => FlockSteeringConfig.Instance.Arrive_targetRadius; set => FlockSteeringConfig.Instance.Arrive_targetRadius = value; }//目标半径范围
        [AIParam_Float("减速半径")]
        public float Arrive_slowRadius { get => FlockSteeringConfig.Instance.Arrive_slowRadius; set => FlockSteeringConfig.Instance.Arrive_slowRadius = value; } //减速半径
        #endregion 参数配置

        protected override void OnFixedUpdate()
        {
            //计算中心实体位置
            Vector3 centerPosition = Vector3.zero;
            Vector3 velocity = Vector3.zero;
            for (int i = 0; i < birdEntitys.Count; i++)
            {
                birdEntitys[i].UpdateConfig();

                centerPosition += birdEntitys[i].Entity.GetStaticStae().Position;
                velocity += birdEntitys[i].Entity.Velocity;
            }

            velocity /= birdEntitys.Count;
            centerPosition /= birdEntitys.Count;
            centerEntity.SetPosition(centerPosition);
            float centerDirection = UtilsTool.ComputeOrientation(velocity); //计算平均方向
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
            //创建障碍
            obstacleEntitys = new List<IKinematicEntity>();
            for (int i = 0; i < 3; i++)
            {
                Navigation_Obstacle_Item point_Obstacle = UtilsTool.CreateNavigation_Obstacle() as Navigation_Obstacle_Item;
                obstacleEntitys.Add(point_Obstacle);
                point_Obstacle.AllowDrag = true;
                point_Obstacle.SetPosition(new Vector3(0, 0, i * 5));
            }

            birdEntitys = new List<FlockSteering_ObstacleAvoidance>();
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
                birdEntitys.Add(new FlockSteering_ObstacleAvoidance());
                birdEntitys[i].Entity = item;
                birdEntitys[i].ObstacleEntitys = obstacleEntitys; //设置障碍物实体列表
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
                birdEntitys[i].SimilarFlockerEntitys = new IKinematicEntity[birdEntitys.Count - 1];
                int index = 0;
                for (int j = 0; j < birdEntitys.Count; j++)
                {
                    if (i == j) continue; //跳过自己
                    birdEntitys[i].SimilarFlockerEntitys[index] = birdEntitys[j].Entity;
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
            if (obstacleEntitys != null)
            {
                foreach (var item in obstacleEntitys)
                {
                    if (item != null) item?.Destroy();
                }
            }
        }
    }
}