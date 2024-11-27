using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    [Serializable]
    public sealed class BGuid : IEquatable<BGuid>
    {
        public bool Equals(BGuid other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return m_Guid == other.m_Guid;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is BGuid other && Equals(other);
        }

        public override int GetHashCode()
        {
            return m_Guid;
        }

        public static bool operator ==(BGuid left, BGuid right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BGuid left, BGuid right)
        {
            return !Equals(left, right);
        }

        public static readonly BGuid Empty = new BGuid();

        public static BGuid CreateGuid()
        {
            BGuid guid = new BGuid();
            return guid;
        }

        private BGuid()
        {
        }

        private int m_Guid = ScratchUtils.InvalidGuid;
        
        public ref int GetGuid()
        {
            return ref m_Guid;
        }
        public int SetGuid(int guid, out BGuid BGuid)
        {
            if (guid == ScratchUtils.InvalidGuid)
            {
                BGuid = Empty; 
            }
            else
            {
                BGuid = CreateGuid();
                BGuid.m_Guid = guid;
            }
   
            return m_Guid;
        }
    }
}