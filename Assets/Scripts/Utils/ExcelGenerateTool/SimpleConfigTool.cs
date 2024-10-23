using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Utils.ExcelTool.ToLuban
{
    public interface IEData
    {
        public int GetHashKey();
    }
    public sealed class EData
    {
        private IEData m_EData = null;
        public IEData Data => m_EData;
        public EData() { }
        public void SetData(IEData eData)
        {
            m_EData = eData;
        }

    }
    public class EDataTable<T> : IEnumerable<T> where T : IEData
    {
        public Dictionary<int, EData> DataTableDict => m_DataTableDict;
        public Type DataType;

        private Dictionary<int, EData> m_DataTableDict;
        private PropertyInfo[] m_propertyInfos = null;

        public PropertyInfo[] PropertyInfos
        {
            get
            {
                if (m_propertyInfos == null) m_propertyInfos = EDataTableExtension.GetPropertyInfos(DataType);
                return m_propertyInfos;
            }
        }
        protected EDataTable()
        {
            DataType = typeof(T);
        }
        public static async Task<EDataTable<T>?> GetTableAsync(string filePath)
        {
            EDataTable<T> table = new EDataTable<T>();
            bool result = false;
            try
            {
                table.m_DataTableDict = await table.ReadAsync(filePath, (res) =>
                {
                    result = res;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(result.ToString());
                return null;
            }
            return result ? table : null;
        }
        public static EDataTable<T> GetTable(IEnumerable<T> dataSource)
        {
            Dictionary<int, EData> m_tempDict = new Dictionary<int, EData>();
            foreach (T data in dataSource)
            {
                EData edata = new EData();
                edata.SetData(data);
                m_tempDict[data.GetHashKey()] = edata;
            }
            EDataTable<T> table = new EDataTable<T>();
            table.m_DataTableDict = m_tempDict;
            return table;
        }
        public Task ReWriteAsync(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("##var,");
            foreach (var prop in PropertyInfos)
            {
                sb.Append($"{prop.Name},");
            }
            sb.AppendLine("");
            sb.Append("##type,");
            foreach (var prop in PropertyInfos)
            {
                sb.Append($"{EDataTableExtension.GetTypeString(prop.PropertyType)},");
            }
            sb.AppendLine("");
            sb.Append("##,");
            foreach (var prop in PropertyInfos)
            {
                sb.Append($"{EDataTableExtension.GetPropertyAttribute(prop).Des},");
            }
            sb.AppendLine("");
            foreach (var dictKey in m_DataTableDict.Keys)
            {
                EDataSerialize(ref sb, m_DataTableDict[dictKey]);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (fileStream)
            {
                fileStream.Position = 0;
                using (var writer = new StreamWriter(stream: fileStream, Encoding.UTF8))
                {
                    return writer.WriteAsync(sb.ToString());
                }
            }
        }
        public async Task<Dictionary<int, EData>> ReadAsync(string filePath, Action<bool> callback = null)
        {
            if (!File.Exists(filePath))
            {
                callback?.Invoke(false);
                return null;
            }
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            using (fileStream)
            {
                using (var reader = new StreamReader(stream: fileStream, Encoding.UTF8))
                {
                    try
                    {
                        string varStr = await reader.ReadLineAsync();
                        varStr = await reader.ReadLineAsync();
                        string[] typesStrs = varStr.Split(",");

                        for (int i = 0; i < PropertyInfos.Length; i++)
                        {
                            if (EDataTableExtension.GetTypeString(PropertyInfos[i].PropertyType) != typesStrs[i + 1])
                            {
                                callback?.Invoke(false);
                                return null;
                            }
                        }
                        await reader.ReadLineAsync();
                        Dictionary<int, EData> tempEDatas = new Dictionary<int, EData>();
                        while (!reader.EndOfStream)
                        {
                            string result = await reader.ReadLineAsync();

                            EData edata = EDataDeSerialize(result);
                            tempEDatas[edata.GetHashCode()] = edata;
                        }
                        callback?.Invoke(true);
                        return tempEDatas;
                    }
                    catch (Exception e)
                    {
                        callback?.Invoke(false);
                        return null;
                    }
                }
            }
        }
        public void EDataSerialize(ref StringBuilder sb, EData data)
        {
            sb.Append(",");
            for (int i = 0; i < PropertyInfos.Length; i++)
            {
                string temp = $"{PropertyInfos[i].GetValue(data.Data)},";
                sb.Append(temp);
            }
            sb.AppendLine("");
        }
        public EData EDataDeSerialize(string info)
        {
            string[] dataVas = info.Split(",");
            EData data = new EData();
            T obj = Activator.CreateInstance<T>();
            for (int i = 0; i < PropertyInfos.Length; i++)
            {
                PropertyInfos[i].SetValue(obj, EDataTableExtension.GetTypeData(dataVas[i + 1], PropertyInfos[i].PropertyType));
            }
            data.SetData(obj);
            return data;
        }
        public IEnumerator<T> GetEnumerator()
        {
            foreach (int key in m_DataTableDict.Keys)
            {
                yield return (T)m_DataTableDict[key].Data;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EDataProperty : Attribute
    {
        public int Index = -1;
        public string Des = null;
        public EDataProperty(int index = -1, string des = null)
        {
            Index = index;
            Des = des == null ? string.Empty : des;
        }
    }
    public static partial class EDataTableExtension
    {
        public static PropertyInfo[] GetPropertyInfos(Type type)
        {
            return type.GetProperties()
                        .Where(p => p.GetCustomAttribute(typeof(EDataProperty)) != null)
                        .OrderBy(p => ((EDataProperty)p.GetCustomAttribute(typeof(EDataProperty))).Index)
                        .ToArray();
        }
        public static string GetTypeString(Type type)
        {
            if (type == typeof(bool))
            {
                return "bool";
            }
            else if (type == typeof(byte))
            {
                return "byte";
            }
            else if (type == typeof(short))
            {
                return "short";
            }
            else if (type == typeof(int))
            {
                return "int";
            }
            else if (type == typeof(float))
            {
                return "float";
            }
            else if (type == typeof(double))
            {
                return "double";
            }
            else if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(string))
            {
                return "text";
            }
            else if (type == typeof(DateTime))
            {
                return "datetime";
            }
            //TODO 支持其他类型
            throw new Exception("不支持类型");
        }
        public static object GetTypeData(string typeStr, Type type)
        {
            if (type == typeof(bool))
            {
                return bool.Parse(typeStr);
            }
            else if (type == typeof(byte))
            {
                return byte.Parse(typeStr);
            }
            else if (type == typeof(short))
            {
                return short.Parse(typeStr);
            }
            else if (type == typeof(int))
            {
                return int.Parse(typeStr);
            }
            else if (type == typeof(float))
            {
                return float.Parse(typeStr);
            }
            else if (type == typeof(double))
            {
                return double.Parse(typeStr);
            }
            else if (type == typeof(string))
            {
                return typeStr;
            }
            else if (type == typeof(string))
            {
                return typeStr;
            }
            else if (type == typeof(DateTime))
            {
                return DateTime.Parse(typeStr);
            }
            //TODO 支持其他类型
            throw new Exception("不支持类型");
        }
        public static EDataProperty GetPropertyAttribute(PropertyInfo type)
        {
            return type.GetCustomAttribute<EDataProperty>();
        }
    }
    internal class TestData : IEData
    {
        [EDataProperty(0)]
        public int Id { get; set; }
        [EDataProperty(2, "测试名称")]
        public string Name { get; set; }
        [EDataProperty(1)]
        public bool Res { get; set; }

        public int GetHashKey()
        {
            return Id;
        }
    }
    public static class SimpleConfigTool
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/SimpleConfigTool/TestExcelToLuban")]
#endif
        public static async void Test()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "TestData.csv");

            EDataTable<TestData>? table = await EDataTable<TestData>.GetTableAsync(filePath);
            if (table == null)
            {
                Debug.Log("开始生成数据结构...");
                List<TestData> datas = new List<TestData>();
                for (int i = 0; i < 10; i++)
                {
                    var data = new TestData();
                    data.Id = i;
                    data.Name = Guid.NewGuid().ToString();
                    datas.Add(data);
                }
                table = EDataTable<TestData>.GetTable(datas);
                await table.ReWriteAsync(filePath);
            }
            else
            {
                Debug.Log($"表格不NULL{table.DataTableDict.Count}...");
            }
            foreach (TestData data in table)
            {
                Debug.Log($"表格{data.Id}:{data.Name}");
            }
        }
    }
}

