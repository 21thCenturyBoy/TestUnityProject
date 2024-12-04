using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ScratchFramework
{
    /// <summary>
    /// 单元测试引擎核心
    /// </summary>
    public class TestEngineCore : IEngineCoreInterface
    {
        private readonly string tempJsonfileUrl = "file://" + tempJsonfilePath;
        private static readonly string tempJsonfilePath = Application.streamingAssetsPath + "/TempCanvas/TestCanvas.json";

        public void LoadCanvasGroup(Action<EngineBlockCanvasGroup> callback = null)
        {
            IEnumerator GetJsonFile(string pathUrl, Action<Stream> callback = null)
            {
                UnityWebRequest request = UnityWebRequest.Get(pathUrl);
                request.downloadHandler = new DownloadHandlerBuffer();

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Get Error: {request.error}");
                    callback?.Invoke(null);
                }
                else
                {
                    if (request.downloadHandler.data != null)
                    {
                        var stream = new MemoryStream(request.downloadHandler.data);
                        callback?.Invoke(stream);
                    }
                }
            }

            ScratchEngine.Instance.StartCoroutine(GetJsonFile(tempJsonfileUrl, (stream) =>
            {
                EngineBlockCanvasGroup group = null;

                if (stream == null)
                {
                    group = new EngineBlockCanvasGroup();
                    group.GlobalCanvas = EngineBlockCanvasGroup.CreateNewCanvas(nameof(EngineBlockCanvasGroup.GlobalCanvas));
                }
                else
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        try
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings();
                            settings.TypeNameHandling = TypeNameHandling.All;
                            settings.Converters = new List<JsonConverter> { new GuidListConverter() };
                            group = JsonConvert.DeserializeObject<EngineBlockCanvasGroup>(json, settings);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.ToString());
                            return;
                        }

                        if (group == null)
                        {
                            group = new EngineBlockCanvasGroup();
                            group.GlobalCanvas = EngineBlockCanvasGroup.CreateNewCanvas(nameof(EngineBlockCanvasGroup.GlobalCanvas));
                        }
                    }
                }

                callback?.Invoke(group);
            }));
        }


        public void SaveCanvasGroup(EngineBlockCanvasGroup group, Action<bool> callback = null)
        {
            var filepath = tempJsonfilePath;
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = tempJsonfilePath;
            }

            if (File.Exists(filepath)) File.Delete(filepath);
            FileStream stream = new FileStream(filepath, FileMode.CreateNew);
            using (stream)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.All;
                settings.Converters = new List<JsonConverter> { new GuidListConverter() };
                settings.Formatting = Formatting.Indented;
                
                var json = JsonConvert.SerializeObject(group, settings);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                stream.Write(bytes, 0, bytes.Length);
            }

            callback?.Invoke(true);
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
                        return true;
                    }

                    break;
                case ScratchValueType.Byte:
                    if (blockBase.VariableValue == null)
                    {
                        value = "0";
                        return true;
                    }

                    break;
                case ScratchValueType.Integer:
                    if (blockBase.VariableValue == null)
                    {
                        value = "0";
                        return true;
                    }

                    break;
                case ScratchValueType.Float:
                    if (blockBase.VariableValue == null)
                    {
                        value = "0.00";
                        return true;
                    }

                    break;
                case ScratchValueType.Vector2:
                    if (blockBase.VariableValue == null)
                    {
                        value = Vector2.zero.ToString();
                        return true;
                    }

                    break;
                case ScratchValueType.Vector3:
                    if (blockBase.VariableValue == null)
                    {
                        value = Vector3.zero.ToString();
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
    }
}