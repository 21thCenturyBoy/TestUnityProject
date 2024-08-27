using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ScratchFramework
{
    using UnityObject = UnityEngine.Object;
    public static partial class ScratchUtils
    {
        public const string Block_PefixName = "Body";
        public const string Section_PefixName  = "Section";
        public const string SectionHeader_PefixName  = "Header";
        public const string SectionBody_PefixName  = "Body";
        
        public static T GetOrAddComponent<T>(this UnityObject uo) where T : Component
        {
            return uo.GetComponent<T>() ?? uo.AddComponent<T>();
        }
        
        public static void ConvertSimpleBlock(GameObject obj)
        {
            obj.GetOrAddComponent<Block>();
            obj.GetOrAddComponent<BlockLayout>();
            obj.GetOrAddComponent<BlockDrag_Trigger>();

            int childCount = obj.transform.childCount;
            
            for (int i = 0; i < childCount; i++)
            {
                var selection = obj.transform.GetChild(i);

                if (selection.transform.name.Contains(Section_PefixName))
                {
                    selection.gameObject.GetOrAddComponent<BlockSection>();
                    int selectionChildCount = selection.transform.childCount;

                    for (int j = 0; j < selectionChildCount; j++)
                    {
                        var selectionChild = selection.transform.GetChild(j);

                        if (selectionChild.transform.name.Contains(SectionHeader_PefixName))
                        {
                            selectionChild.gameObject.GetOrAddComponent<BlockSectionHeader>();
                        }
                        
                        if (selectionChild.transform.name.Contains(SectionBody_PefixName))
                        {
                            selectionChild.gameObject.GetOrAddComponent<BlockSectionBody>();
                        }
                    }
      
                }
            }
        }
    }
    
}
