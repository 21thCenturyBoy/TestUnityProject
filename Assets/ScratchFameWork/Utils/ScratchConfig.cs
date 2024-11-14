using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScratchFramework
{
    [CreateAssetMenu(menuName = "Scratch/Create ScratchConfig")]
    public class ScratchConfig : ScriptableObject
    {
        [Header("Sprite资源引用")] 

        [Header(nameof(BlockType.Condition))] public Sprite Condition_EndBody;
        public Sprite Condition_Header;
        public Sprite Condition_MiddleBody;
        public Sprite Condition_MiddleHeader;
        public Sprite Condition_RoundConditional;

        [Header(nameof(BlockType.Define))] public Sprite Define_Header;

        [Header(nameof(BlockType.Operation))] public Sprite Operation_Header;

        [Header(nameof(BlockType.Simple))] public Sprite Simple_Header;
        public Sprite Simple_HeaderGhost;

        [Header(nameof(BlockType.Trigger))] public Sprite Trigger_Header;
        
        [Space]
        [Header("HeaderDataItem Prefab 预制体引用")]
        public GameObject Prefab_Label;

        public GameObject Prefab_Input;
        public GameObject Prefab_VariabelLabel;
        public GameObject Prefab_ReturnVariabelLabel;

        [Space]
        [Header("BlockColor基础颜色配置")] 
        public Color BlockColor_Undefined;

        public Color BlockColor_Event; //事件
        public Color BlockColor_Action; //行为
        public Color BlockColor_Control; //控制
        public Color BlockColor_Condition; //条件，与或非
        public Color BlockColor_GetValue; //取值
        public Color BlockColor_Variable; //变量
        
        [Space]
        [Header("模版配置")] 
        public int Version = 0;
        [Space]
        public string TemplateDatasPath = "Assets/Resources_UGC/Scratch/Prefabs/Resources/TemplateDatas";
        public List<TextAsset> TemplateDatas = new List<TextAsset>();
        
        
        [Space]
        [Header("引擎代码生成配置")] 
        public string CsSharpPath = "Assets/Scripts/ScratchFramework/EngineCore";
        

        private static ScratchConfig _instance;
        public static ScratchConfig Instance
        {
            get
            {
                
                if (!_instance)
                {
                    _instance = Resources.Load<ScratchConfig>("ScratchConfig");
                    // if (_instance == null)
                    // {
                    //     QGResManager.LoadAsync<ScratchConfig>(KoalaUIPaths.ScratchConfig, false, gameObject =>
                    //     {
                    //         if (gameObject == null)
                    //         {
                    //             LogService.LogWarning($"找不到此路径下的资源：{KoalaUIPaths.ScratchConfig}！！！");
                    //         }
                    //         _instance = gameObject;
                    //     });
                    // }
                }

                if (!_instance)
                {
                    // _instance = CreateDefaultGameState();
                }

                return _instance;
            }
        }

        public Color GetFucColor(FucType type)
        {
            switch (type)
            {
                case FucType.Undefined:
                    return BlockColor_Undefined;
                    break;
                case FucType.Event:
                return BlockColor_Event;
                    break;
                case FucType.Action:
                return BlockColor_Action;
                    break;
                case FucType.Control:
                return BlockColor_Control;
                    break;
                case FucType.Condition:
                return BlockColor_Condition;
                    break;
                case FucType.GetValue:
                return BlockColor_GetValue;
                    break;
                case FucType.Variable:
                return BlockColor_Variable;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
