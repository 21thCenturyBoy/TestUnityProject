using System;
using System.Collections;
using System.Collections.Generic;
using TestAI.Move.Kinematic;
using UnityEngine;

namespace TestAI
{
    #region 编辑器属性

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class AIParam_Float : Attribute
    {
        public String ParmName { get; set; }
        public AIParam_Float(String name = null)
        {
            ParmName = name;
        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AIParam_Info : Attribute
    {
        public String ParmName { get; set; }
        public AIParam_Info(String name = null)
        {
            ParmName = name;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AITest_Button : Attribute
    {
        public String ParmName { get; set; }
        public AITest_Button(String name = null)
        {
            ParmName = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AILogicType : Attribute
    {
        public String ParmName { get; set; }
        public AILogicType(String name = null)
        {
            ParmName = name;
        }
    }


    #endregion

    public static class Utils_Attr
    {
        private static Dictionary<String, System.Type> m_KinematicLogicTypeCache;

        public static Dictionary<String, System.Type> GetKinematicLogicTypeCache()
        {
            if (m_KinematicLogicTypeCache == null)
            {
                m_KinematicLogicTypeCache = new Dictionary<string, System.Type>();
                //获取所有IKinematicLogic类型
                var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    if (typeof(KinematicLogic).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        //获取ILogicType特性
                        var logicTypeAttr = type.GetCustomAttributes(typeof(AILogicType), false);
                        if (logicTypeAttr.Length > 0)
                        {
                            var attr = logicTypeAttr[0] as AILogicType;
                            m_KinematicLogicTypeCache.Add(attr.ParmName, type);
                        }
                        //else
                        //{
                        //    //如果没有ILogicType特性，则使用类名作为逻辑类型名称
                        //    m_KinematicLogicTypeCache.Add(type.Name, type);
                        //}
                    }
                }

            }
            return m_KinematicLogicTypeCache;
        }

        public static KinematicLogic CreateKinematicLogic(String logicTypeName)
        {
            if (GetKinematicLogicTypeCache().TryGetValue(logicTypeName, out System.Type logicType))
            {
                KinematicLogic logic = Activator.CreateInstance(logicType) as KinematicLogic;

                return logic;
            }
            else
            {
                Debug.LogError($"未找到逻辑类型: {logicTypeName}");
                return null;
            }
        }
    }
}