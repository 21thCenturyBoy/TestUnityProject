using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ScratchFramework
{
    /// <summary>
    /// Koala数据层
    /// </summary>
    public partial class Block
    {
        #region ScratchInfo

        /// <summary>
        /// scratch type
        /// </summary>
        public ScratchBlockType scratchType;

        /// <summary>
        /// block data
        /// </summary>
        private IEngineBlockBaseData blockData;

        /// <summary>
        /// 记录兄弟索引变化
        /// </summary>
        private int lastSiblingIndex;

        /// <summary>
        /// 记录父级变化
        /// </summary>
        private Transform lastParent;

        #endregion

        public void SetKoalaBlock(IEngineBlockBaseData koalaBlockBase)
        {
            if (blockData != null)
            {
                Debug.LogError("数据没灌进去？" + koalaBlockBase.Guid);
                return;
            }

            // Debug.LogWarning("数据进去:" + koalaBlockBase.Guid + koalaBlockBase.Type);
            blockData = koalaBlockBase;
            
        }

        public IEngineBlockBaseData GetEngineBlockData()
        {
            return blockData;
        }

        public void InitKoalaData()
        {
            if (Type == BlockType.none) return;

            if (GetEngineBlockData() == null)
            {
                SetKoalaBlock(ScratchEngine.Instance.Core.CreateBlock(scratchType));
            }
        }

        public void UpdateKoalaData()
        {
            if (Type == BlockType.none) return;
            // 检查父级变化
            if (transform.parent != lastParent)
            {
                TransformParentChanged();
            }
            // 检查兄弟索引变化
            else if (transform.GetSiblingIndex() != lastSiblingIndex)
            {
                OnSiblingIndexChanged();
            }
        }


        public void DestoryKoalaData()
        {
            if (Type == BlockType.none) return;
        }

        /// <summary> 查找变量索引 </summary>
        public bool FindVarIndex(IEngineBlockBaseData parentBase, out int index)
        {
            index = -1;
            if (parentBase == null) return false;
            if (blockData is IEngineBlockVariableBase)
            {
                if (parentBase is IBlockVarGuid parentVar)
                {
                    int len = parentVar.GetVarGuidsLength();
                    for (int i = 0; i < len; i++)
                    {
                        if (parentVar.GetVarGuid(i) == blockData.Guid)
                        {
                            index = i;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary> 在Header上 </summary>
        public bool IsOnHeader(out BlockSectionHeader header, out int index)
        {
            header = null;
            index = -1;
            if (ParentTrans == null) return false;

            header = ParentTrans.GetComponent<BlockSectionHeader>();
            if (header != null)
            {
                index = RectTrans.GetSiblingIndex();
            }

            return header != null;
        }

        /// <summary> 在Body上 </summary>
        public bool IsOnBody(out BlockSectionBody body, out int bodyindex)
        {
            body = null;
            bodyindex = -1;
            if (ParentTrans == null) return false;

            body = ParentTrans.GetComponent<BlockSectionBody>();
            if (body != null)
            {
                bodyindex = RectTrans.GetSiblingIndex();
            }

            return body != null;
        }

        /// <summary> 查找新哥哥 </summary>
        public bool GetPreBlock(out Block preblock, out int preblockIndex)
        {
            preblockIndex = -1;
            preblock = null;
            if (ParentTrans == null) return false;

            int childCount = ParentTrans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Block childBlock = ParentTrans.GetChild(i).GetComponent<Block>();
                if (childBlock != null)
                {
                    if (preblock == null && childBlock == this) return false; //没哥

                    if (childBlock == this)
                    {
                        preblockIndex = i;
                        return true;
                    }

                    preblock = childBlock; //查找下一个
                }
            }

            preblock = null;
            preblockIndex = -1;
            return false;
        }

        /// <summary> 查找Input绑定Operation </summary>
        public bool GetOperationBlock(BlockHeaderItem_Input input, out BlockHeaderItem_Operation operation)
        {
            operation = null;
            if (ParentTrans == null) return false;

            if (input == null) return false;
            int inputIndex = input.RectTrans.GetSiblingIndex();
            inputIndex--;
            if (inputIndex < 0) return false;

            operation = input.ParentTrans.GetChild(inputIndex).GetComponent<BlockHeaderItem_Operation>();
            return operation != null;
        }


        public void TransformParentChanged()
        {
            if (!transform.IsChildOf(BlockCanvasManager.Instance.RectTrans)) return;

            ScratchEngine.Instance.Core.ChangeBlockData(this, lastParent, transform.parent);

            lastParent = transform.parent;
        }

        public void FixedUIPosData()
        {
            if (!transform.IsChildOf(BlockCanvasManager.Instance.RectTrans)) return;
            blockData.IsRoot = GetComponentInParent<IScratchSectionChild>() == null;

            if (blockData.IsRoot)
            {
                ScratchEngine.Instance.Core.TryFixedBlockBaseDataPos(blockData, transform.position);
            }
            else
            {
                ScratchEngine.Instance.Core.TryFixedBlockBaseDataPos(blockData, Vector3.zero);
            }
        }


        public void OnSiblingIndexChanged()
        {
            if (!transform.IsChildOf(BlockCanvasManager.Instance.RectTrans)) return;

            Debug.Log("Sibling index changed to: " + transform.GetSiblingIndex());
            lastSiblingIndex = transform.GetSiblingIndex();
        }
    }
}