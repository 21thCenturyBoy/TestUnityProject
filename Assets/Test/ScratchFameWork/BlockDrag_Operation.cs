using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public class BlockDrag_Operation : BlockDrag
    {
        public BlockHeaderItem_Operation CurrentOperation => GetComponent<BlockHeaderItem_Operation>();

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);

            var parentInput = CurrentOperation?.ContextData.ParentInput?.GetData();
            if (parentInput != null)
            {
                parentInput.ChildOperation = null;
            }

            BlockSpot_Input spot = BlockCanvasManager.Instance.FindClosestSpotOfType<BlockSpot_Input>(this, BlockDragManager.Instance.DetectionDistance, true);

            if (spot != null)
            {
                // last selected spot
                if (BlockDragManager.Instance.TargetSpot != null && BlockDragManager.Instance.TargetSpot != spot)
                {
                    if (BlockDragManager.Instance.TargetSpot is BlockSpot_Input targetSpot)
                    {
                        targetSpot.ShowOutline(false);
                    }
                }

                BlockDragManager.Instance.SetTargetSpot(spot);
                spot.ShowOutline(true);
            }
            else
            {
                if (BlockDragManager.Instance.TargetSpot != null)
                {
                    if (BlockDragManager.Instance.TargetSpot is BlockSpot_Input targetSpot)
                    {
                        targetSpot.ShowOutline(false);
                    }

                    BlockDragManager.Instance.SetTargetSpot(null);
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
            base.OnEndDrag(eventData);

            if (BlockDragManager.Instance.TargetSpot != null)
            {
                var target = BlockDragManager.Instance.TargetSpot;

                DropPlace(target);

                BlockDragManager.Instance.SetTargetSpot(null);
            }
            else
            {
                var spot = BlockCanvasManager.Instance.GetScratchUIAtPointer<BlockSpot>();
                if (spot != null)
                {
                    if (spot.transform.IsChildOf(BlockCanvasManager.Instance.transform))
                    {
                        this.SetParent(BlockCanvasManager.Instance);
                    }
                }
            }

            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
            transform.localEulerAngles = Vector3.zero;
        }
    }
}