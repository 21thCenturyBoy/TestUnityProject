using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScratchFramework
{
    public static class ScratchUtils_Editor
    {
        [MenuItem("GameObject/Scratch/Convert Simple Block", false, 80)]
        public static void Editor_Convert_Simple_Block()
        {
            if (Selection.activeGameObject == null)return;

            ScratchUtils.ConvertSimpleBlock(Selection.activeGameObject);
        }
    }
}