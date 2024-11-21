using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ScratchFramework
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class BlockGuidRefAttribute : System.Attribute
    {
        public BlockGuidRefAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class BlockGuidRefListAttribute : System.Attribute
    {
        public BlockGuidRefListAttribute()
        {
        }
    }

    public interface IEngineBlockBaseData
    {
        public bool IsRoot { get; set; }
        public BVector2 CanvasPos { get; set; }
        public FucType FucType { get; }
        public BlockType BlockType { get; }
        public ScratchBlockType Type { get; }
        public int Guid { get; set; }
    }

    public interface IBlockPlug
    {
        public int NextGuid { get; set; }
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
        public int GetVarGuidsLength();
        public void SetVarsGuid(int Index, int guid);
        public int GetVarGuid(int Index);
        public int[] GetVarGuids();
    }


    public interface IEngineBlockBranch
    {
        /// <summary>
        /// True,False == 1
        /// If ... else ... == 1
        /// if ... else if ... else ... == 2
        /// if ... else if ... else if ... else ... == 3
        /// </summary>
        [BlockGuidRefList]
        public BGuidList BranchOperationBGuids { get; set; }

        /// <summary>
        /// True,False == 2
        /// If ... else ... == 2
        /// if ... else if ... else ... == 3
        /// if ... else if ... else if ... else ... == 4
        /// </summary>
        /// <returns></returns>
        [BlockGuidRefList]
        public BGuidList BranchBlockBGuids { get; set; }
    }

    public interface IEngineBlockTriggerBase : IEngineBlockBaseData, IBlockPlug, IBlockReturnVarGuid
    {
    }

    public interface IEngineBlockConditionBase : IEngineBlockBaseData, IBlockPlug, IEngineBlockBranch
    {
    }

    public interface IEngineBlockLoopBase : IEngineBlockBaseData, IBlockPlug, IBlockVarGuid
    {
        [BlockGuidRef] public int ChildRootGuid { get; set; }
    }

    public interface IEngineBlockSimpleBase : IEngineBlockBaseData, IBlockPlug, IBlockVarGuid
    {
    }

    public interface IEngineBlockOperationBase : IEngineBlockBaseData, IBlockVarGuid
    {
    }

    public interface IEngineBlockVariableBase : IEngineBlockBaseData
    {
        public ScratchValueType ValueType { get; }
        public string VariableName { get; set; }
        public object VariableValue { get; set; }
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
        public void GenerateBlocks(string filepath = null, Action<List<Block>> callback = null);

        public void SaveBlocks(string filepath = null, Action<bool> callback = null);

        /// <summary>
        /// 变量值转字符串
        /// </summary>
        /// <param name="blockBase"></param>
        /// <param name="value"></param>
        public bool VariableValue2String(IEngineBlockVariableBase blockBase, out string value);

        /// <summary>
        /// 字符串转变量值
        /// </summary>
        /// <param name="blockBase"></param>
        /// <param name="value"></param>
        public bool String2VariableValueTo(IEngineBlockVariableBase blockBase, string value);

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