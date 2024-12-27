using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public class BlockDrag_Operation : BlockDrag
    {
        public BlockHeaderItem_Operation CurrentOperation => GetComponent<BlockHeaderItem_Operation>();

        public override bool IsAllowDrag()
        {
            bool allowDrag = base.IsAllowDrag();

            if (Block.BlockFucType == FucType.Variable)
            {
                //返回值看看允不允许拖拽,不在Input上
                if (Block.ParentSection != null && CurrentOperation.ContextData.ParentInput == ScratchVMDataRef<BlockHeaderParam_Data_Input>.NULLRef)
                {
                    return false;
                }
            }

            return true;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsDraging) return;
            
            base.OnDrag(eventData);

            var parentInput = CurrentOperation?.ContextData.ParentInput?.GetData();
            if (parentInput != null)
            {
                parentInput.ChildOperation = null;
            }

            BlockSpot_Input spot = BlockCanvasUIManager.Instance.FindClosestSpotOfType<BlockSpot_Input>(this, BlockDragUIManager.Instance.DetectionDistance, true);

            if (spot != null)
            {
                // last selected spot
                if (BlockDragUIManager.Instance.TargetSpot != null && BlockDragUIManager.Instance.TargetSpot != spot)
                {
                    if (BlockDragUIManager.Instance.TargetSpot is BlockSpot_Input targetSpot)
                    {
                        targetSpot.ShowOutline(false);
                    }
                }

                BlockDragUIManager.Instance.SetTargetSpot(spot);
                spot.ShowOutline(true);
            }
            else
            {
                if (BlockDragUIManager.Instance.TargetSpot != null)
                {
                    if (BlockDragUIManager.Instance.TargetSpot is BlockSpot_Input targetSpot)
                    {
                        targetSpot.ShowOutline(false);
                    }

                    BlockDragUIManager.Instance.SetTargetSpot(null);
                }
            }
        }

        private void DropPlace(BlockSpot target)
        {
            this.SetParent(target.transform.parent, target.transform.GetSiblingIndex());

            if (target is BlockSpot_Input targetSpot)
            {
                targetSpot.ShowOutline(false);
            }

            BlockHeaderItem_Input input = target.GetComponent<BlockHeaderItem_Input>();
            if (input != null && input.ContextData != null)
            {
                input.ContextData.ChildOperation = CurrentOperation.ContextData.CreateRef<BlockHeaderParam_Data_Operation>();
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDraging) return;
            
            base.OnEndDrag(eventData);
            
            if (BlockDragUIManager.Instance.TargetSpot != null)
            {
                var target = BlockDragUIManager.Instance.TargetSpot;

                DropPlace(target);

                BlockDragUIManager.Instance.SetTargetSpot(null);
            }
            else
            {
                var spot = BlockCanvasUIManager.Instance.GetScratchUIAtPointer<BlockSpot>();
                if (spot != null)
                {
                    if (spot.transform.IsChildOf(BlockCanvasUIManager.Instance.transform))
                    {
                        this.SetParent(BlockCanvasUIManager.Instance);
                    }
                }
            }
            
            LocalPosition = new Vector3(LocalPosition.x, LocalPosition.y, 0);
            LocalEulerAngles = Vector3.zero;
            
            BlockDragUIManager.Instance.EndDragFixPosData(this);
        }
    }
}