using System;
using UnityEngine;
using UnityEngine.UI;

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
            Block block = ScratchUtils.DeserializeBlock(TemplateDatas, BlockCanvasUIManager.Instance.RectTrans);

            block.Position = TempCanvasUIManager.Instance.CanvasCenter.Position;

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
                IEngineBlockBaseData baseData = ScratchUtils.CreateBlockData(block.scratchType);
                ScratchEngine.Instance.AddBlocksData(baseData);
                if (baseData == null)
                {
                    Debug.LogError("Engine Add Block Error:" + baseData.Guid);
                }

                if (baseData is IEngineBlockVariableBase variableBase)
                {
                    var variableLabel = block.VariableLabel;
                    var variableData = variableLabel.GetVariableData();

                    ScratchUtils.CreateVariableName(variableBase);
                    //绑定数据
                    variableData.VariableRef = variableBase.Guid.ToString();

                    BlockFragmentDataRef dataRef = BlockFragmentDataRef.Create(variableBase);
                    ScratchEngine.Instance.AddFragmentDataRef(dataRef);

                    block.SetKoalaBlock(dataRef);
                }
                else
                {
                    block.SetKoalaBlock(baseData);
                }

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
                if (blockBase is IEngineBlockVariableBase variableBase)
                {
                    BlockFragmentDataRef dataRef = BlockFragmentDataRef.Create(variableBase);
                    block.SetKoalaBlock(dataRef);

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
                else
                {
                    block.SetKoalaBlock(blockBase);
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