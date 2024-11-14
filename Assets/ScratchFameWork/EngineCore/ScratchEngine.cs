using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{


    public class ScratchEngine : Singleton<ScratchEngine>
    {
        IEngineCoreInterface m_engineCore;
        public IEngineCoreInterface Core => m_engineCore ??= new TestEngineCore();

        /// <summary>
        /// 编辑器模式&运行时下获取默认值
        /// </summary>
        public string GetDefaultType(ScratchValueType type)
        {
            String value = string.Empty;
            switch (type)
            {
                case ScratchValueType.Undefined:
                    break;
                case ScratchValueType.Boolean:
                    value = "false";
                    break;
                case ScratchValueType.Byte:
                    value = "0";
                    break;
                case ScratchValueType.Integer:
                    value = "0";
                    break;
                case ScratchValueType.Float:
                    value = "0.00";
                    break;
                case ScratchValueType.Vector2:
                    value = "(0.00,0.00)";
                    break;
                case ScratchValueType.Vector3:
                    value = "(0.00,0.00,0.00)";
                    break;
                case ScratchValueType.EntityRef:
                    break;
                case ScratchValueType.AssetRef:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return value;
        }

        // /// <summary>
        // /// 编辑器模式&运行时下获取默认值
        // /// </summary>
        // public bool TryConvertType(ScratchValueType type,string inputValue)
        // {
        //     String value = string.Empty;
        //     
        //     Core.
        //
        //     return value;
        // }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}