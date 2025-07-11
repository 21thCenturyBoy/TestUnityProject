using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestAI.Move.Kinematic
{
    public class KinematicLogic : IKinematicLogic
    {
        private List<GameObject> m_AI_Pram_Objs = new List<GameObject>();
        protected bool m_Inited = false;

        public const float FixedDeltaTime = 0.02f; // 固定时间步长，20ms

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

                    obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().onEndEdit.AddListener((val) =>
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

    public class SteeringLogic : KinematicLogic {

    }
}

