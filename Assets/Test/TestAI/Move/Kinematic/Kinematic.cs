using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestAI.Move.Kinematic
{
    public abstract class KinematicLogic : IKinematicLogic
    {
        private List<GameObject> m_AI_Pram_Objs = new List<GameObject>();
        private Dictionary<FieldInfo, GameObject> m_AI_Info_Objs = new Dictionary<FieldInfo, GameObject>();
        protected bool m_Inited = false;

        public const float FixedDeltaTime = 0.02f; // 固定时间步长，20ms

        public virtual void FixedUpdate()
        {
            foreach(var item in m_AI_Info_Objs)
            {
                var field = item.Key;
                var obj = item.Value;
                if (field.FieldType == typeof(float))
                {
                    obj.transform.Find("ValueLabel").GetComponent<TMPro.TMP_Text>().text = field.GetValue(this).ToString();
                }
                else if (field.FieldType == typeof(int))
                {
                    obj.transform.Find("ValueLabel").GetComponent<TMPro.TMP_Text>().text = field.GetValue(this).ToString();
                }
            }

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
                foreach (var item in m_AI_Info_Objs)
                {
                    GameObject.Destroy(item.Value.gameObject);
                }
                m_AI_Pram_Objs.Clear();
                m_AI_Info_Objs.Clear();
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
            GameObject AIParam_Float_prefab = Resources.Load<GameObject>("AIParam_Float");
            GameObject AIParam_Info_prefab = Resources.Load<GameObject>("AIParam_Info");
            GameObject AITest_Button_prefab = Resources.Load<GameObject>("AITest_Button");
            var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(AIParam_Float)))
                {
                    GameObject obj = GameObject.Instantiate(AIParam_Float_prefab);
                    obj.transform.SetParent(parentTrans);
                    RectTransform rect = obj.GetComponent<RectTransform>();
                    rect.localScale = Vector3.one;
                    rect.localRotation = Quaternion.identity;

                    var AIParm_Attr = field.GetCustomAttribute<AIParam_Float>(false);
                    if (string.IsNullOrEmpty(AIParm_Attr.ParmName))
                    {
                        obj.transform.Find("Label").GetComponent<TMPro.TMP_Text>().text = field.Name;
                    }
                    else
                    {
                        obj.transform.Find("Label").GetComponent<TMPro.TMP_Text>().text = AIParm_Attr.ParmName;
                    }

                    obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().text = field.GetValue(this).ToString();

                    obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().onEndEdit.AddListener((val) =>
                    {
                        float val_float = float.Parse(val.ToString());
                        field.SetValue(this, val_float);
                    });
                    m_AI_Pram_Objs.Add(obj);
                }

                if (Attribute.IsDefined(field, typeof(AIParam_Info)))
                {
                    GameObject obj = GameObject.Instantiate(AIParam_Info_prefab);
                    obj.transform.SetParent(parentTrans);
                    RectTransform rect = obj.GetComponent<RectTransform>();
                    rect.localScale = Vector3.one;
                    rect.localRotation = Quaternion.identity;

                    var AIParm_Attr = field.GetCustomAttribute<AIParam_Info>(false);
                    if (string.IsNullOrEmpty(AIParm_Attr.ParmName))
                    {
                        obj.transform.Find("NameLabel").GetComponent<TMPro.TMP_Text>().text = field.Name;
                    }
                    else
                    {
                        obj.transform.Find("NameLabel").GetComponent<TMPro.TMP_Text>().text = AIParm_Attr.ParmName;
                    }

                    m_AI_Info_Objs[field] = obj;
                }
            }
            // 获取所有属性
            var properties = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var prop in properties)
            {
                if (Attribute.IsDefined(prop, typeof(AIParam_Float)) && prop.CanRead)
                {
                    GameObject obj = GameObject.Instantiate(AIParam_Float_prefab);
                    obj.transform.SetParent(parentTrans);
                    RectTransform rect = obj.GetComponent<RectTransform>();
                    rect.localScale = Vector3.one;
                    rect.localRotation = Quaternion.identity;

                    var AIParm_Attr = prop.GetCustomAttribute<AIParam_Float>(false);
                    if (string.IsNullOrEmpty(AIParm_Attr.ParmName))
                    {
                        obj.transform.Find("Label").GetComponent<TMPro.TMP_Text>().text = prop.Name;
                    }
                    else
                    {
                        obj.transform.Find("Label").GetComponent<TMPro.TMP_Text>().text = AIParm_Attr.ParmName;
                    }

                    obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().text = prop.GetValue(this).ToString();

                    obj.transform.Find("InputValue").GetComponent<TMPro.TMP_InputField>().onValueChanged.AddListener((val) =>
                    {
                        float val_float = float.Parse(val.ToString());
                        prop.SetValue(this, val_float);
                    });
                    m_AI_Pram_Objs.Add(obj);
                }
            }

            var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if (Attribute.IsDefined(method, typeof(AITest_Button)) && method.GetParameters().Length == 0 && method.ReturnType == typeof(void))
                {
                    GameObject obj = GameObject.Instantiate(AITest_Button_prefab);
                    obj.transform.SetParent(parentTrans);
                    RectTransform rect = obj.GetComponent<RectTransform>();
                    rect.localScale = Vector3.one;
                    rect.localRotation = Quaternion.identity;
                    var AITest_Attr = method.GetCustomAttribute<AITest_Button>(false);
                    if (string.IsNullOrEmpty(AITest_Attr.ParmName))
                    {
                        obj.transform.Find("Label").GetComponent<TMPro.TMP_Text>().text = method.Name;
                    }
                    else
                    {
                        obj.transform.Find("Label").GetComponent<TMPro.TMP_Text>().text = AITest_Attr.ParmName;
                    }

                    obj.transform.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        method.Invoke(this, null);
                    });
                    m_AI_Pram_Objs.Add(obj);
                }
            }
        }


    }
    public abstract class SteeringLogic : KinematicLogic
    {
        public abstract SteeringOutput GetSteeringOut();
    }
}

