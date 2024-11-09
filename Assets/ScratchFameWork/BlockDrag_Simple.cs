using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public class BlockDrag_Simple : BlockDrag
    {
        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsDraging) return;
            
            base.OnDrag(eventData);

            BlockSpot spot = BlockCanvasManager.Instance.FindClosestSpotForBlock(this, BlockDragManager.Instance.DetectionDistance);

            var ghostBlock = TempCanvasManager.Instance.GhostBlock;
            var ghostBlockTrans = ghostBlock.transform;

            if (spot is BlockSpot_SectionBody && spot.Block != Block)
            {
                ghostBlock.InsertSpot(spot.transform);
                ghostBlock.Active = true;

                BlockDragManager.Instance.SetTargetSpot(spot);
            }
            else if (spot is BlockSpot_OuterArea)
            {
                ghostBlock.InsertSpot(spot.Block.transform.parent, spot.Block.transform.GetSiblingIndex() + 1);
                ghostBlock.Active = true;

                spot.Block.ParentSection.UpdateLayout();
                BlockDragManager.Instance.SetTargetSpot(spot);
            }
            else
            {
                ghostBlock.SetParent(TempCanvasManager.Instance);
                ghostBlock.Active = false;

                BlockDragManager.Instance.SetTargetSpot(null);
            }

            //adjustments on position and angle of blocks for supporting all canvas render modes
            ghostBlockTrans.localPosition = new Vector3(ghostBlockTrans.localPosition.x, ghostBlockTrans.localPosition.y, 0);
            ghostBlockTrans.localEulerAngles = Vector3.zero;
        }


        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDraging) return;
            
            base.OnEndDrag(eventData);

            if (BlockDragManager.Instance.TargetSpot != null)
            {
                var target = BlockDragManager.Instance.TargetSpot;

                if (target is BlockSpot_SectionBody)
                {
                    this.SetParent(target.transform, 0);
                }
                else
                {
                    this.SetParent(target.Block.transform.parent, target.Block.transform.GetSiblingIndex() + 1);
                }

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

            var ghostBlock = TempCanvasManager.Instance.GhostBlock;
            ghostBlock.SetParent(TempCanvasManager.Instance);
            ghostBlock.Active = false;
        }
    }
}