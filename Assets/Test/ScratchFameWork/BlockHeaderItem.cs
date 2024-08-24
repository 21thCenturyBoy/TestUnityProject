using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    [Serializable]
    public enum ParamType
    {
        Undefined,
        Label,
        InputField,
        Dropdown,
        Operation,
        Toggle,
        Variable
    }

    [Serializable]
    public class BlockHeaderParam_Data : ScratchVMData
    {
    }


    public class BlockHeaderParam : ScratchUIBehaviour<BlockHeaderParam_Data>
    {
    }

    public abstract class BlockHeaderItem<T> : ScratchUIBehaviour<T>, IScratchSerializeData where T : ScratchVMData, new()
    {
    }
}