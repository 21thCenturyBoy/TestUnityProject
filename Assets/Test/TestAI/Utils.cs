using PlasticGui.Configuration.CloudEdition.Welcome;
using System;
using System.Collections.Generic;
using TestAI.Move;
using TestAI.Move.Flocking;
using TestAI.Move.Kinematic;
using UnityEngine;
namespace TestAI
{
    public enum Color
    {
        red, green, blue
    }

    /// <summary>
    /// 静态状态
    /// </summary>
    public struct StaticStae
    {
        //位置
        public Vector3 Position;
        //方向（弧度）
        public float Orientation;
    }

    /// <summary>
    /// 转向输出加速度
    /// </summary>
    public struct SteeringOutput
    {
        //线加速度
        public Vector3 Linear;
        //角加速度
        public float Angular;

        //重写运算符
        public static SteeringOutput operator *(float weight, SteeringOutput output)
        {
            SteeringOutput result = new SteeringOutput();
            result.Linear = output.Linear * weight;
            result.Angular = output.Angular * weight;
            return result;
        }
        public static SteeringOutput operator *(SteeringOutput output, float weight)
        {
            return weight * output;
        }
        public static SteeringOutput operator +(SteeringOutput a, SteeringOutput b)
        {
            SteeringOutput result = new SteeringOutput();
            result.Linear = a.Linear + b.Linear;
            result.Angular = a.Angular + b.Angular;
            return result;
        }
        public static SteeringOutput operator -(SteeringOutput a, SteeringOutput b)
        {
            SteeringOutput result = new SteeringOutput();
            result.Linear = a.Linear - b.Linear;
            result.Angular = a.Angular - b.Angular;
            return result;
        }
    }

    /// <summary>
    /// 运动学输出速度
    /// </summary>
    public struct KinematicOutput
    {
        //线速度
        public Vector3 Velocity;
        //角速度
        public float Rotation;
    }

    public interface IKinematicLogic
    {
        void Start();
        void FixedUpdate();
        void Stop();
    }

    public interface IPath
    {
        public float GetParam(Vector3 position, float lastParam);

        public Vector3 GetPosition(float param);
    }
    public interface IPoint
    {
        void SetPosition(Vector3 pos);
        Vector3 GetPosition();
    }

