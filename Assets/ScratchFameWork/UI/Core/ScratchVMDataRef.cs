using System;
using System.Collections.Generic;

namespace ScratchFramework
{
    public class ScratchVMDataRef<T> : IScratchVMDataRef, IScratchRefreshRef, IEquatable<ScratchVMDataRef<T>> where T : ScratchVMData
    {
        public static readonly ScratchVMDataRef<T> NULLRef = new ScratchVMDataRef<T>(null);

        private T m_RefData = null;
        public System.Type RefType => typeof(T);

        private int m_IntPtr = ScratchVMData.UnallocatedId;

        public int RefIdPtr
        {
            get
            {
                if (m_RefData != null) return m_RefData.IdPtr;
                else return m_IntPtr;
            }
        }

        public bool InVaildPtr { get; private set; } = false;

        protected ScratchVMDataRef(T vmData)
        {
            m_RefData = vmData;
        }

        public static ScratchVMDataRef<T> CreateInVaildPtr<T>(int intPtr = ScratchVMData.UnallocatedId) where T : ScratchVMData
        {
            return new ScratchVMDataRef<T>(intPtr);
        }

        public static ScratchVMDataRef<T> CreateVMDataRef<T>(T vmData) where T : ScratchVMData
        {
            return new ScratchVMDataRef<T>(vmData);
        }

        protected ScratchVMDataRef(int id)
        {
            m_IntPtr = id;
            InVaildPtr = true;
        }

        public virtual T GetData(bool refId = true)
        {
            if (m_RefData != null) return m_RefData;
            else
            {
                if (refId) return GetRefData(m_IntPtr);
                else return m_RefData;
            }
        }

        private T GetRefData(int id)
        {
            var vmData = BlockDataUIManager.Instance.GetDataById(id);
            if (vmData is T tData)
            {
                return tData;
            }

            return null;
        }

        public void RefreshRef(Dictionary<int, int> refreshDic)
        {
            if (refreshDic.ContainsKey(m_IntPtr))
            {
                T data = GetRefData(refreshDic[m_IntPtr]);
                m_RefData = data;
                m_IntPtr = refreshDic[m_IntPtr];
            }

            InVaildPtr = false;
        }

        public static bool operator ==(ScratchVMDataRef<T> left, ScratchVMDataRef<T> right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(right, NULLRef)) return true;
            if (ReferenceEquals(null, right) && ReferenceEquals(left, NULLRef)) return true;

            return Equals(left, right);
        }

        public static bool operator !=(ScratchVMDataRef<T> left, ScratchVMDataRef<T> right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(right, NULLRef)) return false;
            if (ReferenceEquals(null, right) && ReferenceEquals(left, NULLRef)) return false;

            return !Equals(left, right);
        }

        public bool Equals(ScratchVMDataRef<T> other)
        {
            if (ReferenceEquals(null, other) && ReferenceEquals(this, NULLRef)) return true;
            if (ReferenceEquals(null, this) && ReferenceEquals(other, NULLRef)) return true;
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RefIdPtr == other.RefIdPtr;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj) && ReferenceEquals(this, NULLRef)) return true;
            if (ReferenceEquals(null, this) && ReferenceEquals(obj, NULLRef)) return true;
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ScratchVMDataRef<T>)obj);
        }

        public override int GetHashCode()
        {
            return RefIdPtr;
        }

        public override string ToString()
        {
            return $"{nameof(RefIdPtr)}: {RefIdPtr}";
        }
    }
}