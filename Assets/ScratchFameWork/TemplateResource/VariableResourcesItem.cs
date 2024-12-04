using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ScratchFramework
{
    public class VariableResourcesItem : ResourcesItem
    {
        public override void CreateBlockTemplate()
        {
            if (Data != null)
            {
                var block = Data.CreateBlock(null);

                IBlockHeaderVariableLabel variableLabel =  block.GetComponentInChildren<IBlockHeaderVariableLabel>();
                variableLabel.GetVariableData().VariableRef = block.GetEngineBlockData().Guid.ToString();
                
                ScratchResourcesManager.Instance.RefreshResources();
            }
        }
    }
}