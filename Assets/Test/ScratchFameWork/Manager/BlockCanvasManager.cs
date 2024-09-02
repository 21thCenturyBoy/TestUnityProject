using System.Collections;
using System.Collections.Generic;
using MG_BlocksEngine2.Block;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public class BlockCanvasManager : ScratchUISingleton<BlockCanvasManager>, IScratchManager
    {
        protected override void OnInitialize()
        {
            // Move to Block ,Clear Temp Canvas
            var childs = TempCanvasManager.Instance.GetChildTempBlock();
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].SetParent(transform);
            }

            base.OnInitialize();
        }

        public void OnUpdate()
        {
        }

        public void OnLateUpdate()
        {
        }

        public bool Clear()
        {
            return true;
        }

        public T GetScratchUIAtPointer<T>() where T : ScratchUIBehaviour
        {
            EventSystem eventSystem = EventSystem.current;

            if (eventSystem == null) return null;

            PointerEventData _pointerEventData = new PointerEventData(eventSystem);

            _pointerEventData.position = BlockDragManager.Instance.PointerPos;
            List<RaycastResult> globalResults = new List<RaycastResult>();
            eventSystem.RaycastAll(_pointerEventData, globalResults);

            int resultCount = globalResults.Count;
            for (int i = 0; i < resultCount; i++)
            {
                RaycastResult result = globalResults[i];

                var com = result.gameObject.GetComponent<T>();
                if (com != null) return com;
            }

            return null;
        }

        public T[] GetScratchUIsAtPointer<T>() where T : ScratchUIBehaviour
        {
            EventSystem eventSystem = EventSystem.current;

            List<T> results = new List<T>();
            if (eventSystem == null) return results.ToArray();

            PointerEventData _pointerEventData = new PointerEventData(eventSystem);

            _pointerEventData.position = BlockDragManager.Instance.PointerPos;
            List<RaycastResult> globalResults = new List<RaycastResult>();
            eventSystem.RaycastAll(_pointerEventData, globalResults);

            int resultCount = globalResults.Count;
            for (int i = 0; i < resultCount; i++)
            {
                RaycastResult result = globalResults[i];
                results.AddRange(result.gameObject.GetComponents<T>());
            }

            return results.ToArray();
        }

        public T FindClosestSpotOfType<T>(BlockDrag drag, float maxDistance, bool usePointer = false) where T : BlockSpot
        {
            float minDistance = Mathf.Infinity;
            T found = null;
            var spots = BlockDragManager.Instance.SpotsList;
            Vector2 dragPos = ScratchUtils.WorldPos2ScreenPos(drag.Position);
            for (int i = 0; i < spots.Count; i++)
            {
                BlockSpot spot = spots[i];

                if ((spot is T targetT && spot.Active && spot.Visible))
                {
                    BlockDrag d = spot.GetComponentInParent<BlockDrag>();

                    if (d.transform.IsChildOf(transform))
                    {
                        if (d != drag && Active && Visible)
                        {
                            Vector2 spotPos = ScratchUtils.WorldPos2ScreenPos(spot.DropPosition);
                            float distance = Vector2.Distance(usePointer ? BlockDragManager.Instance.PointerPos : dragPos, spotPos);

                            if (distance < minDistance && distance <= maxDistance)
                            {
                                found = targetT;
                                minDistance = distance;
                            }
                        }
                    }
                }
            }

            return found;
        }

        public BlockSpot FindClosestSpotForBlock(BlockDrag drag, float maxDistance, bool usePointer = false)
        {
            float minDistance = Mathf.Infinity;
            BlockSpot found = null;
            var spots = BlockDragManager.Instance.SpotsList;
            Vector2 dragPos = ScratchUtils.WorldPos2ScreenPos(drag.Position);
            for (int i = 0; i < spots.Count; i++)
            {
                BlockSpot spot = spots[i];

                if ((spot is BlockSpot_SectionBody || (spot is BlockSpot_OuterArea && spot.Block.ParentSection != null)) && spot.Active && spot.Visible)
                {
                    BlockDrag d = spot.GetComponentInParent<BlockDrag>();

                    if (d.transform.IsChildOf(transform))
                    {
                        if (d != drag && Active && Visible)
                        {
                            Vector2 spotPos = ScratchUtils.WorldPos2ScreenPos(spot.DropPosition);
                            float distance = Vector2.Distance(usePointer ? BlockDragManager.Instance.PointerPos : dragPos, spotPos);
                            if (distance < minDistance && distance <= maxDistance)
                            {
                                found = spot;
                                minDistance = distance;
                            }
                        }
                    }
                }
            }

            return found;
        }
    }
}