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
        public static Block InitializeStruct(Block block)
        {
            return block
                .AddBlockDrag()
                .AddUIVisible();
        }

        private static Block AddBlockDrag(this Block block)
        {
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
                    block.TryAddComponent<BlockDrag_Operation>();
                    break;
                case BlockType.define:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return block;
        }

        private static Block AddUIVisible(this Block block)
        {
            switch (block.Type)
            {
                case BlockType.none:
                    break;
                case BlockType.trigger:
                    var sections = block.Layout.SectionsArray;
                    for (int i = 0; i <   sections.Length; i++)
                    {
                        sections[i].Body.Image.Visible = false;
                    }
                    break;
                case BlockType.simple:
                    break;
                case BlockType.condition:
                    break;
                case BlockType.loop:
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