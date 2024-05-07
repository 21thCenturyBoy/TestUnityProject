using System;
using System.Collections.Generic;
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

        protected Command()
        {
            m_CreateDateTime = DateTime.Now;
        }

        protected Action<Command> m_Redo;
        protected Action<Command> m_Undo;
        protected Action<Command> m_Dispose;

        public CommandState State = CommandState.C;

        private bool m_IsDisposed = false;

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
            Command command = new Command();
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

    /// <summary> 撤销还原功能模块抽象基类 </summary>
    public abstract class UnRedoModule
    {
        private Stack<Command> undoCommands = new Stack<Command>();
        private Stack<Command> redoCommands = new Stack<Command>();

        private List<Command> waitCommands = new List<Command>();

        public Stack<Command> UndoCommands => undoCommands;
        public Stack<Command> RedoCommands => redoCommands;

        public virtual bool IsUndoing { get; protected set; } = false;
        public virtual bool IsRedoing { get; protected set; } = false;

        public bool Enable
        {
            get => m_Enable;
            set => SetEnable(value);
        }

        protected UnRedoModule(int mMaxSaveNum = 100)
        {
            m_MaxSaveNum = mMaxSaveNum;
            undoCommands = new Stack<Command>(m_MaxSaveNum);
            redoCommands = new Stack<Command>(m_MaxSaveNum);
        }

        protected int m_MaxSaveNum;

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

        public void Undo()
        {
            if (!m_Inited)
            {
                Debug.LogError("未初始化！");
                return;
            }

            if (!m_Enable)
            {
                return;
            }

            if (undoCommands.Count > 0)
            {
                var command = undoCommands.Pop();
                OnUndoBegin();
                try
                {
                    command.Undo();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Undo Redo执行命令抛出异常：" + e.ToString());
                }


                redoCommands.Push(command);
                command.State = CommandState.R;

                OnUndoEnd();
            }
        }

        public void Redo()
        {
            if (!m_Inited)
            {
                Debug.LogError("未初始化！");
                return;
            }

            if (!m_Enable)
            {
                return;
            }

            if (redoCommands.Count > 0)
            {
                var command = redoCommands.Pop();
                OnRedoBegin();
                IsRedoing = true;
                try
                {
                    command.Redo();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Undo Redo执行命令抛出异常：" + e.ToString());
                }


                undoCommands.Push(command);
                command.State = CommandState.U;

                OnRedoEnd();
            }
        }

        public virtual T ExcuteCommand<T>(T command) where T : Command
        {
            if (!m_Inited)
            {
                Debug.LogError("未初始化！");
                return null;
            }

            if (IsUndoing || IsRedoing || command is not { State: CommandState.C })
            {
                Debug.LogError($"<color=red>非法入栈[{command.State}] {command} 已拦截！</color>");
                return null;
            }

            EnterStack(command);

            return command;
        }

        protected virtual void EnterStack(Command command)
        {
            while (undoCommands.Count >= m_MaxSaveNum)
            {
                RemoveBottomElement(undoCommands);
            }

            if (waitCommands.Contains(command))
            {
                waitCommands.Remove(command);
            }

            //Debug.Log("EnterStack " + command.Name);
            Debug.Log($"记录撤销还原:{command.Name}:" + command);

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

        /// <summary>
        /// 删除栈底元素
        /// </summary>
        /// <param name="stack"></param>
        /// <typeparam name="T"></typeparam>
        public static void RemoveBottomElement<T>(Stack<T> stack)
        {
            if (stack.Count == 0)
            {
                return;
            }

            // Create a temporary stack to hold the elements
            Stack<T> tempStack = new Stack<T>();

            // Pop all elements from the original stack and push them onto the temporary stack
            while (stack.Count > 0)
            {
                tempStack.Push(stack.Pop());
            }

            // Pop the bottom element from the temporary stack
            tempStack.Pop();

            // Push all remaining elements from the temporary stack back onto the original stack
            while (tempStack.Count > 0)
            {
                stack.Push(tempStack.Pop());
            }
        }
    }

    /// <summary> 撤销还原功能模块池 </summary>
    public class UnRedoModulePool<T> : ClassObjectPool<T> where T : UnRedoModule, new()
    {
        protected Stack<T> pool = new Stack<T>();

        public int Count
        {
            get => pool.Count;
        }

        public new T Spawn(bool createIfPoolEmpty)
        {
            if (pool.Count > 0)
            {
                T rtn = pool.Pop();
                if (rtn == null)
                {
                    if (createIfPoolEmpty)
                    {
                        rtn = new T();
                    }
                }

                rtn.Init();

                return rtn;
            }
            else
            {
                if (createIfPoolEmpty)
                {
                    T rtn = new T();
                    return rtn;
                }
            }

            return null;
        }


        public new bool Recycle(T obj)
        {
            if (obj == null) return false;

            obj.Release();

            if (pool.Count >= maxCount && maxCount > 0)
            {
                obj = null;
                return false;
            }


            pool.Push(obj);
            return true;
        }

        public void Clear()
        {
            foreach (var module in pool)
            {
                module.Release();
            }

            pool.Clear();
            maxCount = 0;
        }

        public UnRedoModulePool(int maxCount) : base(maxCount)
        {
        }
    }


    public class ClassObjectPool<T> where T : class, new()
    {
        protected Stack<T> pool = new Stack<T>();

        public int Count
        {
            get => pool.Count;
        }

        //最大对象个数  <=0表示不限制个数
        protected int maxCount;

        public ClassObjectPool(int maxCount)
        {
            this.maxCount = maxCount;
        }

        /// <summary>
        /// 从池里面取类对象
        /// </summary>
        /// <param name="createIfPoolEmpty">如果为空是否new出来</param>
        public T Spawn(bool createIfPoolEmpty)
        {
            if (pool.Count > 0)
            {
                T rtn = pool.Pop();
                if (rtn == null)
                {
                    if (createIfPoolEmpty)
                    {
                        rtn = new T();
                    }
                }

                return rtn;
            }
            else
            {
                if (createIfPoolEmpty)
                {
                    T rtn = new T();
                    return rtn;
                }
            }

            return null;
        }

        /// <summary>
        /// 回收类对象
        /// </summary>
        public bool Recycle(T obj)
        {
            if (obj == null)
                return false;
            if (pool.Count >= maxCount && maxCount > 0)
            {
                obj = null;
                return false;
            }

            pool.Push(obj);
            return true;
        }

        public void Clear()
        {
            pool.Clear();
            maxCount = 0;
        }
    }
}