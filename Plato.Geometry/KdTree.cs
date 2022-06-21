using System;
using System.Collections.Generic;
using System.Linq;
using Vim.LinqArray;
using Vim.Math3d;
using System.Diagnostics;

namespace Vim.Geometry
{
    public class KdTree<T> where T : IBounded
    {
        public int Axis => Depth % 3;
        public int Depth;
        public KdTree<T> Left;
        public KdTree<T> Right;
        public AABox Box;
        public List<T> Items = new List<T>();

        public const int MinNodesForSplit = 4;

        public KdTree(int depth)
            => Depth = depth;

        public KdTree(int depth, IEnumerable<T> items)
            : this(depth)
            => AddRange(items);

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var x in items)
                Add(x);
        }

        public void Add(T item)
        {
            if (IsSplit)
                throw new Exception("Cannot add meshes after split");
            Box = item.UpdateBounds(Box);
            Items.Add(item);
        }

        public bool IsSplit
            => Left != null || Right != null;

        public void Split()
        {
            if (IsSplit)
                throw new Exception("Already split");
            if (Items.Count < MinNodesForSplit)
                return;
            var minVal = Items.Min(m => m.Bounds.Min.GetComponent(Axis));
            var maxVal = Items.Max(m => m.Bounds.Max.GetComponent(Axis));
            var n = Items.Count;
            var meshes = Items.OrderBy(m => m.Bounds.Min.GetComponent(Axis));
            Left = new KdTree<T>(Depth + 1, meshes.Take(n / 2));
            Right = new KdTree<T>(Depth + 1, meshes.Skip(n / 2));
            Items.Clear();
            Debug.Assert(Items.Count == 0);
            Debug.Assert(Left.Items.Count + Right.Items.Count == n);
            Left.Split();
            Right.Split();
        }

        public bool Visit(Func<AABox, bool> intersects, Func<T, bool> visitAndContinue)
        {
            if (!intersects(Box))
                return true;

            if (IsSplit)
            {
                if (!Left.Visit(intersects, visitAndContinue))
                    return false;
                if (!Right.Visit(intersects, visitAndContinue))
                    return false;
            }
            else
            {
                foreach (var item in Items)
                {
                    if (!visitAndContinue(item))
                        return false;
                }
            }

            return true;
        }
    }
}
