using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ScratchFramework
{
    /// <summary>
    /// 单元测试引擎核心
    /// </summary>
    public class TestEngineCore : IEngineCoreInterface
    {
        private Dictionary<int, IEngineBlockBaseData> m_blocks = new Dictionary<int, IEngineBlockBaseData>();
        private Dictionary<int, IEngineBlockBaseData> m_RootBlockDatas = new Dictionary<int, IEngineBlockBaseData>();

        public string GetEngineVersion()
        {
            return string.Empty;
        }

        public Dictionary<int, IEngineBlockBaseData> GetAllBlocksRef()
        {
            return m_blocks;
        }

        public Dictionary<int, IEngineBlockBaseData> GetRootBlocks()
        {
            return m_RootBlockDatas;
        }

        public IEngineBlockBaseData GetBlocksDataRef(int guid)
        {
            if (m_blocks.ContainsKey(guid)) return m_blocks[guid];
            return null;
        }

        public bool CreateBlocksData(IEngineBlockBaseData data)
        {
            if (m_blocks.ContainsKey(data.Guid))
            {
                return false;
            }

            m_blocks[data.Guid] = data;
            return true;
        }

        public bool ClearBlocksData(IEngineBlockBaseData data)
        {
            if (!m_blocks.ContainsKey(data.Guid))
            {
                return false;
            }
            
            return m_blocks.Remove(data.Guid);
        }

        private readonly string tempJsonfileUrl = "file://" + tempJsonfilePath;


        IEnumerator GetJsonFile(string pathUrl, Action<Stream> callback = null)
        {
            WWW www = new WWW(pathUrl);
            yield return www;

            if (www.isDone)
            {
                if (www.bytes != null)
                {
                    var stream = new MemoryStream(www.bytes);
                    callback?.Invoke(stream);
                }
            }

            yield break;
        }


        private bool TryDeserializeBlockDatas(Stream stream)
        {
            Dictionary<int, IEngineBlockBaseData> allBlockDatas = new Dictionary<int, IEngineBlockBaseData>();

            HashSet<Block> blocks = new HashSet<Block>();
            List<IEngineBlockBaseData> blockDatas = null;
            m_RootBlockDatas.Clear();
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.TypeNameHandling = TypeNameHandling.All;
                    settings.Converters = new List<JsonConverter> { new GuidListConverter() };
                    blockDatas = JsonConvert.DeserializeObject<List<IEngineBlockBaseData>>(json, settings);


                    if (blockDatas != null)
                    {
                        ScratchUtils.RefreshDataGuids(blockDatas);

                        for (int i = 0; i < blockDatas.Count; i++)
                        {
                            allBlockDatas[blockDatas[i].Guid] = blockDatas[i];
                            if (blockDatas[i].IsRoot)
                            {
                                m_RootBlockDatas[blockDatas[i].Guid] = blockDatas[i];
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    return false;
                }

                if (blockDatas == null)
                {
                    blockDatas = new List<IEngineBlockBaseData>();
                }

                m_blocks = allBlockDatas;

                for (int i = 0; i < blockDatas.Count; i++)
                {
                    var block = blockDatas[i];
                    if (block.IsRoot)
                    {
                        var blockview = ScratchUtils.DrawNodeRoot(block, BlockCanvasManager.Instance.RectTrans, -1);
                        for (int j = 0; j < blockview.Count; j++)
                        {
                            blocks.Add(blockview[j]);
                        }
                    }
                }
                
                ScratchUtils.FixedBindOperation(blocks);

                return true;
            }
        }

        public void GenerateBlocks(string filepath = null, Action<List<Block>> callback = null)
        {
            List<Block> blocks = new List<Block>();
            List<IEngineBlockBaseData> blockDatas = null;

            if (string.IsNullOrEmpty(filepath))
            {
                ScratchEngine.Instance.StartCoroutine(GetJsonFile(tempJsonfileUrl, (stream) => { TryDeserializeBlockDatas(stream); }));
            }
            else
            {
                ScratchEngine.Instance.StartCoroutine(GetJsonFile(filepath, (stream) => { TryDeserializeBlockDatas(stream); }));
            }
        }

        #region 生成预支存储数据

        private static readonly string tempJsonfilePath = Application.streamingAssetsPath + "/TempCanvas/TestCanvas.json";

        public void SaveBlocks(string filepath = null, Action<bool> callback = null)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = tempJsonfilePath;
            }

            if (File.Exists(filepath)) File.Delete(filepath);
            FileStream stream = new FileStream(filepath, FileMode.CreateNew);
            using (stream)
            {
                List<IEngineBlockBaseData> blockDatas = new List<IEngineBlockBaseData>();
                foreach (var block in m_blocks)
                {
                    blockDatas.Add(block.Value);
                }

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.All;
                settings.Converters = new List<JsonConverter> { new GuidListConverter() };
                settings.Formatting = Formatting.Indented;

                var json = JsonConvert.SerializeObject(blockDatas, settings);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public bool VariableValue2String(IEngineBlockVariableBase blockBase, out string value)
        {
            value = String.Empty;

            switch (blockBase.ValueType)
            {
                case ScratchValueType.Undefined:
                    break;
                case ScratchValueType.Boolean:
                    if (blockBase.VariableValue == null)
                    {
                        value = bool.FalseString;
                        return false;
                    }

                    break;
                case ScratchValueType.Byte:
                    if (blockBase.VariableValue == null)
                    {
                        value = "0";
                        return false;
                    }

                    break;
                case ScratchValueType.Integer:
                    if (blockBase.VariableValue == null)
                    {
                        value = "0";
                        return false;
                    }

                    break;
                case ScratchValueType.Float:
                    if (blockBase.VariableValue == null)
                    {
                        value = "0.00";
                        return false;
                    }

                    break;
                case ScratchValueType.Vector2:
                    if (blockBase.VariableValue == null)
                    {
                        value = Vector2.zero.ToString();
                        return false;
                    }

                    break;
                case ScratchValueType.Vector3:
                    if (blockBase.VariableValue == null)
                    {
                        value = Vector3.zero.ToString();
                        return false;
                    }

                    break;
                case ScratchValueType.EntityRef:
                    break;
                case ScratchValueType.AssetRef:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        private bool TryParseVector3(string value, out Vector3 result)
        {
            result = Vector3.zero;
            value = value.Trim(new char[] { '(', ')' });
            string[] sArray = value.Split(',');
            if (sArray.Length != 3) return false;
            else
            {
                result = new Vector3(
                    float.Parse(sArray[0]),
                    float.Parse(sArray[1]),
                    float.Parse(sArray[2]));
                return true;
            }
        }

        private bool TryParseVector2(string value, out Vector2 result)
        {
            result = Vector2.zero;
            value = value.Trim(new char[] { '(', ')' });
            string[] sArray = value.Split(',');
            if (sArray.Length != 2) return false;
            else
            {
                result = new Vector2(
                    float.Parse(sArray[0]),
                    float.Parse(sArray[1]));
                return true;
            }
        }

        public bool String2VariableValueTo(IEngineBlockVariableBase blockBase, string value)
        {
            switch (blockBase.ValueType)
            {
                case ScratchValueType.Undefined:
                    break;
                case ScratchValueType.Boolean:
                    if (bool.TryParse(value, out bool boolresult))
                    {
                        blockBase.VariableValue = boolresult;
                        return true;
                    }

                    break;
                case ScratchValueType.Byte:
                    if (byte.TryParse(value, out byte byteresult))
                    {
                        blockBase.VariableValue = byteresult;
                        return true;
                    }

                    break;
                case ScratchValueType.Integer:
                    if (int.TryParse(value, out int intresult))
                    {
                        blockBase.VariableValue = intresult;
                        return true;
                    }

                    break;
                case ScratchValueType.Float:
                    if (float.TryParse(value, out float floatresult))
                    {
                        blockBase.VariableValue = floatresult;
                        return true;
                    }

                    break;
                case ScratchValueType.Vector2:
                    if (TryParseVector2(value, out Vector2 vector2result))
                    {
                        blockBase.VariableValue = vector2result;
                        return true;
                    }

                    break;
                case ScratchValueType.Vector3:
                    if (TryParseVector3(value, out Vector3 vector3result))
                    {
                        blockBase.VariableValue = vector3result;
                        return true;
                    }

                    break;
                case ScratchValueType.EntityRef:
                    break;
                case ScratchValueType.AssetRef:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        #endregion
    }
}