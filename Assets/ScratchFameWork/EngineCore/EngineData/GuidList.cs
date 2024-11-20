using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ScratchFramework
{
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
                if (!m_List.Contains(guids[i]) || guids[i] == ScratchUtils.InvalidGuid)
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
                if (!m_List.Contains(guids[i]) || guids[i] == ScratchUtils.InvalidGuid)
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
}