namespace ScratchFramework
{
    public class ScratchEngine : Singleton<ScratchEngine>
    {
        IEngineCoreInterface m_engineCore;
        public IEngineCoreInterface Core => m_engineCore ??= new TestEngineCore();

        private EngineBlockFileData m_FileData;

        public EngineBlockFileData FileData
        {
            get => m_FileData;
        }

        private EngineBlockCanvasGroup m_CurrentGroup;

        public EngineBlockCanvasGroup CurrentGroup
        {
            get => m_CurrentGroup;
            set
            {
                if (m_CurrentGroup == value) return;
                m_CurrentGroup = value;

                Current = CurrentGroup.Canvas[0];
            }
        }

        private EngineBlockCanvas m_Current;

        public EngineBlockCanvas Current
        {
            get => m_Current;
            set
            {
                if (m_Current == value) return;
                m_Current = value;

                ScratchUtils.SetDirtyType(BlocksDataDirtyType.Change);
            }
        }

        public void SetFileData(EngineBlockFileData fileData)
        {
            m_FileData = fileData;

            m_CurrentGroup = fileData.Global;
            Current = CurrentGroup.Canvas[0];

            m_FileData.RefreshRef();
            m_FileData.RefreshDataGuids();
            BlockCanvasUIManager.Instance.RefreshCanvas();

            TempCanvasUIManager.Instance.TopCanvasGroup.Initialize();
        }

        public bool CurrentIsGlobal => CurrentGroup == FileData.Global;

        public bool ContainGuids(int guid)
        {
            return FileData.ContainGuids(guid);
        }

        public void SerachVariableData(out IEngineBlockBaseData[] listGlobal, out IEngineBlockBaseData[] listLocal)
        {
            listGlobal = FileData.Global.SelectBlockDatas<IEngineBlockVariableBase>();
            listLocal = Current.SelectBlockDatas<IEngineBlockVariableBase>();
        }

        public bool AddBlocksData(params IEngineBlockBaseData[] datas)
        {
            bool res = true;
            for (int i = 0; i < datas.Length; i++)
            {
                res = m_Current.AddBlocksData(datas[i]) && res;
            }

            return res;
        }

        public bool RemoveCurrentCanvasData(params IEngineBlockBaseData[] datas)
        {
            bool res = true;
            for (int i = 0; i < datas.Length; i++)
            {
                res = m_Current.RemoveBlocksData(datas[i]) && res;
            }

            return res;
        }

        public bool RemoveCurrentCanvasData(params int[] datas)
        {
            return m_Current.RemoveBlocksData(datas);
        }

        public bool AddFragmentDataRef(BlockFragmentDataRef dataRef)
        {
            return m_Current.AddFragmentDataRef(dataRef);
        }

        public bool RemoveFragmentDataRef(BlockFragmentDataRef dataRef)
        {
            return m_Current.RemoveFragmentDataRef(dataRef);
        }

        public bool RemoveFileFragmentRef(IEngineBlockBaseData dataRef)
        {
            return FileData.RemoveAllFragmentDataRef(dataRef);
        }

        public void RemoveFileData(IEngineBlockBaseData dataRef)
        {
            foreach (var canvas in FileData.Global.Canvas)
            {
                if (canvas.ContainGuids(dataRef.Guid))
                {
                    canvas.RemoveBlocksData(dataRef);
                }
            }

            foreach (var group in FileData.CanvasGroups)
            {
                foreach (var canvas in group.Canvas)
                {
                    if (canvas.ContainGuids(dataRef.Guid))
                    {
                        canvas.RemoveBlocksData(dataRef);
                    }
                }
            }
            
        }
    }
}