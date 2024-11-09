using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    public static class BlockCreator
    {
        private static int IdIndex = 0;

        public static Block CreateBlock(BlockData data, RectTransform parentTrans)
        {
            GameObject obj = new GameObject($"[{IdIndex}]" + data.Name);
            IdIndex++;

            Block block = obj.AddComponent<Block>();
            BlockLayout layout = obj.AddComponent<BlockLayout>();

            block.SetParent(parentTrans);

            RectTransform rectTransform = block.TryAddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            
            block.BlockFucType = data.BlockFucType;
            block.Type = data.Type;
            block.Version = data.Version;
            block.scratchType = data.ScratchType;

            //TODO Other Serialize Data
            switch (data.BlockFucType)
            {
                case FucType.Undefined:
                    AddVerticalLayoutGroup(rectTransform);
                    CreateSections(block, data);
                    layout.Color = ScratchConfig.Instance.BlockColor_Undefined;
                    break;
                case FucType.Event:
                    AddVerticalLayoutGroup(rectTransform);
                    CreateSections(block, data);
                    layout.Color = ScratchConfig.Instance.BlockColor_Event;
                    break;
                case FucType.Action:
                    AddVerticalLayoutGroup(rectTransform);
                    CreateSections(block, data);
                    layout.Color = ScratchConfig.Instance.BlockColor_Action;
                    break;
                case FucType.Control:
                    AddVerticalLayoutGroup(rectTransform);
                    CreateSections(block, data);
                    layout.Color = ScratchConfig.Instance.BlockColor_Control;
                    break;
                case FucType.Condition:
                    AddVerticalLayoutGroup(rectTransform);
                    CreateSections(block, data);
                    layout.Color = ScratchConfig.Instance.BlockColor_Condition;
                    break;
                case FucType.GetValue:
                    AddVerticalLayoutGroup(rectTransform);
                    CreateSections(block, data);
                    layout.Color = ScratchConfig.Instance.BlockColor_GetValue;
                    break;
                case FucType.Variable:
                    AddVerticalLayoutGroup(rectTransform);
                    CreateSections(block, data);
                    layout.Color = ScratchConfig.Instance.BlockColor_Variable;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (data.Type)
            {
                case BlockType.none:
                    break;
                case BlockType.trigger:
                    CreateOuterArea(block);
                    block.TryAddComponent<BlockDrag_Trigger>();
                    break;
                case BlockType.simple:
                    CreateOuterArea(block);
                    block.TryAddComponent<BlockDrag_Simple>();
                    break;
                case BlockType.condition:
                    CreateOuterArea(block);
                    block.TryAddComponent<BlockDrag_Simple>();
                    break;
                case BlockType.loop:
                    CreateOuterArea(block);
                    block.TryAddComponent<BlockDrag_Simple>();
                    break;
                case BlockType.operation:
                    BlockHeaderItem_Operation HeadOperation = block.TryAddComponent<BlockHeaderItem_Operation>();
                    block.TryAddComponent<BlockDrag_Operation>();
                    break;
                case BlockType.define:
                    CreateOuterArea(block);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            layout.FixedScale();
            
            block.LocalPosition = data.LocalPosition;

            return block;
        }

        #region LayoutGroup

        private static VerticalLayoutGroup AddVerticalLayoutGroup(RectTransform transform)
        {
            VerticalLayoutGroup verticalLayoutGroup = transform.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            verticalLayoutGroup.childForceExpandHeight = true;
            verticalLayoutGroup.childForceExpandWidth = true;

            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.childScaleHeight = false;
            verticalLayoutGroup.childScaleWidth = false;

            return verticalLayoutGroup;
        }

        private static HorizontalLayoutGroup AddHorizontalLayoutGroup(RectTransform transform)
        {
            HorizontalLayoutGroup horizontalLayoutGroup = transform.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;

            horizontalLayoutGroup.spacing = 15;

            horizontalLayoutGroup.childForceExpandHeight = true;
            horizontalLayoutGroup.childForceExpandWidth = false;

            horizontalLayoutGroup.childControlHeight = false;
            horizontalLayoutGroup.childControlWidth = false;
            horizontalLayoutGroup.childScaleHeight = false;
            horizontalLayoutGroup.childScaleWidth = false;

            return horizontalLayoutGroup;
        }

        #endregion


        private static void CreateSections(Block block, BlockData data)
        {
            bool isChildForceExpend = data.SectionTreeList.Length <= 1;

            for (int i = 0; i < data.SectionTreeList.Length; i++)
            {
                BlockSectionData sectionData = data.SectionTreeList[i] as BlockSectionData;
                GameObject obj = new GameObject($"{nameof(BlockSection)}_{i}");

                obj.transform.SetParent(block.transform);

                BlockSection section = obj.AddComponent<BlockSection>();
                RectTransform rectTransform = obj.AddComponent<RectTransform>();

                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);

                VerticalLayoutGroup verticalLayoutGroup = AddVerticalLayoutGroup(rectTransform);

                verticalLayoutGroup.childForceExpandHeight = isChildForceExpend;
                verticalLayoutGroup.childForceExpandWidth = isChildForceExpend;


                BlockSectionHeader header = CreateHeader(rectTransform, i, data);

                BlockSectionBody body = CreateBody(rectTransform, i, data);
            }
        }

        private static BlockSectionBody CreateBody(RectTransform sectionTrans, int sectionIndex, BlockData data)
        {
            if (data.Type == BlockType.none) return null;
            if (data.Type == BlockType.operation) return null;
            if (data.Type == BlockType.simple) return null;


            GameObject obj = new GameObject($"{nameof(BlockSectionBody)}");

            obj.transform.SetParent(sectionTrans);

            BlockSectionBody body = obj.AddComponent<BlockSectionBody>();

            RectTransform rectTransform = obj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            BlockImage image = obj.AddComponent<BlockImage>();

            VerticalLayoutGroup verticaltalLayoutGroup = AddVerticalLayoutGroup(rectTransform);

            verticaltalLayoutGroup.spacing = -10;

            verticaltalLayoutGroup.childAlignment = TextAnchor.UpperLeft;

            verticaltalLayoutGroup.childForceExpandHeight = false;
            verticaltalLayoutGroup.childForceExpandWidth = false;

            switch (data.Type)
            {
                case BlockType.none:
                    break;
                case BlockType.trigger:
                    verticaltalLayoutGroup.padding = new RectOffset(0, 0, -10, 0);

                    body.TryAddComponent<BlockSpot_SectionBody>();
                    break;
                case BlockType.simple:
                    break;
                case BlockType.condition:
                    verticaltalLayoutGroup.padding = new RectOffset(20, 0, -10, 0);
                    if (data.SectionTreeList.Length == 1)
                    {
                        image.Image.sprite = ScratchConfig.Instance.Condition_EndBody;
                    }

                    if (data.SectionTreeList.Length == 2)
                    {
                        if (sectionIndex == 0)
                        {
                            image.Image.sprite = ScratchConfig.Instance.Condition_MiddleBody;
                        }

                        if (sectionIndex == 1)
                        {
                            image.Image.sprite = ScratchConfig.Instance.Condition_EndBody;
                        }
                    }

                    body.TryAddComponent<BlockSpot_SectionBody>();

                    break;
                case BlockType.loop:
                    verticaltalLayoutGroup.padding = new RectOffset(20, 0, -10, 0);
                    image.Image.sprite = ScratchConfig.Instance.Condition_EndBody;

                    body.TryAddComponent<BlockSpot_SectionBody>();
                    break;
                case BlockType.operation:
                    break;
                case BlockType.define:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (data.BlockFucType)
            {
                case FucType.Undefined:
                    break;
                case FucType.Event:
                    break;
                case FucType.Action:
                    break;
                case FucType.Control:
                    break;
                case FucType.Condition:
                    break;
                case FucType.GetValue:
                    break;
                case FucType.Variable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CreateBodyItems(body.RectTrans, data.SectionTreeList[sectionIndex] as BlockSectionData);
            return body;
        }

        private static void CreateBodyItems(RectTransform bodyTrans, BlockSectionData sectionData)
        {
            for (int j = 0; j < sectionData.BlockTreeList.Length; j++)
            {
                IBlockData blockData = sectionData.BlockTreeList[j];
                CreateBodyItem(bodyTrans, sectionData, blockData as BlockData);
            }
        }

        private static Block CreateBodyItem(RectTransform bodyTrans, BlockSectionData sectionData, BlockData blockData)
        {
            IBlockData blockHeadItem = blockData;

            Block block = CreateBlock(blockData, bodyTrans);
            return block;
        }

        private static void CreateOuterArea(Block block)
        {
            GameObject obj = new GameObject($"{nameof(BlockSpot_OuterArea)}");

            obj.transform.SetParent(block.transform);

            BlockSpot_OuterArea section = obj.AddComponent<BlockSpot_OuterArea>();
            RectTransform rectTransform = obj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
        }

        #region Head

        private static BlockSectionHeader CreateHeader(RectTransform sectionTrans, int sectionIndex, BlockData data)
        {
            GameObject obj = new GameObject($"{nameof(BlockSectionHeader)}");

            obj.transform.SetParent(sectionTrans);

            BlockSectionHeader header = obj.AddComponent<BlockSectionHeader>();

            header.minHeight = 80;
            header.minWidth = 150;

            RectTransform rectTransform = obj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            BlockImage image = obj.AddComponent<BlockImage>();

            HorizontalLayoutGroup horizontalLayoutGroup = AddHorizontalLayoutGroup(rectTransform);
            switch (data.Type)
            {
                case BlockType.none:
                    image.Image.sprite = ScratchConfig.Instance.Simple_HeaderGhost;
                    break;
                case BlockType.trigger:
                    image.Image.sprite = ScratchConfig.Instance.Trigger_Header;

                    header.minHeight = 105;
                    header.minWidth = 150;

                    break;
                case BlockType.simple:
                    image.Image.sprite = ScratchConfig.Instance.Simple_Header;
                    break;
                case BlockType.condition:
                    if (data.SectionTreeList.Length == 1)
                    {
                        image.Image.sprite = ScratchConfig.Instance.Condition_Header;
                    }

                    if (data.SectionTreeList.Length == 2)
                    {
                        if (sectionIndex == 0)
                        {
                            image.Image.sprite = ScratchConfig.Instance.Condition_Header;
                        }

                        if (sectionIndex == 1)
                        {
                            image.Image.sprite = ScratchConfig.Instance.Condition_MiddleHeader;
                        }
                    }

                    break;
                case BlockType.loop:
                    image.Image.sprite = ScratchConfig.Instance.Condition_Header;
                    break;
                case BlockType.operation:
                    image.Image.sprite = ScratchConfig.Instance.Operation_Header;

                    header.minHeight = 50;

                    break;
                case BlockType.define:
                    image.Image.sprite = ScratchConfig.Instance.Define_Header;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            switch (data.BlockFucType)
            {
                case FucType.Undefined:
                    horizontalLayoutGroup.padding = new RectOffset(15, 0, 10, 20);
                    break;
                case FucType.Event:
                    horizontalLayoutGroup.padding = new RectOffset(15, 0, 30, 20);
                    break;
                case FucType.Action:
                    horizontalLayoutGroup.padding = new RectOffset(15, 0, 10, 20);
                    break;
                case FucType.Control:

                    if (data.SectionTreeList.Length == 1)
                    {
                        horizontalLayoutGroup.padding = new RectOffset(15, 0, 10, 20);
                    }

                    if (data.SectionTreeList.Length == 2)
                    {
                        if (sectionIndex == 0)
                        {
                            horizontalLayoutGroup.padding = new RectOffset(15, 0, 10, 20);
                        }

                        if (sectionIndex == 1)
                        {
                            horizontalLayoutGroup.padding = new RectOffset(15, 0, 5, 5);
                            header.minHeight = 60;
                            header.SetSize(new Vector2(header.GetSize().x, header.minHeight));
                        }
                    }

                    break;
                case FucType.Condition:
                    horizontalLayoutGroup.padding = new RectOffset(15, 0, 0, 0);

                    break;
                case FucType.GetValue:
                    horizontalLayoutGroup.padding = new RectOffset(15, 0, 0, 0);
                    break;
                case FucType.Variable:
                    horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                    horizontalLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CreateHeadItems(header.RectTrans, data.SectionTreeList[sectionIndex] as BlockSectionData);
            return header;
        }

        private static void CreateHeadItems(RectTransform headerTrans, BlockSectionData sectionData)
        {
            for (int j = 0; j < sectionData.BlockHeadTreeList.Length; j++)
            {
                IBlockHeadData blockHeadData = sectionData.BlockHeadTreeList[j];
                CreateHeadItem(headerTrans, sectionData, blockHeadData);
            }
        }

        private static IBlockScratch_Head CreateHeadItem(RectTransform headerTrans, BlockSectionData sectionData, IBlockHeadData headData)
        {
            IBlockHeadData blockHeadItem = headData;
            GameObject prefab = null;
            IBlockScratch_Head scratchHead = null;
            switch (blockHeadItem.DataType)
            {
                case DataType.Undefined:
                    break;
                case DataType.Label:
                    prefab = ScratchConfig.Instance.Prefab_Label;
                    var scratchHead_Label = GameObject.Instantiate(prefab, headerTrans).AddComponent<BlockHeaderItem_Label>();
                    scratchHead = scratchHead_Label;
                    break;
                case DataType.Input:
                    prefab = ScratchConfig.Instance.Prefab_Input;
                    var scratchHead_Input = GameObject.Instantiate(prefab, headerTrans).AddComponent<BlockHeaderItem_Input>();
                    scratchHead = scratchHead_Input;
                    break;
                case DataType.Operation:
                    BlockHeaderParam_Data_Operation headOperationData = headData as BlockHeaderParam_Data_Operation;
                    var blockData = headOperationData.GetBlockData() as BlockData;
                    Block block = CreateBlock(blockData, headerTrans);
                    var scratchHead_Operation = block.TryAddComponent<BlockHeaderItem_Operation>();
                    scratchHead = scratchHead_Operation;

                    break;
                case DataType.VariableLabel:
                    prefab = ScratchConfig.Instance.Prefab_VariabelLabel;
                    var scratchHead_VariableLabel = GameObject.Instantiate(prefab, headerTrans).AddComponent<BlockHeaderItem_VariableLabel>();
                    scratchHead = scratchHead_VariableLabel;
                    // scratchHead_VariableLabel.enabled = true;
                    break;
                case DataType.RenturnVariableLabel:
                    prefab = ScratchConfig.Instance.Prefab_ReturnVariabelLabel;
                    var scratchHead_RenturnVariableLabel = GameObject.Instantiate(prefab, headerTrans).AddComponent<BlockHeaderItem_RenturnVariableLabel>();
                    scratchHead = scratchHead_RenturnVariableLabel;
                    // scratchHead_VariableLabel.enabled = true;
                    break;
                case DataType.Icon:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            scratchHead.SetData(headData);

            scratchHead.RefreshUI();
            return scratchHead;
        }

        #endregion
    }

    public static class BlockDirector
    {
        public static Block InitializeStruct(Block block)
        {
            return block.AddUIVisible();
        }



        private static Block AddUIVisible(this Block block)
        {
            switch (block.Type)
            {
                case BlockType.none:
                    break;
                case BlockType.trigger:
                    var sections = block.Layout.SectionsArray;
                    for (int i = 0; i < sections.Length; i++)
                    {
                        sections[i].Body.Image.Visible = false;
                    }
                    break;
                case BlockType.simple:
                    break;
                case BlockType.condition:
                    break;
                case BlockType.loop:
                    break;
                case BlockType.operation:
                    break;
                case BlockType.define:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return block;
        }
    }
}