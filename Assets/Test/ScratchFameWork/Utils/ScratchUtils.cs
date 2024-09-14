using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

namespace ScratchFramework
{
    using UnityObject = UnityEngine.Object;

    public static partial class ScratchUtils
    {
        public const string Block_PefixName = "Body";
        public const string Section_PefixName = "Section";
        public const string SectionHeader_PefixName = "Header";
        public const string SectionBody_PefixName = "Body";

        public static T GetOrAddComponent<T>(this UnityObject uo) where T : Component
        {
            return uo.GetComponent<T>() ?? uo.AddComponent<T>();
        }

        public static Block CloneBlock(Block block)
        {
            var dict = new Dictionary<int, int>();

            BlockData blockData = block.GetDataRef() as BlockData;

            BlockData.OrginData.Clear();
            BlockData.NewData.Clear();

            byte[] datas = blockData.Serialize();
            MemoryStream memoryStream = CreateMemoryStream(datas);

            BlockData newBlockData = new BlockData();
            newBlockData.BlockData_Deserialize(memoryStream, 0, true);
            
            Block newblock = null;

            BlockData.SetReocordDeserialize(true);


            if (newBlockData.Type == BlockType.operation)
            {
                newblock = BlockCreator.CreateBlock(newBlockData, BlockCanvasManager.Instance.RectTrans);
            }
            else
            {
                newblock = BlockCreator.CreateBlock(newBlockData, block.ParentTrans);
            }
            

            if (BlockData.OrginData.Count != BlockData.NewData.Count)
            {
                Debug.LogError("CloneBlock Failed !");
            }
            else
            {
                Dictionary<int, int> dictionary = new Dictionary<int, int>();
                List<int> orginHeadDataIds = new List<int>();
                List<int> newHeadDataIds = new List<int>();
                for (int i = 0; i < BlockData.OrginData.Count; i++)
                {
                    if (BlockData.OrginData[i] is IBlockHeadData headData)
                    {
                        int dataId = headData.GetDataId();
                        orginHeadDataIds.Add(dataId);
                    }
                }
                for (int i = 0; i < BlockData.NewData.Count; i++)
                {
                    if (BlockData.NewData[i] is IBlockHeadData headData)
                    {
                        int dataId = headData.GetDataId();
                        newHeadDataIds.Add(dataId);
                    }
                }

                int len = orginHeadDataIds.Count;
                for (int i = 0; i < len; i++)
                {
                    dictionary[orginHeadDataIds[i]] = newHeadDataIds[i];
                }
                
                
                for (int i = 0; i < BlockData.NewData.Count; i++)
                {
                    if (BlockData.NewData[i] is IScratchRefreshRef refreshRef)
                    {
                        refreshRef.RefreshRef(dictionary);
                    }
                }
            }


            // foreach (IScratchRefreshRef refreshRef in newBlockData.RefreshRefDict)
            // {
            //     refreshRef.RefreshRef(newBlockData.DataRefIdDict);
            // }

            BlockData.SetReocordDeserialize(false);

            return newblock;
        }

        public static int GetDataId(this ScratchVMData data)
        {
            if (data == null) return ScratchVMData.UnallocatedId;
            return data.IdPtr;
        }

        public static int GetDataId(this IBlockHeadData data)
        {
            if (data == null) return ScratchVMData.UnallocatedId;
            return GetDataId(data as ScratchVMData);
        }

        public static bool IdIsValid(int id)
        {
            return id > ScratchVMData.UnallocatedId;
        }

        public static int UnallocatedId(ref int id)
        {
            if (id > 0) id = -id;
            return id;
        }

        public static int AllocatedId(ref int id)
        {
            if (id < 0) id = ~id + 1;
            return id;
        }

        public static void DestroyBlock(Block block)
        {
            //TODO Data
            GameObject.Destroy(block.gameObject);
        }

