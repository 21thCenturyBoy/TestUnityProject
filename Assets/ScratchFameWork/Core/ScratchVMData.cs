using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ScratchFramework
{
    public partial class ScratchVMData : INotifyPropertyChanged
    {
        [Newtonsoft.Json.JsonIgnore]
        [NonSerialized] 
        protected readonly Guid m_guid = Guid.Empty;
        
        [Newtonsoft.Json.JsonIgnore]
        public Guid Guid => m_guid;

        public const int UnallocatedId = 0;
        public int IdPtr { get; set; } = UnallocatedId;

        protected ScratchVMData()
        {
            m_guid = Guid.NewGuid();
            ScratchDataManager.Instance.AddData(this);
        }

        public ScratchVMDataRef<T> CreateRef<T>() where T : ScratchVMData
        {
            if (this is T Tdata)
            {
                return ScratchVMDataRef<T>.CreateVMDataRef(Tdata);
            }

            return default;
        }

        public ScratchVMDataRef<ScratchVMData> CreateRef()
        {
            return CreateRef<ScratchVMData>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public override string ToString()
        {
            return $"[{IdPtr}]";
            // return $"{nameof(Guid)}: {Guid}";
        }
    }
}