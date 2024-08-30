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
            //TODO Data
            var obj = GameObject.Instantiate(block.gameObject, BlockCanvasManager.Instance.transform);
            return obj.GetComponent<Block>();
        }

        public static void DestroyBlock(Block block)
        {
            //TODO Data
            GameObject.Destroy(block.gameObject);
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

        public static SerializeMode SerializeMode = SerializeMode.Json;

        public static byte[] SerializeData<T>(this IScratchData scratchData, T data)
        {
            switch (SerializeMode)
            {
                case SerializeMode.Json:
                    return ScratchSerialize_Json.SerializeData(data);
                    break;
                case SerializeMode.MessagePack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
        
        public static T DeserializeData<T>(this IScratchData scratchData, byte[] data)
        {
            switch (SerializeMode)
            {
                case SerializeMode.Json:
                    return ScratchSerialize_Json.DeserializeData<T>(data);
                    break;
                case SerializeMode.MessagePack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return default;
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

        internal static void DrawScreenEllipse(Vector2 center, float xRadius, float yRadius, Color color, int smooth = 50)
        {
            // if (s_material == null)
            // {
            //     Material s_material = new Material(Shader.Find("Unlit/Color"));
            //     // s_material = new Material(Shader.Find("UI/Default"));
            // }
            Material s_material = new Material(Shader.Find("Unlit/Color"));
            s_material.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            s_material.SetColor("_Color", color);
            s_material.renderQueue = 4000;

            for (int i = 0; i < smooth; ++i)
            {
                int nextStep = (i + 1) % smooth;
                GL.Vertex3((center.x + xRadius * Mathf.Cos(2 * Mathf.PI / smooth * i)) / Screen.width,
                    (center.y + yRadius * Mathf.Sin(2 * Mathf.PI / smooth * i)) / Screen.height, 0);
                GL.Vertex3((center.x + xRadius * Mathf.Cos(2 * Mathf.PI / smooth * nextStep)) / Screen.width,
                    (center.y + yRadius * Mathf.Sin(2 * Mathf.PI / smooth * nextStep)) / Screen.height, 0);
            }

            GL.End();
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