        public static Vector3 ScreenPos2WorldPos(this ScratchUIBehaviour transform, Vector2 screenPos)
        {
            Vector3 worldPos = Vector3.zero;
            var camera = ScratchManager.Instance.Canvas.worldCamera;
            switch (ScratchManager.Instance.Canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    worldPos = screenPos;
                    break;
                case RenderMode.ScreenSpaceCamera:
                    worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, transform.Position.z));
                    break;
                case RenderMode.WorldSpace:
                    worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, transform.Position.z));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return worldPos;
        }

        public static Vector3 WorldPos2ScreenPos(Vector3 worldPos)
        {
            Vector3 screenPos = Vector3.zero;
            var camera = ScratchManager.Instance.Canvas.worldCamera;
            switch (ScratchManager.Instance.Canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    screenPos = worldPos;
                    break;
                case RenderMode.ScreenSpaceCamera:
                    screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    break;
                case RenderMode.WorldSpace:
                    screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return screenPos;
        }

        public static void SetParent(this ScratchBehaviour scratch, Transform parent)
        {
            scratch.transform.SetParent(parent);
        }

        public static void SetParent(this ScratchBehaviour scratch, Transform parent, int index)
        {
            scratch.transform.SetParent(parent);
            scratch.transform.SetSiblingIndex(index);
        }

        public static void SetParent(this ScratchBehaviour scratch, ScratchBehaviour parent)
        {
            scratch.SetParent(parent.transform);
        }

        private static Material s_material;

        internal static void DrawScreenRect(Rect rect, Color color)
        {
            if (s_material == null)
            {
                s_material = new Material(Shader.Find("Unlit/Color"));
            }

            s_material.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            s_material.SetColor("_Color", color);

            GL.Vertex3(rect.xMin / Screen.width, rect.yMin / Screen.height, 0);
            GL.Vertex3(rect.xMin / Screen.width, rect.yMax / Screen.height, 0);

            GL.Vertex3(rect.xMin / Screen.width, rect.yMax / Screen.height, 0);
            GL.Vertex3(rect.xMax / Screen.width, rect.yMax / Screen.height, 0);

            GL.Vertex3(rect.xMax / Screen.width, rect.yMax / Screen.height, 0);
            GL.Vertex3(rect.xMax / Screen.width, rect.yMin / Screen.height, 0);

            GL.Vertex3(rect.xMax / Screen.width, rect.yMin / Screen.height, 0);
            GL.Vertex3(rect.xMin / Screen.width, rect.yMin / Screen.height, 0);

            GL.End();
        }

        internal static void DrawScreenEllipse(Vector2 center, float xRadius, float yRadius, Color color, Material material = null, int smooth = 50)
        {
            GL.PushMatrix();
            if (material == null)
            {
                material = new Material(Shader.Find("Unlit/Color"));
            }

            material.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            material.SetColor("_Color", color);

            for (int i = 0; i < smooth; ++i)
            {
                int nextStep = (i + 1) % smooth;
                GL.Vertex3((center.x + xRadius * Mathf.Cos(2 * Mathf.PI / smooth * i)) / Screen.width,
                    (center.y + yRadius * Mathf.Sin(2 * Mathf.PI / smooth * i)) / Screen.height, 0);
                GL.Vertex3((center.x + xRadius * Mathf.Cos(2 * Mathf.PI / smooth * nextStep)) / Screen.width,
                    (center.y + yRadius * Mathf.Sin(2 * Mathf.PI / smooth * nextStep)) / Screen.height, 0);
            }

            GL.End();
            GL.PopMatrix();
        }

        public static void ConvertSimpleBlock(GameObject obj)
        {
            obj.GetOrAddComponent<Block>();
            obj.GetOrAddComponent<BlockLayout>();
            obj.GetOrAddComponent<BlockDrag_Trigger>();

            int childCount = obj.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                var selection = obj.transform.GetChild(i);

                if (selection.transform.name.Contains(Section_PefixName))
                {
                    selection.gameObject.GetOrAddComponent<BlockSection>();
                    int selectionChildCount = selection.transform.childCount;

                    for (int j = 0; j < selectionChildCount; j++)
                    {
                        var selectionChild = selection.transform.GetChild(j);

                        if (selectionChild.transform.name.Contains(SectionHeader_PefixName))
                        {
                            selectionChild.gameObject.GetOrAddComponent<BlockSectionHeader>();
                        }

                        if (selectionChild.transform.name.Contains(SectionBody_PefixName))
                        {
                            selectionChild.gameObject.GetOrAddComponent<BlockSectionBody>();
                        }
                    }
                }
            }
        }
    }
}