using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestUnRedo
{
    public enum CommandState
    {
        C,
        U,
        R,
        D
    }

    public class Command
    {
        public string Name;

        protected DateTime m_CreateDateTime;
        public long CommandTicks => m_CreateDateTime.Ticks;

        public Command()
        {
            m_CreateDateTime = DateTime.Now;
        }

        private Action<Command> m_Redo;
        private Action<Command> m_Undo;
        private Action<Command> m_Dispose;

        public CommandState State = CommandState.C;

        private bool m_IsDisposed = false;

        //暴露内部扩展，不太好
        public Action<Command> ExtendRedoStart;
        public Action<Command> ExtendUndoStart;
        public Action<Command> ExtendRedoCallback;
        public Action<Command> ExtendUndoCallback;

        public virtual void Redo()
        {
            ExtendRedoStart?.Invoke(this);
            m_Redo?.Invoke(this);
            ExtendRedoCallback?.Invoke(this);
        }

        public virtual void Undo()
        {
            ExtendUndoStart?.Invoke(this);
            m_Undo?.Invoke(this);
            ExtendUndoCallback?.Invoke(this);
        }

        public virtual void SetCommand(Action<Command> redo, Action<Command> undo, Action<Command> dispose = null)
        {
            m_Redo = redo;
            m_Undo = undo;
            m_Dispose = dispose;
        }

        public static Command Create(string name = "Command")
        {
            return Create<Command>(name);
        }

        public static T Create<T>(string name = "Command")where T : Command, new()
        {
            T command = new T();
            command.Name = name;

            //TODO 创建优化,池
            return command;
        }
    }

    public class DataCommand<T> : Command
    {
        protected T m_PreData;
        protected T m_NewData;

        protected DataCommand()
        {
            m_CreateDateTime = DateTime.Now;
        }

        public Action<T> Action { get; protected set; }

        public virtual void SetCommand(T preData, T newData, Action<T> action)
        {
            m_PreData = preData;
            m_NewData = newData;
            Action = action;

            base.SetCommand((c) => { Action?.Invoke(m_NewData); }, (c) => { Action?.Invoke(m_PreData); });
        }

        public static DataCommand<T> Create<T>(string name = "DataCommand")
        {
            DataCommand<T> command = new DataCommand<T>();
            command.Name = name;

            //TODO 创建优化,池
            return command;
        }

        public override string ToString()
        {
            return $"{m_PreData}->{m_NewData}";
        }
    }

    public class CombineCommand : Command
    {
        protected List<Command> m_Commands;

        public int Count => m_Commands?.Count ?? 0;

        /// <summary>
        /// 按顺序依次塞入命令
        /// </summary>
        /// <param name="commands"></param>
        public virtual void SetCommand(params Command[] commands)
        {
            m_Commands = commands.ToList();
        }

        public virtual void SetCommand<T>(List<T> commands) where T : Command
        {
            m_Commands = new List<Command>(commands);
        }

        public virtual void AddCommand(params Command[] commands)
        {
            if (m_Commands == null)
            {
                m_Commands = commands.ToList();
            }
            else
            {
                m_Commands.AddRange(commands);
            }
        }

        public override void Redo()
        {
            if (m_Commands == null) return;
            for (int i = 0; i < m_Commands.Count; i++)
            {
                m_Commands[i].Redo();
            }
        }

        public override void Undo()
        {
            if (m_Commands == null) return;
            for (int i = m_Commands.Count - 1; i >= 0; i--)
            {
                m_Commands[i].Undo();
            }
        }

        public static CombineCommand Create(string name = "CombineCommand")
        {
            CombineCommand command = new CombineCommand();
            command.Name = name;

            //TODO 创建优化,池
            return command;
        }

        public static T Create<T>(string name = "CombineCommand") where T : CombineCommand, new()
        {
            T command = new T();
            command.Name = name;

            //TODO 创建优化,池
            return command;
        }

        public override string ToString()
        {
            return $"{nameof(Count)}: {Count}";
        }
    }
}