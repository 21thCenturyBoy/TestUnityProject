using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFamework
{
    public enum Type
    {
        Undefined,
        Event,
        Action,
        Control,//控制
        Condition,//条件，与或非
        GetValue ,//取值
        Variable,//变量
        Operation,//表达式
    }

    /// <summary>Block基类 </summary>
    public abstract class BaseScratchBlock : MonoBehaviour
    {
        
    }
}
