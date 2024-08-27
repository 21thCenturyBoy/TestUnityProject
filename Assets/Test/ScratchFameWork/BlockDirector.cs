using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public interface IBlockData
    {
        BlockType GetType();
    }

    public static class BlockDirector
    {
        public static Block AddBlockDrag(Block block) {

            switch (block.Type)
            {
                case BlockType.none:
                    break;
                case BlockType.trigger:
                    block.TryAddComponent<BlockDrag_Trigger>();
                    break;
                case BlockType.simple:
                    block.TryAddComponent<BlockDrag_Simple>();
                    break;
                case BlockType.condition:
                    break;
                case BlockType.loop:
                    block.TryAddComponent<BlockDrag_Simple>();
                    break;
                case BlockType.operation:
                    break;
                case BlockType.define:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return block;
        }
    }

}
