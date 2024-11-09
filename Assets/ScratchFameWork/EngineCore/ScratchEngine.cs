using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    /// <summary>
    /// 单元测试引擎核心
    /// </summary>
    public class TestEngineCore : IEngineCoreInterface
    {
        Dictionary<int, IEngineBlockBaseData> m_blocks = new Dictionary<int, IEngineBlockBaseData>();

        public Dictionary<int, IEngineBlockBaseData> GetAllBlocks()
        {
            return m_blocks;
        }

        public IEngineBlockBaseData GetBlocksDataRef(int guid)
        {
            if (m_blocks.ContainsKey(guid)) return m_blocks[guid];

            return null;
        }

        public void ChangeBlockData(Block block, Transform orginParentTrans, Transform newParentTrans)
        {
            // var blockData = block.GetEngineBlockData();
            //
            // if (orginParentTrans != null)
            // {
            //     //清理之前的关系
            //     var oldTag = orginParentTrans.GetComponent<IScratchSectionChild>();
            //     if (oldTag != null)
            //     {
            //         BlockSection oldParentSection = oldTag.GetSection() as BlockSection;
            //         Block oldParentBlock = oldParentSection.Block;
            //         IEngineBlockBaseData oldParentBlockBase = oldParentSection.Block.GetEngineBlockData();
            //
            //         switch (oldParentBlockBase.ClassName) //Old Parent Block
            //         {
            //             case ScratchClassName.Trigger:
            //             {
            //                 //Header 理论上不存在清理数据，因为全是返回值
            //
            //                 //Body
            //                 if (oldParentBlockBase.NextBlock.Guid == blockData.Guid)
            //                 {
            //                     var preBlock = ScratchEngine.Instance.Core.FindPreBlock(oldParentBlock.GetEngineBlockData().Guid, blockData.Guid);
            //                     if (preBlock != null)
            //                     {
            //                         preBlock.NextBlockGuid = blockData.NextBlockGuid;
            //                     }
            //                 }
            //             }
            //                 break;
            //             case ScratchClassName.Condition:
            //             {
            //                 var parentCondition = oldParentBlockBase as IEngineBlockConditionBase;
            //
            //                 //0 Header
            //                 if (block.FindVarIndex(parentCondition, out var index))
            //                 {
            //                     //Header上的变量
            //                     parentCondition.SetVarsGuid(index, -1);
            //                     break;
            //                 }
            //
            //                 //True 分支
            //                 if (parentCondition.TrueBlockGuid == blockData.Guid)
            //                 {
            //                     parentCondition.TrueBlockGuid = blockData.NextBlockGuid;
            //                 }
            //
            //                 var preBlock = ScratchEngine.Instance.Core.FindPreBlock(parentCondition.TrueBlockGuid, blockData.Guid);
            //                 if (preBlock != null)
            //                 {
            //                     preBlock.NextBlockGuid = blockData.NextBlockGuid;
            //                     break;
            //                 }
            //
            //                 //False 分支
            //                 if (parentCondition.FalseBlockGuid == blockData.Guid)
            //                 {
            //                     parentCondition.FalseBlockGuid = blockData.NextBlockGuid;
            //                 }
            //
            //                 preBlock = ScratchEngine.Instance.Core.FindPreBlock(parentCondition.FalseBlockGuid, blockData.Guid);
            //                 if (preBlock != null)
            //                 {
            //                     preBlock.NextBlockGuid = blockData.NextBlockGuid;
            //                     break;
            //                 }
            //             }
            //                 break;
            //             case ScratchClassName.Loop:
            //             {
            //                 var parentLoop = oldParentBlockBase as IEngineBlockLoopBase;
            //
            //                 //0 Header
            //                 if (block.FindVarIndex(parentLoop, out var index))
            //                 {
            //                     //Header上的变量
            //                     parentLoop.SetVarsGuid(index, -1);
            //                     break;
            //                 }
            //
            //                 //0 Body
            //                 if (parentLoop.ChildRootGuid == blockData.Guid)
            //                 {
            //                     parentLoop.ChildRootGuid = blockData.NextBlockGuid;
            //                     break;
            //                 }
            //
            //                 var preBlock = ScratchEngine.Instance.Core.FindPreBlock(parentLoop.ChildRootGuid, blockData.Guid);
            //                 if (preBlock != null)
            //                 {
            //                     preBlock.NextBlockGuid = blockData.NextBlockGuid;
            //                     break;
            //                 }
            //             }
            //                 break;
            //             case ScratchClassName.Simple:
            //             {
            //                 var parentSimple = oldParentBlockBase as IEngineBlockSimpleBase;
            //
            //                 //0 Header
            //                 if (block.FindVarIndex(parentSimple, out var index))
            //                 {
            //                     //Header上的变量
            //                     parentSimple.SetVarsGuid(index, -1);
            //                     break;
            //                 }
            //             }
            //                 break;
            //             case ScratchClassName.Operation:
            //             {
            //                 var parentOperation = oldParentBlockBase as IEngineBlockOperationBase;
            //
            //                 //Var 1
            //                 if (parentOperation.Variable1Guid == blockData.Guid)
            //                 {
            //                     parentOperation.Variable1Guid = -1;
            //                 }
            //                 //Var 2
            //                 else
            //                 {
            //                     parentOperation.Variable2Guid = -1;
            //                 }
            //             }
            //                 break;
            //             case ScratchClassName.Values:
            //             {
            //                 var parentValues = oldParentBlockBase as IEngineBlockVariableBase;
            //
            //                 //0 Header
            //                 if (block.FindVarIndex(parentValues, out var index))
            //                 {
            //                     //Header上的变量
            //                     parentValues.SetVarsGuid(index, -1);
            //                     break;
            //                 }
            //             }
            //                 break;
            //             default:
            //                 break;
            //         }
            //     }
            //
            //     blockData.UnBindEntity(frame);
            // }
            //
            // //建立新的关系
            // var tag = block.GetComponentInParent<IScratchSectionChild>();
            // if (tag != null)
            // {
            //     var parentSection = tag.GetSection() as BlockSection;
            //     var parentBlock = parentSection.Block;
            //     var ParentBlockBase = parentBlock.GetEngineBlockData();
            //     var parentScratchEntity = ParentBlockBase.MountEntity;
            //
            //
            //     KoalaScratchUtil.Instance.BindBlockWithEntity(frame, blockData, parentScratchEntity);
            //
            //     switch (ParentBlockBase.ClassName)
            //     {
            //         case ScratchClassName.Trigger:
            //         {
            //             var parentTrigger = parentBlock.GetEngineBlockData() as IEngineBlockTriggerBase;
            //
            //             if (block.IsOnBody(out var body, out var bodyindex))
            //             {
            //                 var siblingIndex = bodyindex;
            //                 if (siblingIndex == 0)
            //                 {
            //                     blockData.NextBlockGuid = ParentBlockBase.NextBlockGuid;
            //                     ParentBlockBase.NextBlockGuid = blockData.Guid;
            //                 }
            //                 else
            //                 {
            //                     if (block.GetPreBlock(out var preblock, out var preblockIndex))
            //                     {
            //                         block.GetEngineBlockData().NextBlockGuid = preblock.GetEngineBlockData().NextBlockGuid;
            //                         preblock.GetEngineBlockData().NextBlockGuid = blockData.Guid;
            //                     }
            //                     else
            //                     {
            //                         blockData.NextBlockGuid = ParentBlockBase.NextBlockGuid;
            //                         ParentBlockBase.NextBlockGuid = blockData.Guid;
            //                     }
            //                 }
            //             }
            //
            //             if (block.IsOnHeader(out var header, out var headindex))
            //             {
            //                 if (block.VariableLabel != null)
            //                 {
            //                     if (block.IsReturnValue)
            //                     {
            //                         var sections = parentBlock.GetChildSection();
            //                         var headerOperations = sections[0].Header.GetHeaderOperation();
            //                         int variableLen = ParentBlockBase.GetReturnValueLength();
            //                         if (variableLen != headerOperations.Length)
            //                         {
            //                             Debug.LogError("UI结构和数据结构对不上！:" + ParentBlockBase.Type);
            //                             return;
            //                         }
            //
            //                         for (int i = 0; i < headerOperations.Length; i++)
            //                         {
            //                             if (headerOperations[i].OperationBlock == block)
            //                             {
            //                                 parentTrigger.SetReturnValueGuid(i, blockData.Guid);
            //                             }
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //             break;
            //         case ScratchClassName.Condition:
            //         {
            //             var parentCondition = parentBlock.GetEngineBlockData() as IEngineBlockConditionBase;
            //
            //             if (block.IsOnHeader(out var header, out var headindex))
            //             {
            //                 var headerInputs = header.GetHeaderInput();
            //                 int variableLen = parentCondition.GetVariableLength();
            //                 if (variableLen != headerInputs.Length)
            //                 {
            //                     Debug.LogError("UI结构和数据结构对不上！:" + parentCondition.Type);
            //                     return;
            //                 }
            //
            //                 for (int i = 0; i < variableLen; i++)
            //                 {
            //                     if (block.GetOperationBlock(headerInputs[i], out var operation))
            //                     {
            //                         if (operation.OperationBlock == block)
            //                         {
            //                             parentCondition.SetVarsGuid(i, blockData.Guid);
            //                             //koalaBlock.NextBlockGuid = parentCondition.Guid; //operation执行后返回condition
            //                         }
            //                     }
            //                 }
            //             }
            //
            //             if (block.IsOnBody(out var body, out var bodyindex))
            //             {
            //                 //Body Index
            //                 int bodyBlockSectionIndex = parentSection.RectTrans.GetSiblingIndex();
            //
            //                 //0 Body 
            //                 if (bodyBlockSectionIndex == 0)
            //                 {
            //                     var siblingIndex = block.RectTrans.GetSiblingIndex();
            //                     if (siblingIndex == 0)
            //                     {
            //                         blockData.NextBlockGuid = parentCondition.TrueBlockGuid;
            //                         parentCondition.TrueBlockGuid = blockData.Guid;
            //                     }
            //                     else
            //                     {
            //                         if (block.GetPreBlock(out var preblock, out var preblockIndex))
            //                         {
            //                             blockData.NextBlockGuid = preblock.blockData.NextBlockGuid;
            //                             preblock.GetEngineBlockData().NextBlockGuid = blockData.Guid;
            //                         }
            //                         else
            //                         {
            //                             blockData.NextBlockGuid = parentCondition.TrueBlockGuid;
            //                             parentCondition.TrueBlockGuid = blockData.Guid;
            //                         }
            //                     }
            //                 }
            //
            //                 //1 Body 
            //                 if (bodyBlockSectionIndex == 1)
            //                 {
            //                     var siblingIndex = block.RectTrans.GetSiblingIndex();
            //                     if (siblingIndex == 0)
            //                     {
            //                         blockData.NextBlockGuid = parentCondition.FalseBlockGuid;
            //                         parentCondition.FalseBlockGuid = blockData.Guid;
            //                     }
            //                     else
            //                     {
            //                         if (block.GetPreBlock(out var preblock, out var preblockIndex))
            //                         {
            //                             blockData.NextBlockGuid = preblock.blockData.NextBlockGuid;
            //                             preblock.blockData.NextBlockGuid = blockData.Guid;
            //                         }
            //                         else
            //                         {
            //                             blockData.NextBlockGuid = parentCondition.FalseBlockGuid;
            //                             parentCondition.FalseBlockGuid = blockData.Guid;
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //             break;
            //         case ScratchClassName.Loop:
            //         {
            //             var parentLoop = parentBlock.GetEngineBlockData() as IEngineBlockLoopBase;
            //
            //             if (block.IsOnHeader(out var header, out var headindex))
            //             {
            //                 var headerInputs = header.GetHeaderInput();
            //                 int variableLen = parentLoop.GetVariableLength();
            //                 if (variableLen != headerInputs.Length)
            //                 {
            //                     Debug.LogError("UI结构和数据结构对不上！:" + parentLoop.Type);
            //                     return;
            //                 }
            //
            //                 for (int i = 0; i < variableLen; i++)
            //                 {
            //                     if (block.GetOperationBlock(headerInputs[i], out var operation))
            //                     {
            //                         if (operation.OperationBlock == block)
            //                         {
            //                             parentLoop.SetVarsGuid(i, blockData.Guid);
            //                             //koalaBlock.NextBlockGuid = parentLoop.Guid; //operation执行后返回condition
            //                         }
            //                     }
            //                 }
            //             }
            //
            //             if (block.IsOnBody(out var body, out var bodyindex))
            //             {
            //                 //Body
            //                 var siblingIndex = block.RectTrans.GetSiblingIndex();
            //                 if (siblingIndex == 0)
            //                 {
            //                     blockData.NextBlockGuid = parentLoop.ChildRootGuid;
            //                     parentLoop.ChildRootGuid = blockData.Guid;
            //                 }
            //                 else
            //                 {
            //                     if (block.GetPreBlock(out var preblock, out var preblockIndex))
            //                     {
            //                         blockData.NextBlockGuid = preblock.GetEngineBlockData().NextBlockGuid;
            //                         preblock.GetEngineBlockData().NextBlockGuid = blockData.Guid;
            //                     }
            //                     else
            //                     {
            //                         blockData.NextBlockGuid = parentLoop.ChildRootGuid;
            //                         parentLoop.ChildRootGuid = blockData.Guid;
            //                     }
            //                 }
            //             }
            //         }
            //             break;
            //         case ScratchClassName.Simple:
            //         {
            //             var parentSimple = parentBlock.GetEngineBlockData() as IEngineBlockSimpleBase;
            //
            //             if (block.IsOnHeader(out var header, out var headindex))
            //             {
            //                 var headerInputs = header.GetHeaderInput();
            //                 int variableLen = parentSimple.GetVariableLength();
            //                 if (variableLen != headerInputs.Length)
            //                 {
            //                     Debug.LogError("UI结构和数据结构对不上！:" + parentSimple.Type);
            //                     return;
            //                 }
            //
            //                 for (int i = 0; i < variableLen; i++)
            //                 {
            //                     if (block.GetOperationBlock(headerInputs[i], out var operation))
            //                     {
            //                         if (operation.OperationBlock == block)
            //                         {
            //                             parentSimple.SetVarsGuid(i, blockData.Guid);
            //                             //koalaBlock.NextBlockGuid = parentSimple.Guid; //operation执行后返回condition
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //             break;
            //         case ScratchClassName.Operation:
            //         {
            //             var parentOperation = parentBlock.GetEngineBlockData() as IEngineBlockOperationBase;
            //             if (block.IsOnHeader(out var header, out var headindex))
            //             {
            //                 var headerInputs = header.GetHeaderInput();
            //                 if (2 != headerInputs.Length)
            //                 {
            //                     Debug.LogError("UI结构和数据结构对不上！:" + parentOperation.Type);
            //                     return;
            //                 }
            //
            //                 //variable1
            //                 if (block.GetOperationBlock(headerInputs[0], out var variable1operation))
            //                 {
            //                     if (variable1operation.OperationBlock == block)
            //                     {
            //                         parentOperation.SetVarsGuid(0, blockData.Guid);
            //                         //koalaBlock.NextBlockGuid = parentOperation.Guid; //operation执行后返回condition
            //                     }
            //                 }
            //
            //                 //variable2
            //                 if (block.GetOperationBlock(headerInputs[1], out var variable2operation))
            //                 {
            //                     if (variable2operation.OperationBlock == block)
            //                     {
            //                         parentOperation.SetVarsGuid(1, blockData.Guid);
            //                         //koalaBlock.NextBlockGuid = parentOperation.Guid; //operation执行后返回condition
            //                     }
            //                 }
            //             }
            //         }
            //             break;
            //         case ScratchClassName.Values:
            //         {
            //             var parentVariable = parentBlock.GetEngineBlockData() as IEngineBlockVariableBase;
            //             if (block.IsOnHeader(out var header, out var headindex))
            //             {
            //                 var headerInputs = header.GetHeaderInput();
            //                 int variableLen = parentVariable.GetVariableLength();
            //                 if (variableLen != headerInputs.Length)
            //                 {
            //                     Debug.LogError("UI结构和数据结构对不上！:" + parentVariable.Type);
            //                     return;
            //                 }
            //
            //                 for (int i = 0; i < variableLen; i++)
            //                 {
            //                     if (block.GetOperationBlock(headerInputs[i], out var operation))
            //                     {
            //                         if (operation.OperationBlock == block)
            //                         {
            //                             parentVariable.SetVarsGuid(i, blockData.Guid);
            //                             //koalaBlock.NextBlockGuid = parentVariable.Guid; //operation执行后返回condition
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //
            //             break;
            //
            //         default:
            //             return;
            //     }
            // }
        }

        public bool TryFixedBlockBaseDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos)
        {
            // var newEntity = KoalaScratchView.Instance.CurrentEntity;
            // KoalaScratchUtil.Instance.Traverse(blockData, (block) => { KoalaScratchUtil.Instance.BindBlockWithEntity(frame, block, newEntity); });
            //
            // var Pos = new FPVector3(FP.FromFloat_UNSAFE(transform.position.x), FP.FromFloat_UNSAFE(transform.position.y), FP.FromFloat_UNSAFE(transform.position.z));
            // KoalaStaticDataExtensions.GetStaticData<KoalaEntityBlocksData>(frame, newEntity).ScratchTables[blockData.Guid] = Pos;

            return true;
        }

        public bool UpdateDataPos(IEngineBlockBaseData blockBaseData, Vector3 pos)
        {
            // if (blockData != null && blockData.MountEntity != EntityRef.None)
            // {
            //     var scratchData = KoalaStaticDataExtensions.GetStaticData<KoalaEntityBlocksData>(frame, blockData.MountEntity);
            //     if (scratchData != null && scratchData.ScratchTables.ContainsKey(blockData.Guid))
            //     {
            //         scratchData.ScratchTables[blockData.Guid] = new FPVector3(FP.FromFloat_UNSAFE(transform.position.x), FP.FromFloat_UNSAFE(transform.position.y), FP.FromFloat_UNSAFE(transform.position.z));
            //     }
            // }

            return true;
        }

        public void DeleteBlock(IEngineBlockBaseData block, bool recursion = true)
        {
            // TexturePacker_JsonArray.Frame f = KoalaQuantumRunner.GetCurrentFrame(KoalaQuantumRunner.FrameType.Verified);
            // KoalaScratchUtil.Instance.DeleteBlock(f, block.blockData);
        }

        public IEngineBlockBaseData CreateBlock(ScratchBlockType scratchType, bool isAdd = true)
        {
            // Log.Info("CreateBlock:" + scratchType);
            IEngineBlockBaseData block = null;
            // switch (scratchType)
            // {
            //     //Events
            //     case KoalaScratchType.OnGameStarted:
            //         block = new KoalaOnGameStartedBlock();
            //         break;
            //
            //     case KoalaScratchType.InsAddToList:
            //     case KoalaScratchType.InsAddVariable:
            //     case KoalaScratchType.InsBreakLoop:
            //     case KoalaScratchType.InsChangeColor:
            //     case KoalaScratchType.InsClearList:
            //
            //
            //     case KoalaScratchType.InsInsertAtList:
            //     case KoalaScratchType.InsMoveForward:
            //     case KoalaScratchType.InsPlaySound:
            //     case KoalaScratchType.InsRemoveIndexFromList:
            //     case KoalaScratchType.InsRemoveValueFromList:
            //     case KoalaScratchType.InsRepeat:
            //     case KoalaScratchType.InsRepeatForever:
            //     case KoalaScratchType.InsRepeatUntil:
            //     case KoalaScratchType.InsReplaceIndexOfList:
            //     case KoalaScratchType.InsReturn:
            //     case KoalaScratchType.InsRotateAxis:
            //     case KoalaScratchType.InsSetVariable:
            //     case KoalaScratchType.InsSlideForward:
            //     case KoalaScratchType.InsTranslateToPosition:
            //     case KoalaScratchType.InsTurnDirection:
            //     case KoalaScratchType.InsWait:
            //     case KoalaScratchType.InsWhenJoystickKeyPressed:
            //     case KoalaScratchType.InsWhenKeyPressed:
            //     case KoalaScratchType.InsWhenPlayClicked:
            //
            //     case KoalaScratchType.OpBiggerThan:
            //     case KoalaScratchType.OpDivide:
            //     case KoalaScratchType.OpEqual:
            //     case KoalaScratchType.OpIndexOfValueAtList:
            //     case KoalaScratchType.OpJoystickKeyPressed:
            //     case KoalaScratchType.OpKeyPressed:
            //     case KoalaScratchType.OpList:
            //     case KoalaScratchType.OpListContains:
            //     case KoalaScratchType.OpListLength:
            //     case KoalaScratchType.OpMultiply:
            //     case KoalaScratchType.OpNand:
            //     case KoalaScratchType.OpNor:
            //     case KoalaScratchType.OpNot:
            //     case KoalaScratchType.OpOr:
            //     case KoalaScratchType.OpRandom:
            //     case KoalaScratchType.OpSum:
            //     case KoalaScratchType.OpValueAtIndexAtList:
            //     case KoalaScratchType.OpVariable:
            //     case KoalaScratchType.OpXnor:
            //     case KoalaScratchType.OpXor:
            //     case KoalaScratchType.CstLookAtAndMove:
            //     case KoalaScratchType.InsSpacecraftShoot:
            //     case KoalaScratchType.InsDefineFunction:
            //     case KoalaScratchType.InsFunctionBlock:
            //     case KoalaScratchType.InsReferenceFunctionBlock:
            //     case KoalaScratchType.OpFunctionLocalVariable:
            //         block = new KoalaCreateDecorationBlock();
            //         break;
            //
            //     case KoalaScratchType.OpAnd:
            //         block = new KoalaOpAndBlock();
            //         break;
            //     case KoalaScratchType.IfElse:
            //     case KoalaScratchType.InsIfElse:
            //         block = new KoalaInsIfElseBlock();
            //         break;
            //     case KoalaScratchType.InsIf:
            //         block = new KoalaInsIfBlock();
            //         break;
            //
            //     //Actions
            //     case KoalaScratchType.CreateDecoration:
            //         block = new KoalaCreateDecorationBlock();
            //         break;
            //
            //     //Controls
            //     case KoalaScratchType.RunTimer:
            //         block = new KoalaRunTimerBlock();
            //         break;
            //
            //     case KoalaScratchType.OnCollisionEnter:
            //         block = new KoalaOnCollisionEnterBlock();
            //         break;
            //     case KoalaScratchType.OnCollisionStay:
            //         block = new KoalaOnCollisionStayBlock();
            //         break;
            //     case KoalaScratchType.OnCollisionExit:
            //         block = new KoalaOnCollisionExitBlock();
            //         break;
            //     case KoalaScratchType.OnObjectCreated:
            //         block = new KoalaOnObjectCreatedBlock();
            //         break;
            //     case KoalaScratchType.ApplyForce:
            //         block = new KoalaApplyForceBlock();
            //         break;
            //     case KoalaScratchType.DestroyObject:
            //         block = new KoalaDestroyObjectBlock();
            //         break;
            //     case KoalaScratchType.RepeatAction:
            //         block = new KoalaRepeatActionBlock();
            //         break;
            //     case KoalaScratchType.StartCountdown:
            //         block = new KoalaStartCountdownBlock();
            //         break;
            //     case KoalaScratchType.CompareValues:
            //         block = new KoalaCompareValuesBlock();
            //         break;
            //     case KoalaScratchType.GetCharacterSpeed:
            //         block = new KoalaGetCharacterSpeedBlock();
            //         break;
            //     case KoalaScratchType.GetVectorMagnitude:
            //         block = new KoalaGetVectorMagnitudeBlock();
            //         break;
            //     case KoalaScratchType.IntegerValue:
            //         block = new KoalaIntValueBlock();
            //         break;
            //     case KoalaScratchType.VectorValue:
            //         block = new KoalaVector3ValueBlock();
            //         break;
            //     case KoalaScratchType.EntityValue:
            //         block = new KoalaEntityValueBlock();
            //         break;
            //
            //     default: return null;
            // }
            //
            // if (AllBlocks == null) AllBlocks = new Dictionary<int, KoalaBlockBase>();
            // if (isAdd)
            // {
            //     block.Guid = Guid.NewGuid().GetHashCode();
            //     while (AllBlocks.ContainsKey(block.Guid))
            //     {
            //         block.Guid = Guid.NewGuid().GetHashCode();
            //     }
            //
            //     AllBlocks.Add(block.Guid, block);
            // }
            //
            return block;
        }

        public IEngineBlockBaseData FindPreBlock(int rootGuid, int CurGuid)
        {
            var tempblock = GetBlocksDataRef(rootGuid);

            while (tempblock != null)
            {
                if (tempblock.NextBlockGuid == CurGuid)
                {
                    return tempblock;
                }

                tempblock = GetBlocksDataRef(tempblock.NextBlockGuid);
            }

            return null;
        }

        #region 生成预支存储数据

        public List<Block> GenerateBlocks()
        {
            List<Block> blocks = new List<Block>();
            // while (filter.Next(out var entity, out var component))
            // {
            //     var data = KoalaStaticDataExtensions.GetStaticData<KoalaEntityBlocksData>(frame, entity);
            //     if (entity == CurrentEntity)
            //     {
            //         //展示root ui
            //         foreach (var kv in data.ScratchTables)
            //         {
            //             var rootGuid = kv.Key;
            //             var blockUI = PaintBlockInfo(rootGuid, kv.Value.ToUnityVector3());
            //
            //             if (blockUI != null) blocks.Add(blockUI);
            //         }
            //     }
            // }

            //Fixed UI数据 Operation绑定关系
            for (int i = 0; i < blocks.Count; i++)
            {
                var childBlocks = blocks[i].GetComponentsInChildren<Block>();
                for (int j = 0; j < childBlocks.Length; j++)
                {
                    var childBlock = childBlocks[j];
                    if (childBlock.Type == BlockType.operation)
                    {
                        //检查一下 Input赋值
                        if (childBlock.TryGetOperationInput(out var input))
                        {
                            var itemOperation = childBlock.GetScratchComponent<BlockHeaderItem_Operation>();
                            if (!itemOperation.Inited) itemOperation.Initialize();
                            if (!input.Inited) input.Initialize();
                            input.ContextData.ChildOperation = itemOperation.ContextData.CreateRef<BlockHeaderParam_Data_Operation>();
                        }
                    }
                }
            }

            return blocks;
        }

        HashSet<int> check = new HashSet<int>();

        private Block PaintBlockInfo(int guid, Vector3 pos)
        {
            var block = GetBlocksDataRef(guid);
            if (block == null) return null;
            check.Clear();

            var blockview = DrawNode(block, BlockCanvasManager.Instance.RectTrans, -1);
            blockview.transform.position = pos;

            return blockview.GetComponent<Block>();
        }

        private Block DrawNode(IEngineBlockBaseData node, Transform parentTrans, int index = -1)
        {
            if (node == null) return null;
            if (check.Contains(node.Guid) && node.ClassName != ScratchClassName.Values) return null;
            if (!check.Contains(node.Guid)) check.Add(node.Guid);

            var ResourceItem = ScratchResourcesManager.Instance.TemplateResourcesDict[node.Type];
            Block blockUI = ResourceItem.CreateBlock(node);

            if (index == -1)
            {
                int lastPosIndex = parentTrans.childCount;
                blockUI.SetParent(parentTrans, lastPosIndex);
            }
            else
            {
                if (index < parentTrans.childCount)
                {
                    blockUI.SetParent(parentTrans, index);
                }
                else
                {
                    int lastPosIndex = parentTrans.childCount;
                    blockUI.SetParent(parentTrans, lastPosIndex);
                }
            }


            switch (node.ClassName)
            {
                case ScratchClassName.Trigger: //Event
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockTriggerBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerOperations = section0.Header.GetHeaderOperation();
                        int variableLen = node.GetReturnValueLength();
                        if (variableLen != headerOperations.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            int guid = node.GetReturnValueGuid(i);
                            if (headerOperations[i].OperationBlock.GetEngineBlockData() == null)
                            {
                                var operationBlock = GetBlocksDataRef(guid);
                                headerOperations[i].OperationBlock.SetKoalaBlock(operationBlock);
                            }
                        }
                    }

                    //0 Body 
                    if (section0.Body != null)
                    {
                        var childBlock = DrawNode(GetBlocksDataRef(node.NextBlockGuid), section0.Body.RectTrans);
                    }
                }
                    break;
                case ScratchClassName.Condition: //if else
                {
                    var sections = blockUI.GetChildSection();

                    var _node = node as IEngineBlockConditionBase;
                    //0 Header 
                    if (sections[0].Header != null)
                    {
                        var headerInputs = sections[0].Header.GetHeaderInput();
                        int variableLen = node.GetVariableLength();
                        if (variableLen != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), sections[0].Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                        }
                    }

                    //0 Body 
                    if (sections[0].Body != null)
                    {
                        var childTrueBlock = DrawNode(GetBlocksDataRef(_node.TrueBlockGuid), sections[0].Body.RectTrans);
                    }

                    //1 Body 
                    if (sections[1].Body != null)
                    {
                        var childFalseBlock = DrawNode(GetBlocksDataRef(_node.FalseBlockGuid), sections[1].Body.RectTrans);
                    }

                    //Next Guid
                    var nextBlock = DrawNode(GetBlocksDataRef(node.NextBlockGuid), parentTrans);
                }
                    break;
                case ScratchClassName.Loop: //Reapeat Timer
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockLoopBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerInputs = section0.Header.GetHeaderInput();
                        int variableLen = node.GetVariableLength();
                        if (variableLen != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), section0.Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                        }
                    }

                    //0 Body 
                    if (section0.Body != null)
                    {
                        var childBlock = DrawNode(GetBlocksDataRef(_node.ChildRootGuid), section0.Body.RectTrans);
                    }

                    //Next Guid
                    var nextBlock = DrawNode(GetBlocksDataRef(node.NextBlockGuid), parentTrans);
                }
                    break;
                case ScratchClassName.Simple: //Controls KoalaStartCountdownBlock？？？？、Actions KoalaApplyForceBlock
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockSimpleBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerInputs = section0.Header.GetHeaderInput();
                        int variableLen = node.GetVariableLength();
                        if (variableLen != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), section0.Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                        }
                    }

                    //Next Guid
                    var nextBlock = DrawNode(GetBlocksDataRef(node.NextBlockGuid), parentTrans);
                }
                    break;
                case ScratchClassName.Operation:
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockOperationBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerInputs = section0.Header.GetHeaderInput();

                        if (2 != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        var variable1Block = DrawNode(GetBlocksDataRef(_node.Variable1Guid), section0.Header.RectTrans, headerInputs[0].RectTrans.GetSiblingIndex());
                        var variable2Block = DrawNode(GetBlocksDataRef(_node.Variable2Guid), section0.Header.RectTrans, headerInputs[1].RectTrans.GetSiblingIndex());
                    }
                }
                    break;
                case ScratchClassName.Values:
                {
                    var section0 = blockUI.GetChildSection()[0];

                    var _node = node as IEngineBlockVariableBase;
                    //0 Header 
                    if (section0.Header != null)
                    {
                        var headerInputs = section0.Header.GetHeaderInput();
                        int variableLen = node.GetVariableLength();
                        if (variableLen != headerInputs.Length)
                        {
                            Debug.LogError("UI结构和数据结构对不上！:" + node.Type);
                            return null;
                        }

                        for (int i = 0; i < variableLen; i++)
                        {
                            var operationBlock = DrawNode(GetBlocksDataRef(_node.GetVarGuid(i)), section0.Header.RectTrans, headerInputs[i].RectTrans.GetSiblingIndex());
                        }
                    }
                }
                    break;
            }

            return blockUI;
        }

        #endregion
    }

    public class ScratchEngine : Singleton<ScratchEngine>
    {
        IEngineCoreInterface m_engineCore;
        public IEngineCoreInterface Core => m_engineCore ??= new TestEngineCore();

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}