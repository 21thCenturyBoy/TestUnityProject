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
        void GetData(Block block);
    }

    public interface IBlockSectionData : IScratchData
    {
        IBlockHeadData[] BlockHeadTreeList { get; set; }
        IBlockData[] BlockTreeList { get; set; }
        IBlockScratch_Block[] BlockTreeRefList { get; set; }
    }

    public interface IBlockHeadData : IScratchData
    {
        DataType DataType { get; }
        
        bool Deserialize(MemoryStream stream, int version = -1);
    }

    public interface IScratchData
    {
        byte[] Serialize();
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

    public interface IBlockScratch
    {
    }

    public interface IBlockScratch_Head : IBlockScratch
    {
        IBlockHeadData CopyData();
        IBlockHeadData DataRef();
        void SetData(IBlockHeadData data);
        void RefreshUI();
    }

    public interface IBlockLanguage
    {
        void SetLanguageId(int id);
    }
    public interface IBlockScratch_Section : IBlockScratch
    {
        IBlockSectionData GetData();
    }
    public interface IScratchSectionChild
    {
        IBlockScratch_Section GetSection();
    }

    public interface IBlockScratch_Block : IBlockScratch
    {
        IBlockData GetDataRef();
    }

    public interface IScratchBlockClick : IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
    }

    public interface IScratchBlockDrag : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
    }
}