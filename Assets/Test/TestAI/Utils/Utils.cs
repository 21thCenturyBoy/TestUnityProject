using PlasticGui.Configuration.CloudEdition.Welcome;
using System;
using System.Collections.Generic;
using TestAI.Move;
using TestAI.Move.Kinematic;
using UnityEngine;
namespace TestAI
{
    public enum Color
    {
        red, green, blue
    }

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
    }
}