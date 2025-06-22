using System.Collections;
using System.Collections.Generic;
using TestAI.Move;
using Unity.VisualScripting;
using UnityEngine;
namespace TestAI
{
    public struct StaticStae
    {
        //λ��
        public Vector3 Position { get; set; }
        //���򣨻��ȣ�
        public float Orientation { get; set; }
    }
    public struct SteeringOutput
    {
        //���ٶ�
        public Vector3 Velocity { get; set; }
        //���ٶ�
        public float Angular { get; set; }
    }

    public interface IKinematicLogic
    {
        void Start();
        void FixedUpdate();
        void Stop();
    }

    public class KinematicLogic : IKinematicLogic
    {
        protected bool m_Inited = false;
        public virtual void FixedUpdate()
        {
            if(!m_Inited) return;
            OnFixedUpdate();
        }

        protected virtual void OnFixedUpdate() { 
        }

        public virtual void Start()
        {
            if (!m_Inited)
            {
                m_Inited = true;
                OnStart();
            }
        }

        protected virtual void OnStart()
        {
        }


        public virtual void Stop()
        {
            if (m_Inited)
            {
                m_Inited = false;
                OnStop();
            }
        }

        protected virtual void OnStop()
        {
        }

    }

    public interface IKinematicEntity
    {
        StaticStae GetStaticStae();
        void SetStaticStae(StaticStae stae);
    }

    public static class UtilsTool
    {
        /// <summary>
        /// ͨ����ǰ������ٶȼ����µķ���
        /// </summary>
        /// <param name="current"></param>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public static float NewOrientation(float current, Vector3 velocity)
        {
            //���㵱ǰ�������ٶȷ���֮��ĽǶȲ�
            if (velocity.sqrMagnitude < Mathf.Epsilon)
            {
                return current; // ����ٶȽӽ��㣬���ֵ�ǰ����
            }
            else
            {
                // ����Ŀ�귽��ĽǶ�
                // Mathf.Atan2 ���ص��ǻ���ֵ����Χ�� [-��, ��] ֮��
                // ����� velocity.x �� velocity.z ����Ϊ Unity �е� Y �������ϣ�X ������ǰ��Z ��������
                float target = Mathf.Atan2(velocity.x, velocity.z);
                //��������ϵ
                //float target = Mathf.Atan2(-velocity.x, velocity.y);
                return target;
            }
        }

        /// <summary>
        /// ���㳯��
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static float ComputeOrientation(this Transform transform)
        {

            // ȡtransform.forward��XZƽ���ϵ�ͶӰ������������ǰ��(Vector3.forward)�ļн�
            Vector3 forward = transform.forward;
            forward.y = 0;
            if (forward.sqrMagnitude > 0.0001f)
            {
                forward.Normalize();
                float angle = Mathf.Atan2(forward.x, forward.z); // ����
                return angle;
            }
            else
            {
                return 0;
            }
        }


        public static void SetOrientation(this Transform transform, float orientation)
        {
            // ������ת��Ϊ�Ƕ�
            float angle = orientation * Mathf.Rad2Deg;
            // ֻ����Y����ת������X��ZΪ0
            transform.rotation = Quaternion.Euler(90, angle, 0);
        }

        /// <summary>
        /// ���������̬״̬(��С30)
        /// </summary>
        /// <returns></returns>
        public static StaticStae CreateRandomStaticStae(Vector2 rangePos)
        {

            float x = rangePos.x;
            float z = rangePos.y;

            //���������ĸ���
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

        public static void Destroy(this IKinematicEntity entity) {
            if (entity!=null)
            {
                Navigation_AI_Item aI_Item =  entity as Navigation_AI_Item;
                GameObject.Destroy(aI_Item.gameObject);
            }
        }

        public static void SetColor(this IKinematicEntity entity,Color color)
        {
            if (entity != null)
            {
                Navigation_AI_Item aI_Item = entity as Navigation_AI_Item;
                aI_Item.GetComponent<SpriteRenderer>().color = color;
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
        /// ת����ΪӦ��
        /// </summary>
        /// <param name="stae"></param>
        /// <param name="steeringOutput"></param>
        public static void SteeringOutputApply(this ref StaticStae stae, SteeringOutput steeringOutput) {
            stae.Position += steeringOutput.Velocity;
            stae.Orientation += steeringOutput.Angular;
        }
    }
}