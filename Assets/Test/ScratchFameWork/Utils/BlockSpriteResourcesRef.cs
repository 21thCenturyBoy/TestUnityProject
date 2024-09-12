using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    [CreateAssetMenu(menuName = "Scratch/Create BlockSpriteResources")]
    public class BlockSpriteResourcesRef : ScriptableObject
    {
        [Header(nameof(BlockType.condition))] 
        
        public Sprite Condition_EndBody;
        public Sprite Condition_Header;
        public Sprite Condition_MiddleBody;
        public Sprite Condition_MiddleHeader;
        public Sprite Condition_RoundConditional;
        
        [Header(nameof(BlockType.define))] 
        
        public Sprite Define_Header;
        
        [Header(nameof(BlockType.operation))] 
        
        public Sprite Operation_Header;
        
        [Header(nameof(BlockType.simple))] 
        
        public Sprite Simple_Header;
        public Sprite Simple_HeaderGhost;
        
        [Header(nameof(BlockType.trigger))] 
        
        public Sprite Trigger_Header;

        [Header("--------------------------------")] 
        
        [Header("HeaderDataItem Prefab")]
        public GameObject Prefab_Label;
        public GameObject Prefab_Input;
        public GameObject Prefab_VariabelLabel;
        
        [Header("--------------------------------")] 
        
        [Header("Block Color")]
        
       public  Color BlockColor_Undefined;
       public  Color BlockColor_Event; //事件
       public  Color BlockColor_Action; //行为
       public  Color BlockColor_Control; //控制
       public  Color BlockColor_Condition; //条件，与或非
       public  Color BlockColor_GetValue; //取值
       public  Color BlockColor_Variable; //变量


       private static BlockSpriteResourcesRef _instance;
        public static BlockSpriteResourcesRef Instance {
            get {
                if (!_instance) {
                    _instance = Resources.Load<BlockSpriteResourcesRef>("BlockSpriteResourcesRef");
                }
                if (!_instance) {
                    // _instance = CreateDefaultGameState();
                }
                return _instance;
            }
        }
    }
}