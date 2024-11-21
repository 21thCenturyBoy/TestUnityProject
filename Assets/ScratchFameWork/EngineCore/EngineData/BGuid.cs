using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    [Serializable]
    public sealed class BGuid
    {
        private int m_Guid = ScratchUtils.InvalidGuid;
        public event Action<int, int> OnUpdateGuid;
        
        public int GetGuid()
        {
            return m_Guid;
        }
        public int SetGuid(int guid)
        {
            m_Guid = guid;
            OnUpdateGuid?.Invoke(m_Guid, guid);
            return m_Guid;
        }
    }
}