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

    public static class IEngineCoreInterfaceExtension
    {
        public static IEngineBlockBaseData CopyData(this IEngineBlockBaseData data,ref IEngineBlockBaseData target)
        {
            //EditorData
            target.IsRoot = data.IsRoot;
            target.CanvasPos = data.CanvasPos;

            var orginGuids = data.GetGuids();
            var targetGuids = target.GetGuids();
            for (int i = 0; i < orginGuids.Length; i++)
            {
                targetGuids[i] = orginGuids[i];
            }

            if (data is IBlockPlug blockPlug && target is IBlockPlug targetPlug)
            {
                targetPlug.NextGuid = blockPlug.NextGuid;
            }
            
            if (data is IBlockReturnVarGuid blockReturnVarGuid && target is IBlockReturnVarGuid targetReturnVarGuid)
            {
                for (int i = 0; i < blockReturnVarGuid.GetReturnValuesLength(); i++)
                {
                    targetReturnVarGuid.SetReturnValueGuid(i, blockReturnVarGuid.GetReturnValueGuid(i));
                }
            }
            
            if (data is IBlockVarGuid blockVarGuid && target is IBlockVarGuid targetVarGuid)
            {
                for (int i = 0; i < blockVarGuid.GetVarGuidsLength(); i++)
                {
                    targetVarGuid.SetVarsGuid(i, blockVarGuid.GetVarGuid(i));
                }
            }
            
            if (data is IEngineBlockBranch blockBranch && target is IEngineBlockBranch targetBranch)
            {
                targetBranch.BranchOperationBGuids = new BGuidList(blockBranch.BranchOperationBGuids.ToList());
                targetBranch.BranchBlockBGuids = new BGuidList(blockBranch.BranchBlockBGuids.ToList());
            }

            if (data is IEngineBlockTriggerBase blockTriggerBase && target is IEngineBlockTriggerBase targetTriggerBase)
            {
                
            }
            
            if (data is IEngineBlockConditionBase blockConditionBase && target is IEngineBlockConditionBase targetConditionBase)
            {
                
            }
            
            if (data is IEngineBlockLoopBase blockLoopBase && target is IEngineBlockLoopBase targetLoopBase)
            {
                targetLoopBase.ChildRootGuid = blockLoopBase.ChildRootGuid;
            }
            
            if (data is IEngineBlockSimpleBase blockSimpleBase && target is IEngineBlockSimpleBase targetSimpleBase)
            {
                
            }
            
            if (data is IEngineBlockOperationBase blockOperationBase && target is IEngineBlockOperationBase targetOperationBase)
            {
                
            }
            
            if (data is IEngineBlockVariableBase blockVariableBase && target is IEngineBlockVariableBase targetVariableBase)
            {
                targetVariableBase.VariableName = blockVariableBase.VariableName;
                targetVariableBase.VariableValue = blockVariableBase.VariableValue;
                targetVariableBase.ReturnParentGuid = blockVariableBase.ReturnParentGuid;
            }

            return target;
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
        public BGuid[] GetGuids();
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
        [BlockGuidRef] public int ReturnParentGuid { get; set; }
    }

    public interface IEngineCoreInterface
    {
        string GetEngineVersion();

        /// <summary>
        /// 获取所有引擎Block数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, IEngineBlockBaseData> GetAllBlocksRef();


        /// <summary>
        /// 查询引擎Block数据
        /// </summary>
        /// <returns></returns>
        public IEngineBlockBaseData GetBlocksDataRef(int guid);

        /// <summary>
        /// 创建Block数据
        /// </summary>
        /// <returns></returns>
        public bool CreateBlocksData(IEngineBlockBaseData data);
        
        public bool ClearBlocksData(IEngineBlockBaseData data);

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
    }
}