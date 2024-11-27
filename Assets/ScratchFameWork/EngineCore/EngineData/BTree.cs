using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            this.Value = value;
            this.ChildNodes = new List<BTreeNode<T>>();
            foreach (var child in childNodes)
            {
                this.ChildNodes.Add(child);
                child.Parent = this;
            }
        }

        public T Value { get; set; }
        private IList<BTreeNode<T>> ChildNodes { get; set; }
        public BTreeNode<T> Parent { get; set; }

        public BTreeNode<T> AddChild(BTreeNode<T> child)
        {
            this.ChildNodes.Add(child);
            child.Parent = this;
            return this;
        }

        public void TraverseTree(Action<int, BTreeNode<T>> callback, int level = 0)
        {
            callback(level, this);
            foreach (var child in this.ChildNodes)
            {
                child.TraverseTree(callback, level + 1);
            }
        }

        public void ReleaseTree()
        {
            TraverseTree((level, node) =>
            {
                node.Value = default;
                node.ChildNodes.Clear();
                node.Parent = null;
                EmptyNodePool.Push(node);
            });
        }
    }
}