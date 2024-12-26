using UnityEngine;

namespace ScratchFramework
{
    public class Block_GhostBlock : Block
    {
        public override BlockType Type
        {
            get => BlockType.none;
            set => base.Type = BlockType.none;
        }

        public override bool Initialize()
        {
            Type = BlockType.none;

            return base.Initialize();
        }

        public void InsertSpot(Transform parentTrans, int index = 0)
        {
            this.SetParent(parentTrans, index);

            //TOOD Set CSS Size
            RectTrans.localScale = Vector3.one;
        }
    }
}