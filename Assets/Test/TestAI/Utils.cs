using System;
using System.Collections;
using System.Collections.Generic;
using TestAI.Move;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
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
        public List<GameObject> AI_Pram_Objs = new List<GameObject>();
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
                for (int i = 0; i < AI_Pram_Objs.Count; i++) {
                    GameObject.Destroy(AI_Pram_Objs[i]);
                }
                AI_Pram_Objs.Clear();
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

    public class AIParm_Float : Attribute {}

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

        /// <summary>
        /// ����Transform���򣨻��ȣ�
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="orientation"></param>
        public static void SetOrientation(this Transform transform, float orientation)
        {
            // ������ת��Ϊ�Ƕ�
            float angle = orientation * Mathf.Rad2Deg;
            // ֻ����Y����ת������X��ZΪ0
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        /// <summary>
        /// ���򣨻��ȣ�ת����
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="orientation"></param>
        public static Vector3 OrientationToVector(this StaticStae stae)
        {
            //Y��Ϊ0��XZƽ���ϼ��㷽������
            float x = Mathf.Sin(stae.Orientation);
            float z = Mathf.Cos(stae.Orientation);
            return new Vector3(x, 0, z);
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
                aI_Item.GetComponentInChildren<SpriteRenderer>().color = color;
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
        /// ת�����Ӧ��
        /// </summary>
        /// <param name="stae"></param>
        /// <param name="steeringOutput"></param>
        public static void SteeringOutputApply(this ref StaticStae stae, SteeringOutput steeringOutput) {
            // Ӧ���µ��ٶ�������λ��
            stae.Position += steeringOutput.Velocity;
            // ���³���
            stae.Orientation += steeringOutput.Angular;
        }


        public static void CreatAIPramUI(this KinematicLogic logic,Transform parentTrans)
        {
            // �����ȡ���д��� AIParm_Float ���Ե��ֶλ�����
            if (logic != null)
            {
                var type = logic.GetType();
                // ��ȡ�����ֶ�
                GameObject AIParm_Float_prefab = Resources.Load<GameObject>("AIParm_Float");
                var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    if (Attribute.IsDefined(field, typeof(AIParm_Float)))
                    {
                        GameObject obj = GameObject.Instantiate(AIParm_Float_prefab);
                        obj.transform.SetParent(parentTrans);
                        RectTransform rect = obj.GetComponent<RectTransform>();
                        rect.localScale = Vector3.one;
                        rect.localRotation = Quaternion.identity;

                        obj.transform.Find("Label").GetComponent<TMPro.TMP_Text>().text = field.Name;
                        obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().text = field.GetValue(logic).ToString();

                        obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().onValueChanged.AddListener((val) => {
                            float val_float  = float.Parse(val.ToString());
                            field.SetValue(logic, val_float);
                        });
                        logic.AI_Pram_Objs.Add(obj);
                    }
                }
                // ��ȡ��������
                var properties = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                foreach (var prop in properties)
                {
                    if (Attribute.IsDefined(prop, typeof(AIParm_Float)) && prop.CanRead)
                    {
                        GameObject obj = GameObject.Instantiate(AIParm_Float_prefab);
                        obj.transform.SetParent(parentTrans);
                        RectTransform rect = obj.GetComponent<RectTransform>();
                        rect.localScale = Vector3.one;
                        rect.localRotation = Quaternion.identity;

                        obj.transform.Find("Label").GetComponent<TMPro.TMP_Text>().text = prop.Name;
                        obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().text = prop.GetValue(logic).ToString();

                        obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().onValueChanged.AddListener((val) => {
                            float val_float = float.Parse(val.ToString());
                            prop.SetValue(logic, val_float);
                        });
                        logic.AI_Pram_Objs.Add(obj);
                    }
                }
            }
        }
    }
}