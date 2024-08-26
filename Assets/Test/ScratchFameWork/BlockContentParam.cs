using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ScratchFramework
{


    public class BlockContentParam_Data : ScratchVMData
    {
        public ParamType Type;

        public readonly BindableProperty<string> AccountInput = new BindableProperty<string>();
    }

    public class BlockContentParam : ScratchUIBehaviour<BlockContentParam_Data>
    {
        // public override void Initialize(BlockContentParam_Data context = null)
        // {
        //     base.Initialize(context);
        // }
   

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }
    }
}