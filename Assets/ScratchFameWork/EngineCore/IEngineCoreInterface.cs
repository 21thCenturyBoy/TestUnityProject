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

    public enum ScratchValueType : byte
    {
        Boolean = 0,
        Byte = 1,
        Integer = 2,
        FP = 3,
        Vector2 = 4,
        Vector3 = 5,
        EntityRef = 6,
        AssetRef = 7,
    }

    /// <summary>
    /// 这部分应该改成自动生成
    /// </summary>
    public enum ScratchBlockType
    {
        RunTimer,
        OnGameStarted,
        
        
        OnCollisionEnter,
        OnCollisionStay,
        OnCollisionExit,
        OnObjectCreated,
        ApplyForce,
        DestroyObject,
        RepeatAction,
        StartCountdown,
        CompareValues,
        GetCharacterSpeed,
        GetVectorMagnitude,
        
        IntegerValue,
        VectorValue,
        EntityValue,

    }

    public interface IEngineBlockBaseData
    {
        bool TryGetBlockVariableName(out string name);
        public ScratchClassName ClassName { get; }
        public ScratchBlockType Type { get; }
        public int NextBlockGuid { get; set; }
        public IEngineBlockBaseData NextBlock { get; set; }
        public int Guid { get; set; }

        #region Operation or Variable Functions

        public void SetVarsGuid(int Index, int guid);
        public int GetVarGuid(int Index);
        public int GetVariableLength();

        #endregion

        #region Return Value Functions

        public virtual void SetReturnValueGuid(int Index, int guid)
        {
        }

        public virtual int GetReturnValueGuid(int Index)
        {
            return 0;
        }

        public virtual int GetReturnValueLength()
        {
            return 0;
        }

        #endregion
    }

    public interface IEngineBlockTriggerBase : IEngineBlockBaseData
    {
    }

    public interface IEngineBlockConditionBase : IEngineBlockBaseData
    {
        public int TrueBlockGuid { get; set; }
        public int FalseBlockGuid { get; set; }
        public int OperationGuid { get; set; }
    }

    public interface IEngineBlockLoopBase : IEngineBlockBaseData
    {
        public int ChildRootGuid { get; set; }
    }

    public interface IEngineBlockSimpleBase : IEngineBlockBaseData
    {
    }

    public interface IEngineBlockOperationBase : IEngineBlockBaseData
    {
        public int Variable1Guid { get; set; }
        public int Variable2Guid { get; set; }
    }

    public interface IEngineBlockVariableBase : IEngineBlockBaseData
    {
        public string VariableName { get; set; }

        public string GetValueToString();
        public object SetValueToString(string value);
    }

    public interface IEngineCoreInterface
    {
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

        public void ChangeBlockData(Block block, Transform orginParentTrans, Transform newParentTrans);

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
        public List<Block> GenerateBlocks();
    }
}