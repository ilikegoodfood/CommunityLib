using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommunityLib
{
    public class PriorityQueue<TItem, TPriority> : IEnumerable<TItem>, IEnumerable<ItemPriorityPair<TItem, TPriority>> where TPriority : IComparable<TPriority>
    {
        private List<ItemPriorityPair<TItem, TPriority>> heap = new List<ItemPriorityPair<TItem, TPriority>>();

        public int Count => heap.Count;

        public void Enqueue(TItem item, TPriority priority)
        {
            Enqueue(new ItemPriorityPair<TItem, TPriority>(item, priority));
        }

        public void Enqueue(ItemPriorityPair<TItem, TPriority> pair)
        {
            heap.Add(pair);
            HeapifyUp(heap.Count - 1);
        }

        public void Enqueue((TItem, TPriority) tuple)
        {
            Enqueue(new ItemPriorityPair<TItem, TPriority>(tuple.Item1, tuple.Item2));
        }

        public TItem Dequeue()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            TItem root = heap[0].Item;
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            HeapifyDown(0);
            return root;
        }

        public bool TryDequeue(out TItem result)
        {
            if (heap.Count == 0)
            {
                result = default(TItem);
                return false;
            }

            result = Dequeue();
            return true;
        }

        public ItemPriorityPair<TItem, TPriority> DequeueWithPriority()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            var root = heap[0];
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            HeapifyDown(0);
            return root;
        }

        public bool TryDequeueWithPriority(out TItem item, out TPriority priority)
        {
            if (heap.Count == 0)
            {
                item = default(TItem);
                priority = default(TPriority);
                return false;
            }

            var result = DequeueWithPriority();
            item = result.Item;
            priority = result.Priority;
            return true;
        }

        public TItem Peek()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            return heap[0].Item;
        }

        public bool TryPeek(out TItem result)
        {
            if (heap.Count == 0)
            {
                result = default(TItem);
                return false;
            }

            result = Peek();
            return true;
        }

        public ItemPriorityPair<TItem, TPriority> PeekWithPriority()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            return heap[0];
        }

        public bool TryPeekWithPriority(out TItem item, out TPriority priority)
        {
            if (Count == 0)
            {
                item = default(TItem);
                priority = default(TPriority);
                return false;
            }

            var result = PeekWithPriority();
            item = result.Item;
            priority = result.Priority;
            return true;
        }

        public bool Contains(TItem item)
        {
            return heap.Any(pair => EqualityComparer<TItem>.Default.Equals(item, pair.Item));
        }

        public bool UpdatePriority(TItem item, TPriority newPriority)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (EqualityComparer<TItem>.Default.Equals(item, heap[i].Item))
                {
                    TPriority oldPriority = heap[i].Priority;
                    heap[i] = new ItemPriorityPair<TItem, TPriority>(item, newPriority);
                    if (newPriority.CompareTo(oldPriority) < 0)
                    {
                        HeapifyUp(i);
                    }
                    else
                    {
                        HeapifyDown(i);
                    }
                    return true;
                }
            }

            return false;
        }

        public bool Remove(TItem item)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (EqualityComparer<TItem>.Default.Equals(item, heap[i].Item))
                {
                    heap[i] = heap[heap.Count - 1];
                    heap.RemoveAt(heap.Count - 1);
                    HeapifyDown(i);
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            heap.Clear();
        }

        public bool IsEmpty()
        {
            return heap.Count == 0;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (heap[index].Priority.CompareTo(heap[parentIndex].Priority) >= 0)
                {
                    break;
                }

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            while (index < heap.Count)
            {
                int leftChildIndex = 2 * index + 1;
                int rightChildIndex = 2 * index + 2;
                int smallestIndex = index;

                if (leftChildIndex < heap.Count && heap[leftChildIndex].Priority.CompareTo(heap[smallestIndex].Priority) < 0)
                {
                    smallestIndex = leftChildIndex;
                }

                if (rightChildIndex < heap.Count && heap[rightChildIndex].Priority.CompareTo(heap[smallestIndex].Priority) < 0)
                {
                    smallestIndex = rightChildIndex;
                }

                if (smallestIndex == index)
                {
                    break;
                }

                Swap(index, smallestIndex);
                index = smallestIndex;
            }
        }

        private void Swap(int index1, int index2)
        {
            ItemPriorityPair<TItem, TPriority> temp = heap[index1];
            heap[index1] = heap[index2];
            heap[index2] = temp;
        }

        public PriorityQueue<TItem, TPriority> Clone()
        {
            PriorityQueue<TItem, TPriority> copy = new PriorityQueue<TItem, TPriority>();

            foreach (ItemPriorityPair<TItem, TPriority> element in heap)
            {
                copy.Enqueue(element.Item, element.Priority);
            }

            return copy;
        }

        public PriorityQueue<TItem, TPriority> ToPriorityQueue()
        {
            return Clone();
        }

        public List<TItem> ToList()
        {
            PriorityQueue<TItem, TPriority> copy = Clone();
            List<TItem> list = new List<TItem>();
            while(copy.Count > 0)
            {
                list.Add(copy.Dequeue());
            }
            return list;
        }

        public TItem[] ToArray()
        {
            PriorityQueue<TItem, TPriority> copy = Clone();
            int count = copy.Count;
            TItem[] array = new TItem[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = copy.Dequeue();
            }
            return array;
        }

        public Dictionary<TItem, TPriority> ToDictionary()
        {
            PriorityQueue<TItem, TPriority> copy = Clone();
            Dictionary<TItem, TPriority> dict = new Dictionary<TItem, TPriority>();
            while (copy.Count > 0)
            {
                ItemPriorityPair<TItem, TPriority> pair = copy.DequeueWithPriority();

                if (dict.ContainsKey(pair.Item))
                {
                    // Keep the higher priority (smaller value) if a duplicate is found
                    if (pair.Priority.CompareTo(dict[pair.Item]) < 0)
                    {
                        dict[pair.Item] = pair.Priority;
                    }
                }
                else
                {
                    dict.Add(pair.Item, pair.Priority);
                }
            }
            return heap.ToDictionary(pair => pair.Item, pair => pair.Priority);
        }

        public LinkedList<TItem> ToLinkedList()
        {
            PriorityQueue<TItem, TPriority> copy = Clone();
            LinkedList<TItem> linkedList = new LinkedList<TItem>();
            while (copy.Count > 0)
            {
                linkedList.AddLast(copy.Dequeue());
            }
            return linkedList;
        }

        public Queue<TItem> ToQueue()
        {
            PriorityQueue<TItem, TPriority> copy = Clone();
            Queue<TItem> queue = new Queue<TItem>();
            while (copy.Count > 0)
            {
                queue.Enqueue(copy.Dequeue());
            }
            return queue;
        }

        public Stack<TItem> ToStack()
        {
            PriorityQueue<TItem, TPriority> copy = Clone();
            Stack<TItem> stack = new Stack<TItem>();

            // Dequeue elements from the PriorityQueue and push them onto the Stack
            var list = new List<TItem>();
            while (copy.Count > 0)
            {
                list.Add(copy.Dequeue());
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                stack.Push(list[i]);
            }

            return stack;
        }


        public void Merge(PriorityQueue<TItem, TPriority> other)
        {
            PriorityQueue<TItem, TPriority> copy = other.Clone();
            while (copy.Count > 0)
            {
                Enqueue(copy.DequeueWithPriority());
            }
        }

        public void MergeNoDuplicates(PriorityQueue<TItem, TPriority> other)
        {
            PriorityQueue<TItem, TPriority> copy = other.Clone();
            while (copy.Count > 0)
            {
                ItemPriorityPair<TItem, TPriority> value = other.DequeueWithPriority();
                if (!Contains(value.Item))
                {
                    Enqueue(value.Item, value.Priority);
                }
            }
        }

        public void TrimExcess()
        {
            heap.TrimExcess();
        }

        public List<TItem> FindAll(Predicate<TItem> match)
        {
            return heap.Where(pair => match(pair.Item)).Select(pair => pair.Item).ToList();
        }

        public TItem Find(Predicate<TItem> match)
        {
            ItemPriorityPair<TItem, TPriority> pair = heap.FirstOrDefault(p => match(p.Item));
            return pair == null ? default(TItem) : pair.Item;
        }

        public bool TryGetPriority(TItem item, out TPriority priority)
        {
            var pair = heap.FirstOrDefault(p => EqualityComparer<TItem>.Default.Equals(p.Item, item));
            if (pair == null)
            {
                priority = default;
                return false;
            }
            priority = pair.Priority;
            return true;
        }

        public TPriority GetPriority(TItem item)
        {
            var pair = heap.FirstOrDefault(p => EqualityComparer<TItem>.Default.Equals(p.Item, item));
            if (pair == null)
            {
                throw new InvalidOperationException("Item not found in the priority queue.");
            }
            return pair.Priority;
        }

        public void ForEach(Action<TItem> action)
        {
            foreach (var pair in heap)
            {
                action(pair.Item);
            }
        }


        public IEnumerator<TItem> GetEnumerator()
        {
            PriorityQueue<TItem, TPriority> copy = Clone();

            while (copy.Count > 0)
            {
                yield return copy.Dequeue();
            }
        }

        IEnumerator<ItemPriorityPair<TItem, TPriority>> IEnumerable<ItemPriorityPair<TItem, TPriority>>.GetEnumerator()
        {
            PriorityQueue<TItem, TPriority> copy = Clone();

            while (copy.Count > 0)
            {
                yield return copy.DequeueWithPriority();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
