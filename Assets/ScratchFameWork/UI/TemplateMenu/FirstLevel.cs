using UnityEngine;

namespace ScratchFramework
{
    public enum FirstLevelType
    {
        None = -1,
        Event,
        Action,
        Control,
        Condition,
        GetValue,
        Variable,
        Custom,
        Search,
    }

    public class FirstLevel : MonoBehaviour
    {
        public FirstLevelType Type;
    }
}