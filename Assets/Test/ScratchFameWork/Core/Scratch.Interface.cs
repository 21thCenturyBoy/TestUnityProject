using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public interface IScratchLayout
    {
        Vector2 GetSize();
        void OnUpdateLayout();
    }

    public interface IScratchModifyLayout
    {
        void UpdateLayout();
        Vector2 SetSize(Vector2 size);
    }

    public interface IScratchManager
    {
        bool Initialize();
        bool Active { get; set; }
        void OnUpdate();
        void OnLateUpdate();
        bool Clear();
    }

    public interface IBlockData : IScratchData
    {
        BlockType Type { get; set; }
        int Version { get; }
        IBlockSectionData[] SectionTreeList { get; }
        void CopyData(Block block);
        Block CreateBlock();
    }

    public interface IBlockSectionData : IScratchData
    {
        Dictionary<int, int> OperationRefDict { get; }
        IBlockHeadData[] BlockHeadTreeList { get; }
        IBlockData[] BlockTreeList { get; }
    }

    public interface IBlockHeadData : IScratchData
    {
        DataType DataType { get; }

        IBlockData GetBlockData();
    }

    public interface IScratchData
    {
        byte[] Serialize();
        bool Deserialize(MemoryStream stream, int version = -1);
    }

    public interface IScratchRefreshRef
    {
        void RefreshRef(Dictionary<int, int> refreshDic);
    }

    public interface IScratchVMDataRef
    {
        System.Type RefType { get; }

        int RefIdPtr { get; }
    }

    public interface IBlockScratch { }
    
    public interface IBlockScratch_Head:IBlockScratch
    {
        IBlockHeadData CopyData();
        IBlockHeadData DataRef();
        void SetData(IBlockHeadData data);
        void RefreshUI();
    }
    public interface IBlockScratch_Section:IBlockScratch
    {
        IBlockSectionData CopyData();
    }
    public interface IBlockScratch_Block:IBlockScratch
    {
        IBlockData CopyData();
    }

    public interface IScratchBlockClick : IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
    }

    public interface IScratchBlockDrag : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
    }
}