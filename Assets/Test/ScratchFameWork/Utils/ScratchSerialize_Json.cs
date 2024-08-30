using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ScratchFramework
{
    public static class ScratchSerialize_Json
    {
        public static byte[] SerializeData<T>(T data)
        {
            return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)); 
        }
        
        public static T DeserializeData<T>(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return JsonUtility.FromJson<T>(json);
        }
    }
}