using System;
using System.Collections.Generic;

namespace ScratchFramework
{
    public sealed class BTreeNode<T>
    {
        private static int PreCount = 256;
        private static Stack<BTreeNode<T>> EmptyNodePool = new Stack<BTreeNode<T>>(PreCount);

        public static BTreeNode<T> CreateNode(T value, params BTreeNode<T>[] childNodes)
        {
            BTreeNode<T> node = null;
            if (EmptyNodePool.Count <= 0)
            {
                node = new BTreeNode<T>(value, childNodes);
            }
            else
            {
                node = EmptyNodePool.Pop();
            }

            node.Value = value;
            node.ChildNodes = new List<BTreeNode<T>>();
            foreach (var child in childNodes)
            {
                node.ChildNodes.Add(child);
                child.Parent = node;
            }

            return node;
        }

        private BTreeNode(T value, params BTreeNode<T>[] childNodes)
        {
            Value = value;
            ChildNodes = new List<BTreeNode<T>>();
            foreach (var child in childNodes)
            {
                this.ChildNodes.Add(child);
                child.Parent = this;
            }
        }

        private T m_Value = default;

        public T Value
        {
            get => m_Value;
            set { m_Value = value; }
        }

        private IList<BTreeNode<T>> ChildNodes { get; set; }
        public BTreeNode<T> Parent { get; set; }

        public BTreeNode<T> AddChild(BTreeNode<T> child)
        {
            this.ChildNodes.Add(child);
            child.Parent = this;
            return this;
        }
        
        /// <param name="callback">return 是否中断遍历</param>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool TraverseTree(Func<int, BTreeNode<T>, bool> callback, int level = 0)
        {
            if (!callback(level, this)) return false;
            foreach (var child in this.ChildNodes)
            {
                if (!child.TraverseTree(callback, level + 1)) return false;
            }

            return true;
        }
        
        public BTreeNode<T> SelecTreeNode(Func<BTreeNode<T>, bool> callback)
        {
            if (callback(this)) return this;
            foreach (var child in this.ChildNodes)
            {
                var node = child.SelecTreeNode(callback);
                if (node != null) return node;
            }

            return null;
        }

        public void ReleaseTree()
        {
            TraverseTree((level, node) =>
            {
                node.Value = default;
                node.ChildNodes.Clear();
                node.Parent = null;
                EmptyNodePool.Push(node);

                return true;
            });
        }
    }
}