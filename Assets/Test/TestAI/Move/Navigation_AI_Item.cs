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
            //������ק,���������קλ��
            if (!AllowDrag) return;

            // ��ȡ�������Ļ�ϵ�λ��
            Vector3 mouseScreenPos = Input.mousePosition;

            // ����������XZƽ�����ƶ���YΪ���嵱ǰ�߶�
            float y = transform.position.y;

            // �������Ļ����ת��Ϊ��������
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
            //���㳯��
            stae.Orientation = transform.ComputeOrientation();

            return stae;
        }

        public void SetStaticStae(StaticStae stae)
        {
            transform.position = stae.Position;
            transform.SetOrientation(stae.Orientation);
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

