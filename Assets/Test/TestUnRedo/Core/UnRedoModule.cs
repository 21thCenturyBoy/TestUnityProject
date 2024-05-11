using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestUnRedo
{
    /// <summary> 撤销还原功能模块抽象基类 </summary>
    public abstract partial class UnRedoModule
    {
        protected Stack<Command> undoCommands = new Stack<Command>();
        protected Stack<Command> redoCommands = new Stack<Command>();

        public virtual Stack<Command> UndoCommands => undoCommands;
        public virtual Stack<Command> RedoCommands => redoCommands;

        public virtual bool IsUndoing { get; protected set; } = false;
        public virtual bool IsRedoing { get; protected set; } = false;

        public bool Enable
        {
            get => m_Enable;
            set => SetEnable(value);
        }

        public bool IsSingleton { get; protected set; } = false;

        protected int m_MaxSaveNum;

        public virtual string GetModuleName() => nameof(UnRedoModule);
        
        public override string ToString()
        {
            return GetModuleName();
        }

        protected UnRedoModule()
        {
            m_MaxSaveNum = 100;
            undoCommands = new Stack<Command>(m_MaxSaveNum);
            redoCommands = new Stack<Command>(m_MaxSaveNum);
        }


        public bool Inited => m_Inited;

        protected bool m_Enable = true;
        protected bool m_Inited = false;

        protected virtual void SetEnable(bool enable)
        {
            if (m_Enable == enable) return;

            m_Enable = enable;
        }

        public virtual void Init()
        {
            if (Inited) return;

            m_Inited = true;
        }

        public virtual void Clear()
        {
            if (!Inited) return;

            m_Inited = false;

            IsUndoing = false;
            IsRedoing = false;
        }

        public virtual void Release()
        {
            ClearAllCommands();
            Clear();
        }

        protected virtual void OnUndoBegin()
        {
            IsUndoing = true;
        }

        protected virtual void OnUndoEnd()
        {
            IsUndoing = false;
        }

        protected virtual void OnRedoBegin()
        {
            IsRedoing = true;
        }

        protected virtual void OnRedoEnd()
        {
            IsRedoing = false;
        }

        public virtual void Undo()
        {
            if (!m_Inited)
            {
                LogService.LogError("未初始化！");
                return;
            }

            if (!m_Enable)
            {
                return;
            }

            OnUndoBegin();

            if (undoCommands.Count > 0)
            {
                var command = undoCommands.Pop();

                try
                {
                    command.Undo();
                }
                catch (Exception e)
                {
                    LogService.LogError($"Undo Redo执行命令抛出异常：" + e.ToString());
                }

                KEditorLog.Log("Undo()");

                redoCommands.Push(command);
                command.State = CommandState.R;
            }

            OnUndoEnd();
        }

        public virtual void Redo()
        {
            if (!m_Inited)
            {
                LogService.LogError("未初始化！");
                return;
            }

            if (!m_Enable)
            {
                return;
            }

            OnRedoBegin();
            if (redoCommands.Count > 0)
            {
                var command = redoCommands.Pop();
                IsRedoing = true;
                try
                {
                    command.Redo();
                }
                catch (Exception e)
                {
                    LogService.LogError($"Undo Redo执行命令抛出异常：" + e.ToString());
                }

                KEditorLog.Log("Redo()");

                undoCommands.Push(command);
                command.State = CommandState.U;
            }

            OnRedoEnd();
        }

        public virtual T ExcuteCommand<T>(T command) where T : Command
        {
            if (!m_Inited)
            {
                LogService.LogError("未初始化！");
                return null;
            }

            if (IsUndoing || IsRedoing || command is not { State: CommandState.C })
            {
                LogService.LogError($"<color=red>非法入栈[{command.State}] {command} 已拦截！</color>");
                return null;
            }

            EnterStack(command);

            return command;
        }

        protected virtual void EnterStack(Command command)
        {
            while (undoCommands.Count >= m_MaxSaveNum)
            {
                KoalaUtils.RemoveBottomElement(undoCommands);
            }

            //Debug.Log("EnterStack " + command.Name);
            KEditorLog.Log($"记录撤销还原:{command.Name}:" + command);

            undoCommands.Push(command);

            command.State = CommandState.U;

            foreach (Command redoCommand in redoCommands)
            {
                redoCommand.State = CommandState.C;
            }

            redoCommands.Clear();
        }

        public virtual void ClearAllCommands()
        {
            undoCommands.Clear();
            redoCommands.Clear();
        }
    }
}