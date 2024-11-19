using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ScratchFramework
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class BlockGuidRefAttribute : System.Attribute
    {
        public BlockGuidRefAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class BlockGuidRefListAttribute : System.Attribute
    {
        public BlockGuidRefListAttribute()
        {
        }
    }

    /// <summary>
    /// 这部分应该改成自动生成
    /// </summary>
    public enum ScratchValueType : byte
    {
        Undefined = 0,
        Boolean = 1,
        Byte = 2,
        Integer = 3,
        Float = 4,
        Vector2 = 5,
        Vector3 = 6,
        EntityRef = 7,
        AssetRef = 8,
    }

    /// <summary>
    /// 这部分应该改成自动生成
    /// </summary>
    public enum ScratchBlockType : int
    {
        __Undefined__ = 0,
        __Event__ = 10000, //事件
        OnCollisionEnter,
        OnCollisionStay,
        OnCollisionExit,
        OnObjectCreated,
        //TODO Event

        __Action__ = 20000, //行为
        ApplyForce,
        DestroyObject,
        //TODO Action

        __Control__ = 30000, //控制
        IfElse,
        RepeatAction,
        StartCountdown,
        //TODO Control

        __Condition__ = 40000, //条件，与或非
        CompareValues,
        //TODO Condition

        __GetValue__ = 50000, //取值
        GetCharacterSpeed,
        GetVectorMagnitude,
        //TODO GetValue

        __Variable__ = 60000, //变量
        IntegerValue,
        VectorValue,
        EntityValue,
        //TODO Variable
    }

    [Serializable]
    public struct BVector2 : IEquatable<BVector2>
    {
        public static readonly BVector2 zero = new BVector2(Vector2.zero);
        public float x;
        public float y;

        public bool Equals(BVector2 other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is BVector2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public static bool operator ==(BVector2 left, BVector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BVector2 left, BVector2 right)
        {
            return !left.Equals(right);
        }

        public BVector2(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
        }

        public BVector2(Vector3 d)
        {
            x = d.x;
            y = d.y;
        }

        public static implicit operator Vector2(BVector2 v)
        {
            return new Vector3(v.x, v.y);
        }

        //  User-defined conversion from double to Digit
        public static implicit operator BVector2(Vector2 v)
        {
            return new BVector2 { x = v.x, y = v.y };
        }

        public static implicit operator Vector3(BVector2 v)
        {
            return new Vector3(v.x, v.y);
        }

        //  User-defined conversion from double to Digit
        public static implicit operator BVector2(Vector3 v)
        {
            return new BVector2 { x = v.x, y = v.y };
        }
    }

    public interface IEngineBlockBaseData
    {
        public bool IsRoot { get; set; }
        public BVector2 CanvasPos { get; set; }
        public FucType FucType { get; }
        public BlockType BlockType { get; }
        public ScratchBlockType Type { get; }
        public int NextBlockGuid { get; set; }
        public int Guid { get; set; }
    }

    public interface IBlockReturnVarGuid
    {
        public void SetReturnValueGuid(int Index, int guid);
        public int GetReturnValueGuid(int Index);
        public int GetReturnValuesLength();
        public int[] GetReturnValues();
    }

    public interface IBlockVarGuid
    {
        public void SetInputValues(int index, string value);
        public string GetInputValue(int index);
        public string[] GetInputValues();

        public int GetVarGuidsLength();
        public void SetVarsGuid(int Index, int guid);
        public int GetVarGuid(int Index);
        public int[] GetVarGuids();
    }

    public class GuidListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            GuidList list = (GuidList)value;
            string jsonString = JsonConvert.SerializeObject(list.ToList());
            writer.WriteValue(jsonString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            GuidList list = new GuidList(JsonConvert.DeserializeObject<List<int>>((string)reader.Value));
            return list;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GuidList);
        }
    }

    [Serializable]
    public sealed class GuidList : IEnumerator<int>, IEquatable<GuidList>
    {
        private List<int> m_List;
        private int m_Index = -1;
        public event Action<int> OnAddGuid;
        public event Action<int> OnRemoveGuid;
        public event Action<int, int> OnUpdateGuid;
        public int Length => m_List.Count;

        public GuidList(params int[] guids)
        {
            m_List = new List<int>(guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                if (!m_List.Contains(guids[i])||guids[i] == ScratchUtils.InvalidGuid)
                {
                    m_List.Add(guids[i]);
                }
            }
        }

        public GuidList(List<int> guids)
        {
            m_List = new List<int>(guids.Count);
            for (int i = 0; i < guids.Count; i++)
            {
                if (!m_List.Contains(guids[i])||guids[i] == ScratchUtils.InvalidGuid)
                {
                    m_List.Add(guids[i]);
                }
            }
        }

        public List<int> ToList()
        {
            return m_List.ToList();
        }

        public static GuidList CreateEmptyGuidList(int len = 0)
        {
            GuidList newGuidList = new GuidList();
            if (len == 0) return newGuidList;
            else
            {
                newGuidList.m_List = new List<int>(new int[len]);
                return newGuidList;
            }
        }

        public GuidList()
        {
            m_List = new List<int>();
        }

        public void Repleace(GuidList list)
        {
            m_List = new List<int>(list.m_List);
        }

        public int this[int index]
        {
            get { return m_List[index]; }
            set
            {
                if (value != ScratchUtils.InvalidGuid && m_List.Contains(value))
                {
                    return;
                }

                int old = m_List[index];
                m_List[index] = value;
                OnUpdateGuid?.Invoke(old, value);
            }
        }

        public bool Add(int guid)
        {
            if (m_List.Contains(guid) && guid != ScratchUtils.InvalidGuid)
            {
                return false;
            }

            m_List.Add(guid);
            OnAddGuid?.Invoke(guid);
            return true;
        }

        public int FindIndex(int guid)
        {
            return m_List.IndexOf(guid);
        }

        public bool MoveNext()
        {
            m_Index++;
            return m_Index < m_List.Count;
        }

        public bool Equals(GuidList other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return m_List.SequenceEqual(other.m_List);
        }

        public override bool Equals(object obj)
        {
            return obj is GuidList other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (m_List != null ? m_List.GetHashCode() : 0);
        }

        public static bool operator ==(GuidList left, GuidList right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) || ReferenceEquals(null, right)) return false;

            return left.m_List.SequenceEqual(right.m_List);
        }

        public static bool operator !=(GuidList left, GuidList right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return false;
            if (ReferenceEquals(null, left) || ReferenceEquals(null, right)) return true;

            return !left.m_List.SequenceEqual(right.m_List);
        }

        public void Reset()
        {
            m_Index = -1;
        }

        public int Current { get; }
        object IEnumerator.Current => Current;

        public void Clear()
        {
            for (int i = 0; i < m_List.Count; i++)
            {
                if (m_List[i] == ScratchUtils.InvalidGuid) continue;
                OnRemoveGuid?.Invoke(m_List[i]);
            }

            m_List.Clear();
        }

        public void Dispose()
        {
            Clear();
            m_List = null;
        }

        public IEnumerator<int> GetEnumerator()
        {
            while (MoveNext()) yield return m_List[m_Index];
        }
    }

    public interface IEngineBlockBranch
    {
        /// <summary>
        /// True,False == 1
        /// If ... else ... == 1
        /// if ... else if ... else ... == 2
        /// if ... else if ... else if ... else ... == 3
        /// </summary>
        [BlockGuidRefList]
        public GuidList Branch_OperationGuids { get; set; }

        /// <summary>
        /// True,False == 2
        /// If ... else ... == 2
        /// if ... else if ... else ... == 3
        /// if ... else if ... else if ... else ... == 4
        /// </summary>
        /// <returns></returns>
        [BlockGuidRefList]
        public GuidList Branch_BlockGuids { get; set; }
    }

    public interface IEngineBlockTriggerBase : IEngineBlockBaseData, IBlockReturnVarGuid
    {
    }

    public interface IEngineBlockConditionBase : IEngineBlockBaseData, IEngineBlockBranch
    {
    }

    public interface IEngineBlockLoopBase : IEngineBlockBaseData, IBlockVarGuid
    {
        [BlockGuidRef] public int ChildRootGuid { get; set; }
    }

    public interface IEngineBlockSimpleBase : IEngineBlockBaseData, IBlockVarGuid
    {
    }

    public interface IEngineBlockOperationBase : IEngineBlockBaseData, IBlockVarGuid
    {
    }

    public interface IEngineBlockVariableBase : IEngineBlockBaseData
    {
        public ScratchValueType ValueType { get; }
        public string VariableName { get; set; }
        public object VariableValue { get; set; }
    }

    public interface IEngineCoreInterface
    {
        string GetEngineVersion();

        /// <summary>
        /// 获取所有引擎Block数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, IEngineBlockBaseData> GetAllBlocks();

        /// <summary>
        /// 查询引擎Block数据
        /// </summary>
        /// <returns></returns>
        public IEngineBlockBaseData GetBlocksDataRef(int guid);

        public bool ChangeBlockData(Block block, Transform orginParentTrans, Transform newParentTrans);

        /// <summary>
        /// 修复并设置BlockData 位置
        /// </summary>
        public bool TryFixedBlockBaseDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos);

        /// <summary>
        /// 更新BlockData 位置
        /// </summary>
        public bool UpdateDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos);

        /// <summary>
        /// 删除DeleteBlock
        /// </summary>
        public void DeleteBlock(IEngineBlockBaseData block, bool recursion = true);

        /// <summary>
        /// 根据类型创建方块
        /// </summary>
        /// <param name="scratchType"></param>
        /// <param name="isAdd">是否是新创建  （生成新的guid并添加如block池子）</param>
        /// <returns></returns>
        public IEngineBlockBaseData CreateBlock(ScratchBlockType scratchType, bool isAdd = true);

        /// <summary>
        /// 遍历方块
        /// </summary>
        /// <param name="rootGuid"></param>
        /// <param name="function"></param>
        public IEngineBlockBaseData FindPreBlock(int rootGuid, int CurGuid);

        /// <summary>
        /// 生成Block
        /// </summary>
        /// <returns></returns>
        public void GenerateBlocks(string filepath = null, Action<List<Block>> callback = null);

        public void SaveBlocks(string filepath = null, Action<bool> callback = null);

        /// <summary>
        /// 变量值转字符串
        /// </summary>
        /// <param name="blockBase"></param>
        /// <param name="value"></param>
        public bool VariableValue2String(IEngineBlockVariableBase blockBase, out string value);

        /// <summary>
        /// 字符串转变量值
        /// </summary>
        /// <param name="blockBase"></param>
        /// <param name="value"></param>
        public bool String2VariableValueTo(IEngineBlockVariableBase blockBase, string value);

#if UNITY_EDITOR

        /// <summary>
        /// 编辑器创建Block文件
        /// </summary>
        /// <param name="blockCreateName"></param>
        /// <returns></returns>
        public bool BlockCreateCSFile(string blockCreateName);
#endif
    }
}