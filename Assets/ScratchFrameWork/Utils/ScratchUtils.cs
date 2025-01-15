using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public static partial class ScratchUtils
    {
        public const int InvalidGuid = 0;

        public static int CreateGuid()
        {
            return CreateGuid(out var guid);
        }

        public static int CreateGuid(out Guid guid)
        {
            guid = Guid.NewGuid();
            int hashCode = guid.GetHashCode();
            var repeatGuid = ScratchEngine.Instance.ContainGuids(hashCode);
            while (hashCode == InvalidGuid || repeatGuid)
            {
                guid = Guid.NewGuid();
                hashCode = guid.GetHashCode();
            }

            return hashCode;
        }

        public static string EngineVariableToString(IEngineBlockVariableBase blockBase)
        {
            if (ScratchEngine.Instance.Core.VariableValue2String(blockBase, out var strRes))
            {
                return strRes;
            }
            else
            {
                return string.Empty;
            }
        }

        public static bool SetNextGuid(this IEngineBlockBaseData blockBase, int nextGuid)
        {
            if (blockBase is IBlockPlug plug)
            {
                plug.NextGuid = nextGuid;
                return true;
            }

            return false;
        }

        public static int GetNextPlug(this IEngineBlockBaseData blockBase)
        {
            if (blockBase is IBlockPlug plug)
            {
                return plug.NextGuid;
            }

            return InvalidGuid;
        }

        public static int GetBranchCount(this IEngineBlockBranch branch)
        {
            return branch.BranchBlockBGuids.Length;
        }

        public static bool String2VariableEngine(string str, IEngineBlockVariableBase blockBase)
        {
            return ScratchEngine.Instance.Core.String2VariableValueTo(blockBase, str);
        }

        public static void CreateVariableName(IEngineBlockVariableBase blockdata)
        {
            //创建变量名
            if (string.IsNullOrEmpty(blockdata.VariableName))
            {
                string variableRef = blockdata.Guid.ToString();
                switch (blockdata.Type)
                {
                    case ScratchBlockType.IntegerValue:
                        blockdata.VariableName = $"[int]{variableRef}";
                        break;
                    case ScratchBlockType.VectorValue:
                        blockdata.VariableName = $"[Vector3]{variableRef}";
                        break;
                    case ScratchBlockType.EntityValue:
                        blockdata.VariableName = $"[Entity]{variableRef}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static IEngineBlockBaseData GetBlocksDataRef(int guid)
        {
            return ScratchEngine.Instance.Current[guid];
        }

        public static IEngineBlockBaseData CreateBlockData(ScratchBlockType scratchType)
        {
            // IEngineBlockBaseData block = null;
            IEngineBlockBaseData block = scratchType.CreateBlockData();
            block.Guid = CreateGuid();

            return block;
        }

        public static bool IsReturnVariable(this IEngineBlockBaseData blockBase)
        {
            if (blockBase is IEngineBlockVariableBase variableBase)
            {
                return variableBase.ReturnParentGuid != InvalidGuid;
            }

            return false;
        }

        public static Vector3 ScreenPos2WorldPos(this ScratchUIBehaviour transform, Vector2 screenPos)
        {
            Vector3 worldPos = Vector3.zero;
            var camera = ScratchProgrammingManager.Instance.Canvas.worldCamera;
            switch (ScratchProgrammingManager.Instance.Canvas.renderMode)
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
            var camera = ScratchProgrammingManager.Instance.Canvas.worldCamera;
            switch (ScratchProgrammingManager.Instance.Canvas.renderMode)
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