namespace ScratchFramework.Runtime
{
    public static class EngineGlobal
    {
    }

    /// <summary>
    /// 实体中介者
    /// </summary>
    public abstract class EngineEntityMediator
    {
        public abstract object GetEntity();
        public abstract T GetEntity<T>();
    }

    public interface IEngineInstruction_Option<T> : IEngineInstruction
    {
        T GetOperation();
    }

    public interface IEngineInstruction_Function : IEngineInstruction
    {
        void Function();
    }

    public interface IEngineInstruction_OnStart
    {
        void OnStart();
    }

    public interface IEneineInstruction_OnEnable
    {
        void OnEnable();
    }

    public interface IEneineInstruction_OnDisable
    {
        void OnDisable();
    }

    public interface IEneineInstruction_OnDestroy
    {
        void OnDestroy();
    }
    
    /// <summary>
    /// 功能块
    /// </summary>
    public interface IEngineInstruction
    {
        void Init(IEngineBlockBaseData logicData);
        void Reset();
    }
    
    
    //
    // public class EngineBlockStack
    // {
    //     public void Initialize()
    //     {
    //         IsActive = false;
    //         Pointer = 0;
    //     }
    //
    //     private bool m_isActive = false;
    //     private bool m_isStepPlay = false;
    //
    //     public bool IsActive
    //     {
    //         get => m_isActive;
    //         set
    //         {
    //             if (IsActive == false && value == true)
    //             {
    //                 // v2.11.2 - bugfix: blocks stack not starting from where they stopped when set "BE2_BlocksStack.IsActive = true"
    //                 if (m_isStepPlay == false)
    //                 {
    //                     int instructionsCount = InstructionsArray.Length;
    //                     for (int i = 0; i < instructionsCount; i++)
    //                     {
    //                         InstructionsArray[i].InstructionBase.OnStackActive();
    //                     }
    //                 }
    //
    //                 m_isStepPlay = false;
    //
    //                 // activate all shadows
    //                 foreach (I_BE2_Instruction instruction in InstructionsArray)
    //                 {
    //                     instruction.InstructionBase.Block.SetShadowActive(true);
    //                 }
    //             }
    //             else if (IsActive == true && value == false)
    //             {
    //                 // deactivate all shadows
    //                 foreach (I_BE2_Instruction instruction in InstructionsArray)
    //                 {
    //                     instruction.InstructionBase.Block.SetShadowActive(false);
    //                 }
    //             }
    //
    //             m_isActive = value;
    //         }
    //     }
    //
    //     public int Pointer { get; set; }
    //     public IEngineInstruction[] InstructionsArray { get; set; }
    //
    //     public EngineEntityMediator GetEntityMediator()
    //     {
    //         return new EngineEntityMediator();
    //     }
    //
    //     public IEngineInstruction TriggerInstruction { get; set; }
    //
    //     public void Function()
    //     {
    //         ExecuteSection(0);
    //     }
    //
    //     public void ExecuteSection(int sectionIndex)
    //     {
    //         if (sectionIndex < InstructionsArray.Length)
    //         {
    //             IEngineInstruction instruction = InstructionsArray[sectionIndex];
    //             instruction.Function();
    //         }
    //     }
    //
    //     public void EndExecution()
    //     {
    //         IsActive = false;
    //     }
    //
    //     public void OnButtonPlay()
    //     {
    //         IsActive = true;
    //     }
    //
    //     public void OnAwake()
    //     {
    //         IsActive = false;
    //     }
    // }
}