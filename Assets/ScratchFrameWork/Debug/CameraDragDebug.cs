using UnityEngine;

namespace ScratchFramework
{
    /// <summary>
    /// 相机拖拽调试
    /// </summary>
    public class CameraDragDebug : MonoBehaviour
    {
        private Material m_MousePositionDraw;
        private Material m_CurrentDragBlockDraw;
        private Material m_SpotsListDraw;

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label(nameof(BlockDragUIManager.PointerPos) + BlockDragUIManager.Instance.PointerPos.ToString());
            if (BlockDragUIManager.Instance.CurrentDragBlock)
            {
                GUILayout.Label(nameof(BlockDragUIManager.CurrentDragBlock) + BlockDragUIManager.Instance.CurrentDragBlock.Position.ToString());
                GUILayout.Label(nameof(BlockDrag.OffsetPointerDown) + BlockDragUIManager.Instance.CurrentDragBlock.GetComponent<BlockDrag>().OffsetPointerDown.ToString());
            }

            GUILayout.EndVertical();
        }

        private void OnPostRender()
        {
            if (m_MousePositionDraw == null) m_MousePositionDraw = new Material(Shader.Find("Unlit/Color"));
            if (m_CurrentDragBlockDraw == null) m_CurrentDragBlockDraw = new Material(Shader.Find("Unlit/Color"));
            if (m_SpotsListDraw == null) m_SpotsListDraw = new Material(Shader.Find("Unlit/Color"));

            var CurrentDragBlock = BlockDragUIManager.Instance.CurrentDragBlock;
            var DetectionDistance = BlockDragUIManager.Instance.DetectionDistance;

            ScratchUtils.DrawScreenEllipse(Input.mousePosition, DetectionDistance, DetectionDistance, Color.gray, m_MousePositionDraw);
            if (CurrentDragBlock != null)
            {
                ScratchUtils.DrawScreenEllipse(ScratchUtils.WorldPos2ScreenPos(CurrentDragBlock.Position), DetectionDistance, DetectionDistance, Color.green, m_CurrentDragBlockDraw);
            }

            var SpotsList = BlockDragUIManager.Instance.SpotsList;
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
    }
}