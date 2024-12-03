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
            GlobalCanvas.RefreshDataGuids();
            for (int i = 0; i < Canvas.Count; i++)
            {
                Canvas[i].RefreshDataGuids();
            }
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

        [JsonIgnore] public Dictionary<int, IEngineBlockBaseData> RootBlock => m_RootBlock;
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

        public int[] GetKeys() => m_BlockDataDicts.Keys.ToArray();
    }
}