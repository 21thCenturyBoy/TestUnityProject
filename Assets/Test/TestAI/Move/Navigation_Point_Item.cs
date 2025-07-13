using TestAI.Move.Kinematic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace TestAI.Move
{
    public class Navigation_Point_Item : MonoBehaviour, IKinematicEntity, IPoint
    {
        public bool AllowDrag = false;

        public Vector3 Position { get => m_staticStae.Position; }
        public float Orientation { get => m_staticStae.Orientation; }
        public Vector3 Velocity { get; set; }
        public float Rotation { get; set; }

        private StaticStae m_staticStae;
        void Start()
        {
            // 初始化物体位置和朝向
            m_staticStae.Position = transform.position;
            m_staticStae.Orientation = UtilsTool.ComputeOrientation(transform.forward);

            Velocity = Vector3.zero;
            Rotation = 0f;
        }

        public void OnMouseDrag()
        {
            //允许拖拽,计算鼠标拖拽位置
            if (!AllowDrag) return;

            // 获取鼠标在屏幕上的位置
            Vector3 mouseScreenPos = Input.mousePosition;

            // 假设物体在XZ平面上移动，Y为物体当前高度
            float y = transform.position.y;

            // 将鼠标屏幕坐标转换为世界坐标
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
            Plane plane = new Plane(Vector3.up, new Vector3(0, y, 0));
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                transform.position = worldPos;
                m_staticStae.Position = transform.position;
            }
        }

        public StaticStae GetStaticStae() => m_staticStae;

        public void SetStaticStae(StaticStae stae)
        {
            SetOrientation(stae.Orientation);
            SetPosition(stae.Position);
        }
        /// <summary>
        /// 设置Transform方向（弧度）
        /// </summary>
        /// <param name="orientation"></param>
        public void SetOrientation(float orientation)
        {
            // 将弧度转换为角度
            float angle = orientation * Mathf.Rad2Deg;
            // 只设置Y轴旋转，保持X和Z为0
            transform.rotation = Quaternion.Euler(0, angle, 0);

            m_staticStae.Orientation = orientation;
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;

            m_staticStae.Position = pos;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}

