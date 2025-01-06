using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScratchFramework
{
    [Serializable]
    public class BlockFragmentDataRef : IEngineBlockBaseData, IEngineBlockCanvasData
    {
        public BVector2 CanvasPos { get; set; }
        public bool Enable { get; set; }
        public bool IsRoot { get; set; }
        private int m_Guid;

        public int Guid
        {
            get
            {
                if (DataRef != null)
                {
                    return DataRef.Guid;
                }

                return ScratchUtils.InvalidGuid;
            }
            set
            {
                if (DataRef == null)
                {
                    m_Guid = value;
                }
            }
        }

        public void RefreshRef(EngineBlockCanvas canvas)
        {
            IEngineBlockBaseData baseData = canvas[m_Guid];
            m_Data = baseData;
        }

        public void RefreshRef(IEngineBlockBaseData data)
        {
            m_Data = data;
        }

        [JsonIgnore] private IEngineBlockBaseData m_Data;

        public BlockFragmentDataRef()
        {
        }


        [JsonIgnore] public FucType FucType => m_Data.FucType;
        [JsonIgnore] public BlockType BlockType => m_Data.BlockType;
        [JsonIgnore] public ScratchBlockType Type => m_Data.Type;
        public int[] GetGuids() => m_Data.GetGuids();
        public void RefreshGuids(Dictionary<int, int> map) => m_Data.RefreshGuids(map);

        public int DataRefGuid;

        [JsonIgnore] public IEngineBlockBaseData DataRef => m_Data;
    }


    public static class DataRefDirector
    {
        public enum DataRefType
        {
            None,
            Variable,
        }

        public static BlockFragmentDataRef Create(IEngineBlockBaseData mData)
        {
            BlockFragmentDataRef blockFragmentDataRef = new BlockFragmentDataRef();
            blockFragmentDataRef.DataRefGuid = ScratchUtils.CreateGuid();
            if (mData.IsDataRef())
            {
                var otginRef = mData as BlockFragmentDataRef;
                blockFragmentDataRef.RefreshRef(otginRef.DataRef);
            }
            else
            {
                blockFragmentDataRef.RefreshRef(mData);
            }

            return blockFragmentDataRef;
        }

        public static bool IsDataRef(this IEngineBlockBaseData data)
        {
            if (data is BlockFragmentDataRef)
            {
                return true;
            }

            return false;
        }
        
        public static BlockFragmentDataRef AsDataRef(this IEngineBlockBaseData data, out DataRefType type)
        {
            if (data.FucType == FucType.Variable)
            {
                type = DataRefType.Variable;
            }
            else
            {
                type = DataRefType.None;
            }
            
            return data as BlockFragmentDataRef;
        }
    }
}