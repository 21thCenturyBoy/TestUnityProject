using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestAI.Move.Kinematic
{
    public class Kinematic
    {
        //位置
        public Vector3 Position { get; set; }
        //方向（弧度）
        public float Orientation { get; set; }
        //线速度
        public Vector3 Velocity { get; set; }
        //角速度（弧度每秒）
        public float Rotation { get; set; }

        /// <summary>
        /// 手动更新方法(帧率低)，由于帧率低，可能会导致物体跳跃式移动。
        /// </summary>
        /// <param name="steering"></param>
        /// <param name="deltaTime"></param>
        public void ForceUpdate(SteeringOutputVelocity steering, float deltaTime)
        {
            //加速度与位移公式：(vt²-v0²)=2as，s=v0t+at²/2，s2-s1=aT²。
            //更新位置
            float half_t = 0.5f * deltaTime * deltaTime;
            Position += Velocity * deltaTime + steering.Line * half_t;
            //更新方向
            Orientation += Rotation * deltaTime + steering.Angular * half_t;

            //更新线速度
            Velocity += steering.Line * deltaTime;
            //更新角速度
            Rotation += steering.Angular * deltaTime;
        }


        public void FixedUpdate(SteeringOutputVelocity steering, float deltaTime)
        {
            //更新位置
            Position += Velocity * deltaTime;
            //更新方向
            Orientation += Rotation * deltaTime;

            //更新线速度
            Velocity += steering.Line * deltaTime;
            //更新角速度
            Rotation += steering.Angular * deltaTime;
        }

        public void FixedUpdate(SteeringOutputVelocity steering, float maxSpeed, float deltaTime)
        {
            //更新位置
            Position += Velocity * deltaTime;
            //更新方向
            Orientation += Rotation * deltaTime;

            //更新线速度
            Velocity += steering.Line * deltaTime;
            //更新角速度
            Rotation += steering.Angular * deltaTime;

            //检查速度是否超过最大速度
            if (Velocity.magnitude > maxSpeed)
            {
                //如果超过最大速度，则归一化并乘以最大速度
                Velocity = Velocity.normalized * maxSpeed;
            }
        }
    }

    public class KinematicLogic : IKinematicLogic
    {
        private List<GameObject> m_AI_Pram_Objs = new List<GameObject>();
        protected bool m_Inited = false;
        public virtual void FixedUpdate()
        {
            if (!m_Inited) return;
            OnFixedUpdate();
        }

        protected virtual void OnFixedUpdate()
        {
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
                for (int i = 0; i < m_AI_Pram_Objs.Count; i++)
                {
                    GameObject.Destroy(m_AI_Pram_Objs[i]);
                }
                m_AI_Pram_Objs.Clear();
                OnStop();
            }
        }

        protected virtual void OnStop()
        {
        }

        public void CreatAIPramUI(Transform parentTrans)
        {
            // 反射获取所有带有 AIParm_Float 特性的字段或属性
            var type = GetType();
            // 获取所有字段
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
                    obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().text = field.GetValue(this).ToString();

                    obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().onValueChanged.AddListener((val) =>
                    {
                        float val_float = float.Parse(val.ToString());
                        field.SetValue(this, val_float);
                    });
                    m_AI_Pram_Objs.Add(obj);
                }
            }
            // 获取所有属性
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
                    obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().text = prop.GetValue(this).ToString();

                    obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().onValueChanged.AddListener((val) =>
                    {
                        float val_float = float.Parse(val.ToString());
                        prop.SetValue(this, val_float);
                    });
                    m_AI_Pram_Objs.Add(obj);
                }
            }
        }

    }
}

