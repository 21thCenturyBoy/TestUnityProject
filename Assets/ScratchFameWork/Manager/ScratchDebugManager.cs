using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public class CameraDragDebug : MonoBehaviour
    {
#if UNITY_EDITOR

        private Material m_MousePositionDraw;
        private Material m_CurrentDragBlockDraw;
        private Material m_SpotsListDraw;

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label(nameof(BlockDragManager.PointerPos) + BlockDragManager.Instance.PointerPos.ToString());
            if (BlockDragManager.Instance.CurrentDragBlock)
            {
                GUILayout.Label(nameof(BlockDragManager.CurrentDragBlock) + BlockDragManager.Instance.CurrentDragBlock.Position.ToString());
                GUILayout.Label(nameof(BlockDrag.OffsetPointerDown) + BlockDragManager.Instance.CurrentDragBlock.GetComponent<BlockDrag>().OffsetPointerDown.ToString());
            }

            GUILayout.EndVertical();
        }

        private void OnPostRender()
        {
            if (m_MousePositionDraw == null) m_MousePositionDraw = new Material(Shader.Find("Unlit/Color"));
            if (m_CurrentDragBlockDraw == null) m_CurrentDragBlockDraw = new Material(Shader.Find("Unlit/Color"));
            if (m_SpotsListDraw == null) m_SpotsListDraw = new Material(Shader.Find("Unlit/Color"));

            var CurrentDragBlock = BlockDragManager.Instance.CurrentDragBlock;
            var DetectionDistance = BlockDragManager.Instance.DetectionDistance;

            ScratchUtils.DrawScreenEllipse(Input.mousePosition, DetectionDistance, DetectionDistance, Color.gray, m_MousePositionDraw);
            if (CurrentDragBlock != null)
            {
                ScratchUtils.DrawScreenEllipse(ScratchUtils.WorldPos2ScreenPos(CurrentDragBlock.Position), DetectionDistance, DetectionDistance, Color.green, m_CurrentDragBlockDraw);
            }

            var SpotsList = BlockDragManager.Instance.SpotsList;
            for (int i = 0; i < SpotsList.Count; i++)
            {
                if (SpotsList[i] != null)
                {
                    if (CurrentDragBlock == null)
                    {
                        ScratchUtils.DrawScreenEllipse(ScratchUtils.WorldPos2ScreenPos(SpotsList[i].DropPosition), DetectionDistance, DetectionDistance, Color.yellow, m_SpotsListDraw);
                    }
                    else
                    {
                        if (SpotsList[i].GetComponentInParent<Block>() != CurrentDragBlock)
                        {
                            ScratchUtils.DrawScreenEllipse(ScratchUtils.WorldPos2ScreenPos(SpotsList[i].DropPosition), DetectionDistance, DetectionDistance, Color.yellow, m_SpotsListDraw);
                        }
                    }
                }
            }
        }

#endif
    }

    public class ScratchDebugManager : ScratchSingleton<ScratchDebugManager>, IScratchManager
    {
        [Header("Drag Debug")] public bool DragDebug = false;

        private CameraDragDebug m_CameraDragDebug;
        private bool m_pre_DragDebug = false;

        public void OnUpdate()
        {
            if (m_pre_DragDebug != DragDebug)
            {
                if (DragDebug)
                {
                    if (m_CameraDragDebug == null)
                    {
                        m_CameraDragDebug = ScratchManager.Instance.CanvasCamera.gameObject.AddComponent<CameraDragDebug>();
                    }
                }
                else
                {
                    if (m_CameraDragDebug != null)
                    {
                        Destroy(m_CameraDragDebug);
                        m_CameraDragDebug = null;
                    }
                }
            }

            m_pre_DragDebug = DragDebug;
        }

        public void OnLateUpdate()
        {
        }

        public bool Clear()
        {
            return true;
        }
    }
}