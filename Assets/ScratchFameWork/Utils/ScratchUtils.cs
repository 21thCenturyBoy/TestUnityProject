using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro.SpriteAssetUtilities;
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

        public static string VariableKoalaBlockToString(IEngineBlockVariableBase blockBase)
        {
            return blockBase.GetValueToString();
            // if (blockBase is KoalaIntValueBlock intValueBlock)
            // {
            //     return intValueBlock.Value.ToString();
            // }
            //
            // if (blockBase is KoalaVector3ValueBlock vector3ValueBlock)
            // {
            //     return vector3ValueBlock.Value.ToUnityVector3().ToString();
            // }
            //
            // if (blockBase is KoalaEntityValueBlock entityValueBlock)
            // {
            //     //TODO Entity
            //     return "-1";
            // }
            // return String.Empty;
        }

        public static bool String2VariableKoalaBlock(string str, IEngineBlockVariableBase blockBase)
        {
            return blockBase.SetValueToString(str) != null;
            // if (blockBase.Type == KoalaScratchType.VectorValue)
            // {
            //     str = str.Replace("(", "").Replace(")", ""); //将字符串中"("和")"替换为" "
            //     string[] s = str.Split(',');
            //
            //     if (s.Length == 3)
            //     {
            //         if (blockBase is KoalaVector3ValueBlock vector3ValueBlock)
            //         {
            //             try
            //             {
            //                 vector3ValueBlock.Value = new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2])).ToFPVector3();
            //                 return true;
            //             }
            //             catch (Exception e)
            //             {
            //                 Debug.LogError(e);
            //                 return false;
            //             }
            //         }
            //     }
            // }
            //
            // if (blockBase.Type == KoalaScratchType.IntegerValue)
            // {
            //     if (int.TryParse(str, out var res))
            //     {
            //         if (blockBase is KoalaIntValueBlock intValueBlock)
            //         {
            //             intValueBlock.Value = res;
            //             return true;
            //         }
            //     }
            // }
            //
            // if (blockBase.Type == KoalaScratchType.EntityValue)
            // {
            //     if (int.TryParse(str, out var res))
            //     {
            //         if (blockBase is KoalaEntityValueBlock entityValueBlock)
            //         {
            //             //TODO Entity
            //             // entityValueBlock.EntityGuid = res;
            //             return true;
            //         }
            //     }
            // }
            //
            // return false;
        }

        /// <summary>
        /// 反序列化UI模版
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="parentTrans"></param>
        /// <returns></returns>
        public static Block DeserializeBlock(byte[] datas, RectTransform parentTrans = null)
        {
            MemoryStream memoryStream = CreateMemoryStream(datas);

            BlockData newBlockData = new BlockData();
            newBlockData.BlockData_Deserialize(memoryStream, ScratchConfig.Instance.Version, true);

            Block newblock = null;

            if (newBlockData.Type == BlockType.Operation)
            {
                newblock = BlockCreator.CreateBlock(newBlockData, parentTrans);
            }
            else
            {
                newblock = BlockCreator.CreateBlock(newBlockData, parentTrans);
            }

            return newblock;
        }

        /// <summary>
        /// 克隆Block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static Block CloneBlock(Block block)
        {
            var dict = new Dictionary<int, int>();

            BlockData blockData = block.GetDataRef() as BlockData;

            BlockData.OrginData.Clear();
            BlockData.NewData.Clear();

            byte[] datas = blockData.Serialize();

            MemoryStream memoryStream = CreateMemoryStream(datas);

            BlockData newBlockData = new BlockData();
            newBlockData.BlockData_Deserialize(memoryStream, ScratchConfig.Instance.Version, true);

            Block newblock = null;


            if (newBlockData.Type == BlockType.Operation)
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

            //TODO 
            // KoalaScratchUtil.Instance.CopyBlockTree()
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
            ScratchEngine.Instance.Core.DeleteBlock(block.GetEngineBlockData());

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

        public static Dictionary<string, byte[]> ConvertSimpleBlock(GameObject[] objs)
        {
            Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
            for (int i = 0; i < objs.Length; i++)
            {
                Block block = objs[i].GetComponent<Block>();
                if (block != null)
                {
                    var data = block.GetDataRef();
                    BlockData.OrginData.Clear();
                    byte[] datas = data.Serialize();
                    string fileName = objs[i].name.Replace(".", "_").Trim();

                    dictionary[fileName] = datas;
                }
            }

            return dictionary;
        }
    }
}