    public interface IKinematicEntity
    {
        //位置
        public Vector3 Position { get; }
        //方向（弧度）
        public float Orientation { get; }
        //线速度
        public Vector3 Velocity { get; set; }
        //角速度（弧度每秒）
        public float Rotation { get; set; }
        StaticStae GetStaticStae();
        void SetStaticStae(StaticStae stae);
        void SetOrientation(float orientation);
        void SetPosition(Vector3 pos);
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class AIParam_Float : Attribute
    {
        public String ParmName { get; set; }
        public AIParam_Float(String name = null)
        {
            ParmName = name;
        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AIParam_Info : Attribute
    {
        public String ParmName { get; set; }
        public AIParam_Info(String name = null)
        {
            ParmName = name;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AITest_Button : Attribute
    {
        public String ParmName { get; set; }
        public AITest_Button(String name = null)
        {
            ParmName = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AILogicType : Attribute
    {
        public String ParmName { get; set; }
        public AILogicType(String name = null)
        {
            ParmName = name;
        }
    }


    public static class UtilsTool
    {

        private static Dictionary<String, System.Type> m_KinematicLogicTypeCache;

        public static Dictionary<String, System.Type> GetKinematicLogicTypeCache()
        {
            if (m_KinematicLogicTypeCache == null)
            {
                m_KinematicLogicTypeCache = new Dictionary<string, System.Type>();
                //获取所有IKinematicLogic类型
                var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    if (typeof(KinematicLogic).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        //获取ILogicType特性
                        var logicTypeAttr = type.GetCustomAttributes(typeof(AILogicType), false);
                        if (logicTypeAttr.Length > 0)
                        {
                            var attr = logicTypeAttr[0] as AILogicType;
                            m_KinematicLogicTypeCache.Add(attr.ParmName, type);
                        }
                        //else
                        //{
                        //    //如果没有ILogicType特性，则使用类名作为逻辑类型名称
                        //    m_KinematicLogicTypeCache.Add(type.Name, type);
                        //}
                    }
                }

            }
            return m_KinematicLogicTypeCache;
        }

        public static KinematicLogic CreateKinematicLogic(String logicTypeName)
        {
            if (GetKinematicLogicTypeCache().TryGetValue(logicTypeName, out System.Type logicType))
            {
                KinematicLogic logic = Activator.CreateInstance(logicType) as KinematicLogic;

                return logic;
            }
            else
            {
                Debug.LogError($"未找到逻辑类型: {logicTypeName}");
                return null;
            }
        }

        /// <summary>
        /// 通过当前方向和速度计算新的方向。
        /// </summary>
        /// <param name="current"></param>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public static float NewOrientation(float current, Vector3 velocity)
        {
            //计算当前方向与速度方向之间的角度差
            if (velocity.sqrMagnitude < Mathf.Epsilon)
            {
                return current; // 如果速度接近零，保持当前方向
            }
            else
            {
                // 计算目标方向的角度
                // Mathf.Atan2 返回的是弧度值，范围在 [-π, π] 之间
                // 这里的 velocity.x 和 velocity.z 是因为 Unity 中的 Y 轴是向上，X 轴是向前，Z 轴是向右
                float target = Mathf.Atan2(velocity.x, velocity.z);
                //左手坐标系
                //float target = Mathf.Atan2(-velocity.x, velocity.y);
                return target;
            }
        }

        /// <summary>
        /// 计算朝向
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static float ComputeOrientation(Vector3 direction)
        {
            // 取transform.forward在XZ平面上的投影，计算与世界前方(Vector3.forward)的夹角
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
                float angle = Mathf.Atan2(direction.x, direction.z); // 弧度
                return angle;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 方向（弧度）转向量
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="orientation"></param>
        public static Vector3 OrientationToVector(this StaticStae stae)
        {
            //Y轴为0，XZ平面上计算方向向量
            float x = Mathf.Sin(stae.Orientation);
            float z = Mathf.Cos(stae.Orientation);
            return new Vector3(x, 0, z).normalized;
        }

        /// <summary>
        /// 方向（弧度）转向量
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="orientation"></param>
        public static Vector3 OrientationToVector(float orientation)
        {
            //Y轴为0，XZ平面上计算方向向量
            float x = Mathf.Sin(orientation);
            float z = Mathf.Cos(orientation);
            return new Vector3(x, 0, z).normalized;
        }

        /// <summary>
        /// 创建随机静态状态(最小30)
        /// </summary>
        /// <returns></returns>
        public static StaticStae CreateRandomStaticStae(Vector2 rangePos, bool rangeRotate = true)
        {

            float x = rangePos.x;
            float z = rangePos.y;

            //别生成中心附近
            float pos_x = UnityEngine.Random.Range(30, x);
            float pos_z = UnityEngine.Random.Range(30, z);

            if (pos_x % 2 > 1)
            {
                pos_x = pos_x * -1;
            }

            if (pos_z % 2 > 1)
            {
                pos_z = pos_z * -1;
            }
            float orientation = 0f;

            if (rangeRotate)
            {
                orientation = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
            }
            StaticStae stae = new StaticStae();
            stae.Position = new Vector3(pos_x, 0, pos_z);
            stae.Orientation = orientation;

            return stae;
        }

        public static float GetRandomOne()
        {
            return UnityEngine.Random.Range(-1f, 1f);
        }

        public static IKinematicEntity CreateNavigation_AI()
        {
            var navigation_prefab = Resources.Load<GameObject>("Navigation_AI");
            GameObject new_inst = GameObject.Instantiate(navigation_prefab);

            return new_inst.GetComponent<IKinematicEntity>();
        }

        public static IKinematicEntity CreateNavigation_Point()
        {
            var navigation_prefab = Resources.Load<GameObject>("Navigation_Point");
            GameObject new_inst = GameObject.Instantiate(navigation_prefab);

            return new_inst.GetComponent<IKinematicEntity>();
        }

        public static Navigation_Obstacle_Item CreateNavigation_Obstacle()
        {
            var navigation_prefab = Resources.Load<GameObject>("Navigation_Obstacle");
            GameObject new_inst = GameObject.Instantiate(navigation_prefab);

            return new_inst.GetComponent<Navigation_Obstacle_Item>();
        }

        public static IKinematicEntity CreateNavigation_Center()
        {
            var navigation_prefab = Resources.Load<GameObject>("Navigation_Center");
            GameObject new_inst = GameObject.Instantiate(navigation_prefab);

            return new_inst.GetComponent<IKinematicEntity>();
        }


        public static bool PhysicsRaycast(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hitinfo)
        {
            return Physics.Raycast(origin, direction, out hitinfo, maxDistance);

        }
        public static bool PhysicsRaycastAll(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit[] hitinfos)
        {
            // 获取射线路径上的所有碰撞
            hitinfos = Physics.RaycastAll(origin, direction, maxDistance);

            // 按距离对结果进行排序
            System.Array.Sort(hitinfos, (a, b) => a.distance.CompareTo(b.distance));

            return hitinfos.Length != 0;

        }

        public static void Destroy(this IKinematicEntity entity)
        {
            if (entity != null)
            {
                MonoBehaviour entityObj = entity as MonoBehaviour;
                GameObject.Destroy(entityObj.gameObject);
            }
        }

        public static void SetColor(this IKinematicEntity entity, Color color)
        {
            UnityEngine.Color newColor = UnityEngine.Color.white;
            switch (color)
            {
                case Color.red:
                    newColor = UnityEngine.Color.red;
                    break;
                case Color.green:
                    newColor = UnityEngine.Color.green;
                    break;
                case Color.blue:
                    newColor = UnityEngine.Color.blue;
                    break;
            }
            if (entity != null)
            {
                MonoBehaviour entityObj = entity as MonoBehaviour;
                entityObj.GetComponentInChildren<SpriteRenderer>().color = newColor;
            }
        }

        public static void AllowDrag(this IKinematicEntity entity, bool drag)
        {
            if (entity != null)
            {
                Navigation_AI_Item aI_Item = entity as Navigation_AI_Item;
                aI_Item.AllowDrag = drag;
                aI_Item.GetComponent<BoxCollider>().enabled = drag;
            }
        }

        public static void AutoMove(this IKinematicEntity entity, bool automove)
        {
            if (entity != null)
            {
                Navigation_AI_Item aI_Item = entity as Navigation_AI_Item;
                aI_Item.AutoMove = automove;
            }
        }

        /// <summary>
        /// 运动学应用
        /// </summary>
        /// <param name="stae"></param>
        /// <param name="steeringOutput"></param>
        public static void SteeringOutputApply(this ref StaticStae stae, SteeringOutput steeringOutput)
        {
            // 应用新的速度向量到位置
            stae.Position += steeringOutput.Linear;
            // 更新朝向
            stae.Orientation += steeringOutput.Angular;
        }


        /// <summary>
        /// 更新（标准运动学）手动更新方法(帧率低)，由于帧率低，可能会导致物体跳跃式移动。
        /// </summary>
        /// <param name="steering"></param>
        /// <param name="deltaTime"></param>
        public static void ForceUpdate(this IKinematicEntity entity, SteeringOutput steering, float deltaTime)
        {
            //加速度与位移公式：(vt²-v0²)=2as，s=v0t+at²/2，s2-s1=aT²。
            //更新位置
            float half_t = 0.5f * deltaTime * deltaTime;
            StaticStae staticStae = entity.GetStaticStae();
            staticStae.Position += entity.Velocity * deltaTime + steering.Linear * half_t;
            //更新方向
            staticStae.Orientation += entity.Rotation * deltaTime + steering.Angular * half_t;
            entity.SetStaticStae(staticStae);

            //更新线速度
            entity.Velocity += steering.Linear * deltaTime;
            //更新角速度
            entity.Rotation += steering.Angular * deltaTime;
        }

        /// <summary>
        /// 更新（运动学）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="kinematicOutput"></param>
        /// <param name="deltaTime"></param>
        public static void FixedUpdate(this IKinematicEntity entity, KinematicOutput kinematicOutput, float deltaTime)
        {
            //更新位置
            StaticStae staticStae = entity.GetStaticStae();
            staticStae.Position += entity.Velocity * deltaTime;
            //更新方向
            staticStae.Orientation += entity.Rotation * deltaTime;
            entity.SetStaticStae(staticStae);

            //更新线速度
            entity.Velocity = kinematicOutput.Velocity;
            //更新角速度
            entity.Rotation = kinematicOutput.Rotation;
        }

        /// <summary>
        /// 更新（转向行为）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="steering"></param>
        /// <param name="deltaTime"></param>
        public static void FixedUpdate(this IKinematicEntity entity, SteeringOutput steering, float deltaTime)
        {
            //更新位置
            StaticStae staticStae = entity.GetStaticStae();
            staticStae.Position += entity.Velocity * deltaTime;
            //更新方向
            staticStae.Orientation += entity.Rotation * deltaTime;
            entity.SetStaticStae(staticStae);

            //更新线速度
            entity.Velocity += steering.Linear * deltaTime;
            //更新角速度
            entity.Rotation += steering.Angular * deltaTime;
        }

        /// <summary>
        /// 更新（转向行为）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="steering"></param>
        /// <param name="deltaTime"></param>
        public static void FixedUpdate(this IKinematicEntity entity, SteeringOutput steering, float maxSpeed, float deltaTime)
        {
            //更新位置
            StaticStae staticStae = entity.GetStaticStae();
            staticStae.Position += entity.Velocity * deltaTime;
            //更新方向
            staticStae.Orientation += entity.Rotation * deltaTime;
            entity.SetStaticStae(staticStae);

            //更新线速度
            entity.Velocity += steering.Linear * deltaTime;
            //更新角速度
            entity.Rotation += steering.Angular * deltaTime;

            //检查速度是否超过最大速度
            if (entity.Velocity.magnitude > maxSpeed)
            {
                //如果超过最大速度，则归一化并乘以最大速度
                entity.Velocity = entity.Velocity.normalized * maxSpeed;
            }
        }


    }
}