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

            BlockSpot spot = BlockCanvasUIManager.Instance.FindClosestSpotForBlock(this, BlockDragUIManager.Instance.DetectionDistance);

            var ghostBlock = TempCanvasUIManager.Instance.GhostBlock;
            var ghostBlockTrans = ghostBlock.transform;

            if (spot is BlockSpot_SectionBody && spot.Block != Block)
            {
                ghostBlock.InsertSpot(spot.transform);
                ghostBlock.Active = true;

                BlockDragUIManager.Instance.SetTargetSpot(spot);
            }
            else if (spot is BlockSpot_OuterArea)
            {
                ghostBlock.InsertSpot(spot.Block.transform.parent, spot.Block.transform.GetSiblingIndex() + 1);
                ghostBlock.Active = true;

                spot.Block.ParentSection.UpdateLayout();
                BlockDragUIManager.Instance.SetTargetSpot(spot);
            }
            else
            {
                ghostBlock.SetParent(TempCanvasUIManager.Instance);
                ghostBlock.Active = false;

                BlockDragUIManager.Instance.SetTargetSpot(null);
            }

            //adjustments on position and angle of blocks for supporting all canvas render modes
            ghostBlockTrans.localPosition = new Vector3(ghostBlockTrans.localPosition.x, ghostBlockTrans.localPosition.y, 0);
            ghostBlockTrans.localEulerAngles = Vector3.zero;
        }


        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDraging) return;
            
            base.OnEndDrag(eventData);
            
            if (BlockDragUIManager.Instance.TargetSpot != null)
            {
                var target = BlockDragUIManager.Instance.TargetSpot;

                if (target is BlockSpot_SectionBody)
                {
                    this.SetParent(target.transform, 0);
                }
                else
                {
                    this.SetParent(target.Block.transform.parent, target.Block.transform.GetSiblingIndex() + 1);
                }

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

            var ghostBlock = TempCanvasUIManager.Instance.GhostBlock;
            ghostBlock.SetParent(TempCanvasUIManager.Instance);
            ghostBlock.Active = false;
            
            BlockDragUIManager.Instance.EndDragFixPosData(this);
        }
    }
}