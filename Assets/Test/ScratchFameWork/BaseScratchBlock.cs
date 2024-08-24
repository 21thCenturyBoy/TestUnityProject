using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public enum Type
    {
        Undefined,
        Event,//事件
        Action,//行为
        Control,//控制
        Condition,//条件，与或非
        GetValue ,//取值
        Variable,//变量
        Operation,//表达式
    }

    /// <summary>Block基类 </summary>
    public abstract class BaseScratchBlock : ScratchUIBehaviour
    {
        
    }
}
