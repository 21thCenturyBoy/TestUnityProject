using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ScratchFramework
{
    [Serializable]
    public partial class EngineBlockFileData
    {
        public static string VERSION = "0.0.0.241219";

        public string Version;

        public EngineBlockCanvasGroup Global = null;
        public List<EngineBlockCanvasGroup> CanvasGroups = new List<EngineBlockCanvasGroup>();

        public void CreateGlobal()
        {
            Global = CreateNewGroup(nameof(Global));
        }

        public void RefreshRef()
        {
            for (int i = 0; i < Global.Canvas.Count; i++)
            {
                Global.Canvas[i].RefreshRef();
            }

            for (int i = 0; i < CanvasGroups.Count; i++)
            {
                for (int j = 0; j < CanvasGroups[i].Canvas.Count; j++)
                {
                    CanvasGroups[i].Canvas[j].RefreshRef();
                }
            }
        }

        public void RefreshDataGuids()
        {
            Global.RefreshDataGuids();

            for (int i = 0; i < CanvasGroups.Count; i++)
            {
                CanvasGroups[i].RefreshDataGuids();
            }
        }

        public bool ContainGuids(int guid)
        {
            if (Global.ContainGuids(guid)) return true;
            for (int i = 0; i < CanvasGroups.Count; i++)
            {
                if (CanvasGroups[i].ContainGuids(guid)) return true;
            }

            return false;
        }

        public bool RemoveAllFragmentDataRef(IEngineBlockBaseData data)
        {
            bool res = true;
            foreach (var canva in Global.Canvas)
            {
                res = canva.RemoveFragmentDataRef(data, out var removeArrays) && res;
            }

            foreach (var group in CanvasGroups)
            {
                foreach (var canva in group.Canvas)
                {
                    res = canva.RemoveFragmentDataRef(data, out var removeArrays) && res;
                }
            }

            return res;
        }

        public static EngineBlockCanvasGroup CreateNewGroup(string name = null)
        {
            EngineBlockCanvasGroup group = new EngineBlockCanvasGroup();

            if (string.IsNullOrEmpty(name))
            {
                group.Name = EngineBlockCanvasGroup.DefaultName;
            }
            else
            {
                group.Name = name;
            }

            group.Canvas.Add(CreateNewCanvas());
            return group;
        }

        public static EngineBlockCanvas CreateNewCanvas(string name = null)
        {
            EngineBlockCanvas canvas = new EngineBlockCanvas();

            if (string.IsNullOrEmpty(name))
            {
                canvas.Name = EngineBlockCanvas.DefaultName;
            }
            else
            {
                canvas.Name = name;
            }

            return canvas;
        }
    }

    [Serializable]
    public partial class EngineBlockCanvasGroup
    {
        public const string DefaultName = "New CanvasGroup";

        public EngineBlockCanvasGroup()
        {
            Name = DefaultName;
        }

        public EngineBlockCanvasGroup(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = DefaultName;
            }

            Name = DefaultName;
        }

        public string Name { get; set; }
        public List<EngineBlockCanvas> Canvas { get; set; } = new List<EngineBlockCanvas>();


        internal void RefreshDataGuids()
        {
            for (int i = 0; i < Canvas.Count; i++)
            {
                Canvas[i].RefreshDataGuids();
            }
        }

        internal bool ContainGuids(int guid)
        {
            for (int i = 0; i < Canvas.Count; i++)
            {
                if (Canvas[i].ContainGuids(guid)) return true;
            }

            return false;
        }

        public int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Canvas.Count; i++)
                {
                    count += Canvas[i].Count;
                }

                return count;
            }
        }

        public T[] SelectBlockDatas<T>()
        {
            List<T> list = new List<T>();
            for (int i = 0; i < Canvas.Count; i++)
            {
                list.AddRange(Canvas[i].SelectBlockDatas<T>());
            }

            return list.ToArray();
        }
    }

    [Serializable]
    public partial class EngineBlockCanvas
    {
        public static string DefaultName = "New Canvas";
        private string m_Name = string.Empty;

        /// <summary> [Editor Data]Name </summary>
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        private Dictionary<int, IEngineBlockBaseData> m_RootBlock = new Dictionary<int, IEngineBlockBaseData>();

        private Dictionary<int, IEngineBlockBaseData> m_BlockDataDicts = new Dictionary<int, IEngineBlockBaseData>();

        private Dictionary<int, BlockFragmentDataRef> _mFragmentDataRefs = new Dictionary<int, BlockFragmentDataRef>();
        [JsonIgnore] public Dictionary<int, IEngineBlockBaseData> RootBlock => m_RootBlock;

        /// <summary>
        /// 零碎引用
        /// </summary>
        public Dictionary<int, BlockFragmentDataRef> FragmentDataRefs => _mFragmentDataRefs;

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
                if (list[i].AsCanvasData() != null && list[i].AsCanvasData().IsRoot)
                {
                    m_RootBlock[list[i].Guid] = list[i];
                }
            }

            ScratchUtils.SetDirtyType(BlocksDataDirtyType.Refresh);
        }

        public void RefreshRef()
        {
            foreach (KeyValuePair<int, BlockFragmentDataRef> fragmentDataRef in FragmentDataRefs)
            {
                fragmentDataRef.Value.RefreshRef(this);
            }
        }

        public bool AddBlocksData(IEngineBlockBaseData data)
        {
            if (m_BlockDataDicts.ContainsKey(data.Guid))
            {
                Debug.LogError("AddBlocksData Failed!");
                return false;
            }

            var canvasData = data.AsCanvasData();
            if (canvasData != null)
            {
                if (canvasData.IsRoot)
                {
                    RootBlock[data.Guid] = data;
                }
            }

            m_BlockDataDicts[data.Guid] = data;

            ScratchUtils.SetDirtyType(BlocksDataDirtyType.Add);

            return true;
        }

        public bool RemoveBlocksData(IEngineBlockBaseData data)
        {
            if (!m_BlockDataDicts.ContainsKey(data.Guid))
            {
                Debug.LogError("RemoveBlocksData Failed!");
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

        public bool RemoveBlocksData(int[] dataGuids)
        {
            bool res = true;
            foreach (int dataGuid in dataGuids)
            {
                var data = m_BlockDataDicts[dataGuid];
                if (data != null)
                {
                    res = RemoveBlocksData(data) && res;
                }
                else res = false;
            }

            return res;
        }

        public bool AddFragmentDataRef(BlockFragmentDataRef dataRef)
        {
            if (_mFragmentDataRefs.ContainsKey(dataRef.DataRefGuid))
            {
                return false;
            }

            // ScratchUtils.CreateVariableName(variableBase);
            //
            // blockFragmentDataRef = new BlockFragmentDataRef(variableBase);
            if (dataRef.DataRef is IEngineBlockVariableBase variableBase)
            {
                ScratchEngine.Instance.Current.FragmentDataRefs.Add(dataRef.DataRefGuid, dataRef);
                return true;
            }

            return false;
        }

        public bool RemoveFragmentDataRef(BlockFragmentDataRef dataRef)
        {
            if (!_mFragmentDataRefs.ContainsKey(dataRef.DataRefGuid))
            {
                return false;
            }

            return _mFragmentDataRefs.Remove(dataRef.DataRefGuid);
        }

        public bool RemoveFragmentDataRef(IEngineBlockBaseData data, out BlockFragmentDataRef[] removeArrays)
        {
            BlockFragmentDataRef dataRef = data.AsDataRef(out var refType);
            if (dataRef != null)
            {
                removeArrays = new BlockFragmentDataRef[] { dataRef };
                return RemoveFragmentDataRef(dataRef);
            }
            else
            {
                var dataGuid = data.Guid;
                removeArrays = _mFragmentDataRefs.Where(pair => pair.Value.Guid == dataGuid).Select(pair => pair.Value).ToArray();

                bool res = true;

                for (int i = 0; i < removeArrays.Length; i++)
                {
                    res = RemoveFragmentDataRef(removeArrays[i]) && res;
                }

                return res;
            }
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

                for (int i = 0; i < ScratchEngine.Instance.FileData.Global.Canvas.Count; i++)
                {
                    if (ScratchEngine.Instance.FileData.Global.Canvas[i].TryGetDataRef(param, out data))
                    {
                        return data;
                    }
                }

                return null;
            }
        }

        public bool ContainGuids(int guid)
        {
            return m_BlockDataDicts.ContainsKey(guid);
        }

        public int[] GetBlockKeys() => m_BlockDataDicts.Keys.ToArray();

        public int Count => m_BlockDataDicts.Count;
    }
}