using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ScratchFramework
{
    [Serializable]
    public partial class EngineBlockCanvasGroup
    {
        public static string DefaultName = "New Canvas";

        public EngineBlockCanvas GlobalCanvas { get; set; }
        public List<EngineBlockCanvas> Canvas { get; set; } = new List<EngineBlockCanvas>();

        public static EngineBlockCanvas CreateNewCanvas(string name = null)
        {
            EngineBlockCanvas canvas = new EngineBlockCanvas();

            if (string.IsNullOrEmpty(name))
            {
                canvas.Name = DefaultName;
            }
            else
            {
                canvas.Name = name;
            }

            return canvas;
        }

        public void RefreshDataGuids()
        {
            GlobalCanvas.GetRefData();
            for (int i = 0; i < Canvas.Count; i++)
            {
                Canvas[i].GetRefData();
            }

            GlobalCanvas.RefreshDataGuids();
            for (int i = 0; i < Canvas.Count; i++)
            {
                Canvas[i].RefreshDataGuids();
            }
        }

        public bool ContainGuids(int guid)
        {
            if (GlobalCanvas.ContainGuids(guid)) return true;
            for (int i = 0; i < Canvas.Count; i++)
            {
                if (Canvas[i].ContainGuids(guid)) return true;
            }

            return false;
        }
    }


    public class EngineCanvasRefData
    {
        public bool IsRoot { get; set; }
        public BVector2 CanvasPos { get; set; }
        public bool Enable { get; set; }
        public int GuidRef { get; set; }
    }

    [Serializable]
    public class EngineVariableRef : IEngineBlockBaseDataRef
    {
        [JsonIgnore] private IEngineBlockBaseData m_Data;

        public EngineVariableRef()
        {
        }

        public EngineVariableRef(IEngineBlockBaseData mData)
        {
            m_Data = mData;
            GuidRef = m_Data.Guid;
        }

        public bool IsRoot { get; set; }
        public BVector2 CanvasPos { get; set; }
        public bool Enable { get; set; }
        [JsonIgnore]
        public Guid UIGuid { get; set; }
        public int GuidRef { get; set; }
        [JsonIgnore] public FucType FucType => m_Data.FucType;
        [JsonIgnore] public BlockType BlockType => m_Data.BlockType;
        [JsonIgnore] public ScratchBlockType Type => m_Data.Type;
        public int[] GetGuids() => m_Data.GetGuids();
        public void RefreshGuids(Dictionary<int, int> map) => m_Data.RefreshGuids(map);

        [JsonIgnore]
        public int Guid
        {
            get => m_Data.Guid;
            set => m_Data.Guid = value;
        }


        public void RefreshRef(IEngineBlockBaseData mData)
        {
            m_Data = mData;
            GuidRef = m_Data.Guid;
        }
    }

    [Serializable]
    public partial class EngineBlockCanvas
    {
        private string m_Name = string.Empty;

        /// <summary> [Editor Data]Name </summary>
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        private Dictionary<int, IEngineBlockBaseData> m_RootBlock = new Dictionary<int, IEngineBlockBaseData>();

        private Dictionary<int, IEngineBlockBaseData> m_BlockDataDicts = new Dictionary<int, IEngineBlockBaseData>();

        private List<EngineVariableRef> m_VariableRefs = new List<EngineVariableRef>();
        [JsonIgnore] public Dictionary<int, IEngineBlockBaseData> RootBlock => m_RootBlock;
        public List<EngineVariableRef> VariableRefs => m_VariableRefs;
        public Dictionary<int, IEngineBlockBaseData> BlockDataDicts => m_BlockDataDicts;

        public void RefreshDataGuids()
        {
            var list = m_BlockDataDicts.Values.ToList();
            RootBlock.Clear();
            m_BlockDataDicts.Clear();
            ScratchUtils.RefreshDataGuids(list);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Guid == ScratchUtils.InvalidGuid)
                {
                    continue;
                }

                m_BlockDataDicts[list[i].Guid] = list[i];
                if (list[i].IsRoot)
                {
                    m_RootBlock[list[i].Guid] = list[i];
                }
            }

            ScratchUtils.SetDirtyType(BlocksDataDirtyType.Refresh);
        }

        public void GetRefData()
        {
            for (int i = 0; i < VariableRefs.Count; i++)
            {
                VariableRefs[i].RefreshRef(this[VariableRefs[i].GuidRef]);
            }
        }

        public void DrawCanvas()
        {
            HashSet<Block> blocks = new HashSet<Block>();

            foreach (var rootData in m_RootBlock.Values)
            {
                var blockview = ScratchUtils.DrawNodeRoot(rootData, BlockCanvasManager.Instance.RectTrans, -1);
                for (int j = 0; j < blockview.Count; j++)
                {
                    blocks.Add(blockview[j]);
                }
            }

            ScratchUtils.FixedBindOperation(blocks);
        }

        public bool AddBlocksData(IEngineBlockBaseData data)
        {
            if (m_BlockDataDicts.ContainsKey(data.Guid))
            {
                return false;
            }

            if (data.IsRoot)
            {
                RootBlock[data.Guid] = data;
            }

            m_BlockDataDicts[data.Guid] = data;

            ScratchUtils.SetDirtyType(BlocksDataDirtyType.Add);
            return true;
        }

        public bool RemoveBlocksData(IEngineBlockBaseData data)
        {
            if (!m_BlockDataDicts.ContainsKey(data.Guid))
            {
                return false;
            }

            RootBlock.Remove(data.Guid);
            if (m_BlockDataDicts.Remove(data.Guid))
            {
                ScratchUtils.SetDirtyType(BlocksDataDirtyType.Remove);
                return true;
            }

            return false;
        }

        public IEngineBlockBaseData[] SelectBlockDatas(Func<bool> func)
        {
            return m_BlockDataDicts.Values.Where(data => func()).ToArray();
        }

        public T[] SelectBlockDatas<T>()
        {
            return m_BlockDataDicts.Values.OfType<T>().ToArray();
        }

        public bool TryGetDataRef(int guid, out IEngineBlockBaseData data)
        {
            return m_BlockDataDicts.TryGetValue(guid, out data);
        }

        public IEngineBlockBaseData this[int param]
        {
            get
            {
                if (m_BlockDataDicts.TryGetValue(param, out var data))
                {
                    return data;
                }

                if (ScratchEngine.Instance.CurrentGroup.GlobalCanvas.m_BlockDataDicts.TryGetValue(param, out data))
                {
                    return data;
                }

                return null;
            }
        }

        public bool ContainGuids(int guid)
        {
            return m_BlockDataDicts.ContainsKey(guid);
        }

        public int[] GetKeys() => m_BlockDataDicts.Keys.ToArray();
    }
}