using System;
using Button = UnityEngine.UI.Button;

namespace ScratchFramework
{
    [Serializable]
    public class ResourcesItemData
    {
        public byte[] TemplateDatas;
        public string Name;
        public FucType BlockFucType { get; set; }
        public BlockType Type { get; set; }
        public ScratchBlockType ScratchType { get; set; }

        public ResourcesItemData(byte[] templateDatas, string name)
        {
            TemplateDatas = templateDatas;
            Name = name;
        }

        /// <summary>
        /// 创建BlockUI
        /// </summary>
        /// <param name="blockBase">UI 创建数据传NULL</param>
        /// <returns></returns>
        public Block CreateBlock(IEngineBlockBaseData blockBase)
        {
            Block block = ScratchUtils.DeserializeBlock(TemplateDatas, BlockCanvasManager.Instance.RectTrans);

            block.Position = TempCanvasManager.Instance.CanvasCenter.Position;

            //创建数据
            if (blockBase == null)
            {
                SetKoalaBlockData(block);
            }
            else
            {
                SetKoalaBlockData(block, blockBase);
            }

            return block;
        }

        private void SetKoalaBlockData(Block block)
        {
            if (block.GetEngineBlockData() == null)
            {
                block.InitKoalaData();

                block.TransformParentChanged();
                var tag = block.GetComponentInParent<IScratchSectionChild>();
                if (tag == null) block.FixedUIPosData();

                block.OnSiblingIndexChanged();
                var sections = block.GetChildSection();

                for (int i = 0; i < sections.Count; i++)
                {
                    if (sections[i].Header != null)
                    {
                        Block[] heads = sections[i].Header.GetComponentsInChildren<Block>();
                        for (int j = 0; j < heads.Length; j++)
                        {
                            SetKoalaBlockData(heads[j]);
                        }

                        IBlockScratch_Head[] headDatas = sections[i].Header.GetComponentsInChildren<IBlockScratch_Head>();
                        for (int j = 0; j < headDatas.Length; j++)
                        {
                            headDatas[j].RefreshUI();
                        }
                    }

                    if (sections[i].Body != null)
                    {
                        Block[] bodys = sections[i].Body.GetComponentsInChildren<Block>();
                        for (int j = 0; j < bodys.Length; j++)
                        {
                            SetKoalaBlockData(bodys[j]);
                        }
                    }
                }
            }
        }

        private void SetKoalaBlockData(Block block, IEngineBlockBaseData blockBase)
        {
            if (block.GetEngineBlockData() == null)
            {
                block.SetKoalaBlock(blockBase);

                if (blockBase is IEngineBlockVariableBase variableBase)
                {
                    var sections = block.GetChildSection();
                    for (int i = 0; i < sections.Count; i++)
                    {
                        if (sections[i].Header != null)
                        {
                            IBlockScratch_Head[] heads = sections[i].Header.GetComponentsInChildren<IBlockScratch_Head>();
                            for (int j = 0; j < heads.Length; j++)
                            {
                                if (heads[j].DataRef() is IHeaderParamVariable paramVariable)
                                {
                                    ScratchUtils.RefreshVariableName(variableBase, paramVariable);
                                }

                                heads[j].RefreshUI();
                            }
                        }
                    }
                }
            }
        }
    }

    public class ResourcesItem : ScratchBehaviour
    {
        public ResourcesItemData Data = null;

        public Button CreateBtn;

        public TMPro.TMP_Text NameText;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (CreateBtn == null) CreateBtn = transform.Find("CreateBtn").GetComponent<Button>();
            if (NameText == null) NameText = CreateBtn.GetComponentInChildren<TMPro.TMP_Text>();

            CreateBtn.onClick.RemoveAllListeners();
            CreateBtn.onClick.AddListener(CreateBlockTemplate);

            NameText.text = Data.Name;

            if (Data != null)
            {
                CreateBtn.image.color = ScratchConfig.Instance.GetFucColor(Data.BlockFucType);
            }


            CreateBtn.gameObject.SetActive(true);
        }

        public virtual void CreateBlockTemplate()
        {
            if (Data != null)
            {
                Data.CreateBlock(null);
            }
        }
    }
}