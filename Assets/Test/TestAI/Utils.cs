using System;
using TestAI.Move;
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
        public Vector3 Position { get; set; }
        //方向（弧度）
        public float Orientation { get; set; }

    }

    /// <summary>
    /// 转向输出速度
    /// </summary>
    public struct SteeringOutputVelocity
    {
        //线速度
        public Vector3 Line { get; set; }
        //角速度
        public float Angular { get; set; }
    }

    public interface IKinematicLogic
    {
        void Start();
        void FixedUpdate();
        void Stop();
    }

    public interface IKinematicEntity
    {
        StaticStae GetStaticStae();
        void SetStaticStae(StaticStae stae);
    }

    public class AIParm_Float : Attribute { }

    public static class UtilsTool
    {
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
        public static float ComputeOrientation(this Transform transform)
        {

            // 取transform.forward在XZ平面上的投影，计算与世界前方(Vector3.forward)的夹角
            Vector3 forward = transform.forward;
            forward.y = 0;
            if (forward.sqrMagnitude > 0.0001f)
            {
                forward.Normalize();
                float angle = Mathf.Atan2(forward.x, forward.z); // 弧度
                return angle;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 设置Transform方向（弧度）
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="orientation"></param>
        public static void SetOrientation(this Transform transform, float orientation)
        {
            // 将弧度转换为角度
            float angle = orientation * Mathf.Rad2Deg;
            // 只设置Y轴旋转，保持X和Z为0
            transform.rotation = Quaternion.Euler(0, angle, 0);
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
            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// 创建随机静态状态(最小30)
        /// </summary>
        /// <returns></returns>
        public static StaticStae CreateRandomStaticStae(Vector2 rangePos)
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

            StaticStae stae = new StaticStae();
            stae.Position = new Vector3(pos_x, 0, pos_z);

            return stae;
        }

        public static IKinematicEntity CreateNavigation_AI()
        {
            var navigation_prefab = Resources.Load<GameObject>("Navigation_AI");
            GameObject new_inst = GameObject.Instantiate(navigation_prefab);

            return new_inst.GetComponent<IKinematicEntity>();
        }

        public static void Destroy(this IKinematicEntity entity)
        {
            if (entity != null)
            {
                Navigation_AI_Item aI_Item = entity as Navigation_AI_Item;
                GameObject.Destroy(aI_Item.gameObject);
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
                Navigation_AI_Item aI_Item = entity as Navigation_AI_Item;
                aI_Item.GetComponentInChildren<SpriteRenderer>().color = newColor;
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

        /// <summary>
        /// 转向输出应用到静态状态上。
        /// </summary>
        /// <param name="stae"></param>
        /// <param name="steeringOutput"></param>
        public static void SteeringOutputApply(this ref StaticStae stae, SteeringOutputVelocity steeringOutput)
        {
            // 应用新的速度向量到位置
            stae.Position += steeringOutput.Line;
            // 更新朝向
            stae.Orientation += steeringOutput.Angular;
        }

    }
}