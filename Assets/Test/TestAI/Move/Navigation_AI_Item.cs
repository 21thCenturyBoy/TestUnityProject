using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestAI.Move
{
    public class Navigation_AI_Item : MonoBehaviour, IKinematicEntity
    {
        public bool AllowDrag = false;

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
            }
        }

        public StaticStae GetStaticStae()
        {
            StaticStae stae = new StaticStae();
            stae.Position = transform.position;
            //计算朝向
            stae.Orientation = transform.ComputeOrientation();

            return stae;
        }

        public void SetStaticStae(StaticStae stae)
        {
            transform.SetOrientation(stae.Orientation);
            transform.position = stae.Position;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

