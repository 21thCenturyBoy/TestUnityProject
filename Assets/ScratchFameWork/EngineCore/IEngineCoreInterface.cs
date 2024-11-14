using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public enum ScratchClassName : byte
    {
        Trigger = 0,
        Simple = 1,
        Loop = 2,
        Condition = 3,
        Operation = 4,
        Values = 5,
    }

    /// <summary>
    /// 这部分应该改成自动生成
    /// </summary>
    public enum ScratchValueType : byte
    {
        Undefined = 0,
        Boolean = 1,
        Byte = 2,
        Integer = 3,
        Float = 4,
        Vector2 = 5,
        Vector3 = 6,
        EntityRef = 7,
        AssetRef = 8,
    }

    /// <summary>
    /// 这部分应该改成自动生成
    /// </summary>
    public enum ScratchBlockType : int
    {
        __Undefined__ = 0,
        __Event__ = 10000, //事件
        OnCollisionEnter,
        OnCollisionStay,
        OnCollisionExit,
        OnObjectCreated,
        //TODO Event

        __Action__ = 20000, //行为
        ApplyForce,
        DestroyObject,
        //TODO Action

        __Control__ = 30000, //控制
        IfElse,
        RepeatAction,
        StartCountdown,
        //TODO Control

        __Condition__ = 40000, //条件，与或非
        CompareValues,
        //TODO Condition

        __GetValue__ = 50000, //取值
        GetCharacterSpeed,
        GetVectorMagnitude,
        //TODO GetValue

        __Variable__ = 60000, //变量
        IntegerValue,
        VectorValue,
        EntityValue,
        //TODO Variable
    }

    public interface IEngineBlockBaseData
    {
        public Vector3 CanvasPos { get; set; }
        public ScratchClassName ClassName { get; }
        public ScratchBlockType Type { get; }
        public int NextBlockGuid { get; set; }
        [Newtonsoft.Json.JsonIgnore] public IEngineBlockBaseData NextBlock { get; set; }
        public int Guid { get; set; }
    }

    public interface IBlockReturnVarGuid
    {
        public void SetReturnValueGuid(int Index, int guid);
        public int GetReturnValueGuid(int Index);
        public int GetReturnValuesLength();
        public int[] GetReturnValues();
    }

    public interface IBlockVarGuid
    {
        public void SetInputValues(int index, string value);
        public string GetInputValue(int index);
        public string[] GetInputValues();

        public int GetVarGuidsLength();
        public void SetVarsGuid(int Index, int guid);
        public int GetVarGuid(int Index);
        public int[] GetVarGuids();
    }

    public interface IEngineBlockTriggerBase : IEngineBlockBaseData, IBlockReturnVarGuid
    {
    }

    public interface IEngineBlockConditionBase : IEngineBlockBaseData, IBlockVarGuid
    {
        public int TrueBlockGuid { get; set; }
        public int FalseBlockGuid { get; set; }
        public int OperationGuid { get; set; }
    }

    public interface IEngineBlockLoopBase : IEngineBlockBaseData, IBlockVarGuid
    {
        public int ChildRootGuid { get; set; }
    }

    public interface IEngineBlockSimpleBase : IEngineBlockBaseData, IBlockVarGuid
    {
    }

    public interface IEngineBlockOperationBase : IEngineBlockBaseData, IBlockVarGuid
    {
        public int Variable1Guid { get; set; }
        public int Variable2Guid { get; set; }
    }

    public interface IEngineBlockVariableBase : IEngineBlockBaseData, IBlockVarGuid
    {
        bool TryGetBlockVariableName(out string name);
        public string VariableName { get; set; }

        public string GetValueToString();
        public object SetValueToString(string value);
    }

    public interface IEngineCoreInterface
    {
        string GetEngineVersion();

        /// <summary>
        /// 获取所有引擎Block数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, IEngineBlockBaseData> GetAllBlocks();

        /// <summary>
        /// 查询引擎Block数据
        /// </summary>
        /// <returns></returns>
        public IEngineBlockBaseData GetBlocksDataRef(int guid);

        public bool ChangeBlockData(Block block, Transform orginParentTrans, Transform newParentTrans);

        /// <summary>
        /// 修复并设置BlockData 位置
        /// </summary>
        public bool TryFixedBlockBaseDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos);

        /// <summary>
        /// 更新BlockData 位置
        /// </summary>
        public bool UpdateDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos);

        /// <summary>
        /// 删除DeleteBlock
        /// </summary>
        public void DeleteBlock(IEngineBlockBaseData block, bool recursion = true);

        /// <summary>
        /// 根据类型创建方块
        /// </summary>
        /// <param name="scratchType"></param>
        /// <param name="isAdd">是否是新创建  （生成新的guid并添加如block池子）</param>
        /// <returns></returns>
        public IEngineBlockBaseData CreateBlock(ScratchBlockType scratchType, bool isAdd = true);

        /// <summary>
        /// 遍历方块
        /// </summary>
        /// <param name="rootGuid"></param>
        /// <param name="function"></param>
        public IEngineBlockBaseData FindPreBlock(int rootGuid, int CurGuid);

        /// <summary>
        /// 生成Block
        /// </summary>
        /// <returns></returns>
        public List<Block> GenerateBlocks(string filepath = null);
        
        public void SaveBlocks(string filepath = null,Action<bool> callback = null);

#if UNITY_EDITOR

        /// <summary>
        /// 编辑器创建Block文件
        /// </summary>
        /// <param name="blockCreateName"></param>
        /// <returns></returns>
        public bool BlockCreateCSFile(string blockCreateName);
#endif
    }
}