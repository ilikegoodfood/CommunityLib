using Assets.Code;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommunityLib
{
    public class PriorityQueue<TValue, TPriority> : IEnumerable<TValue>, IEnumerable<ValuePriorityPair<TValue, TPriority>> where TPriority : IComparable<TPriority>
    {
        public enum DuplicateHandlingMode
        {
            AllowDuplicates,
            DiscardIfExists,
            UpdateIfExists,
            KeepLowest
        }

        private List<ValuePriorityPair<TValue, TPriority>> heap = new List<ValuePriorityPair<TValue, TPriority>>();

        public int Count => heap.Count;

        public void Enqueue(TValue item, TPriority priority)
        {
            Enqueue(new ValuePriorityPair<TValue, TPriority>(item, priority));
        }

        public void Enqueue(ValuePriorityPair<TValue, TPriority> pair)
        {
            heap.Add(pair);
            HeapifyUp(heap.Count - 1);
        }

        public void EnqueueRange(IEnumerable<(TValue, TPriority)> tuples, DuplicateHandlingMode mode)
        {
            foreach (var tuple in tuples)
            {
                switch (mode)
                {
                    case DuplicateHandlingMode.AllowDuplicates:
                        heap.Add(tuple);
                        break;
                    case DuplicateHandlingMode.DiscardIfExists:
                        if (!Contains(tuple.Item1))
                        {
                            heap.Add(tuple);
                        }
                        break;
                    case DuplicateHandlingMode.UpdateIfExists:
                        if (!UpdatePriority(tuple.Item1, tuple.Item2))
                        {
                            heap.Add(tuple);
                        }
                        break;
                    case DuplicateHandlingMode.KeepLowest:
                        int index = heap.FindIndex(pair => pair.Value.Equals(tuple.Item1));
                        if (index != -1)
                        {
                            if (heap[index].Priority.CompareTo(tuple.Item2) > 0)
                            {
                                heap[index].Priority = tuple.Item2;
                            }
                        }
                        else
                        {
                            heap.Add(tuple);
                        }
                        break;
                }
            }
            Heapify(); // Rebuild heap in one pass
        }

        public void EnqueueRange(IEnumerable<ValuePriorityPair<TValue, TPriority>> priorityQueue, DuplicateHandlingMode mode)
        {
            foreach (var item in priorityQueue)
            {
                switch (mode)
                {
                    case DuplicateHandlingMode.AllowDuplicates:
                        heap.Add(item);
                        break;
                    case DuplicateHandlingMode.DiscardIfExists:
                        if (!Contains(item.Value))
                        {
                            heap.Add(item);
                        }
                        break;
                    case DuplicateHandlingMode.UpdateIfExists:
                        if (!UpdatePriority(item.Value, item.Priority))
                        {
                            heap.Add(item);
                        }
                        break;
                    case DuplicateHandlingMode.KeepLowest:
                        int index = heap.FindIndex(pair => pair.Value.Equals(item.Value));
                        if (index != -1)
                        {
                            if (heap[index].Priority.CompareTo(item.Priority) > 0)
                            {
                                heap[index].Priority = item.Priority;
                            }
                        }
                        else
                        {
                            heap.Add(item);
                        }
                        break;
                }
            }
            Heapify(); // Rebuild heap in one pass
        }

        public TValue Dequeue()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            TValue root = heap[0].Value;
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            HeapifyDown(0);
            return root;
        }

        public bool TryDequeue(out TValue result)
        {
            if (heap.Count == 0)
            {
                result = default(TValue);
                return false;
            }

            result = Dequeue();
            return true;
        }

        public ValuePriorityPair<TValue, TPriority> DequeueWithPriority()
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

        public bool TryDequeueWithPriority(out TValue value, out TPriority priority)
        {
            if (heap.Count == 0)
            {
                value = default(TValue);
                priority = default(TPriority);
                return false;
            }

            var result = DequeueWithPriority();
            value = result.Value;
            priority = result.Priority;
            return true;
        }

        public TValue Peek()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            return heap[0].Value;
        }

        public bool TryPeek(out TValue result)
        {
            if (heap.Count == 0)
            {
                result = default(TValue);
                return false;
            }

            result = Peek();
            return true;
        }

        public ValuePriorityPair<TValue, TPriority> PeekWithPriority()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            return heap[0];
        }

        public bool TryPeekWithPriority(out TValue item, out TPriority priority)
        {
            if (Count == 0)
            {
                item = default(TValue);
                priority = default(TPriority);
                return false;
            }

            var result = PeekWithPriority();
            item = result.Value;
            priority = result.Priority;
            return true;
        }

        public bool Contains(TValue item)
        {
            return heap.Any(pair => EqualityComparer<TValue>.Default.Equals(item, pair.Value));
        }

        public bool UpdatePriority(TValue item, TPriority newPriority)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (EqualityComparer<TValue>.Default.Equals(item, heap[i].Value))
                {
                    TPriority oldPriority = heap[i].Priority;
                    heap[i] = new ValuePriorityPair<TValue, TPriority>(item, newPriority);
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

        public bool Remove(TValue item)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (EqualityComparer<TValue>.Default.Equals(item, heap[i].Value))
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

        private void Heapify()
        {
            for (int i = (heap.Count / 2) - 1; i >= 0; i--)
            {
                HeapifyDown(i);
            }
        }

        private void Swap(int index1, int index2)
        {
            ValuePriorityPair<TValue, TPriority> temp = heap[index1];
            heap[index1] = heap[index2];
            heap[index2] = temp;
        }

        private PriorityQueue<TValue, TPriority> Clone()
        {
            PriorityQueue<TValue, TPriority> copy = new PriorityQueue<TValue, TPriority>();

            foreach (ValuePriorityPair<TValue, TPriority> element in heap)
            {
                copy.Enqueue(element.Value, element.Priority);
            }

            return copy;
        }

        public PriorityQueue<TValue, TPriority> ToPriorityQueue()
        {
            return Clone();
        }

        public List<TValue> ToList()
        {
            PriorityQueue<TValue, TPriority> copy = Clone();
            List<TValue> list = new List<TValue>();
            while(copy.Count > 0)
            {
                list.Add(copy.Dequeue());
            }
            return list;
        }

        public TValue[] ToArray()
        {
            PriorityQueue<TValue, TPriority> copy = Clone();
            int count = copy.Count;
            TValue[] array = new TValue[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = copy.Dequeue();
            }
            return array;
        }

        public Dictionary<TValue, TPriority> ToDictionary()
        {
            PriorityQueue<TValue, TPriority> copy = Clone();
            Dictionary<TValue, TPriority> dict = new Dictionary<TValue, TPriority>();
            while (copy.Count > 0)
            {
                ValuePriorityPair<TValue, TPriority> pair = copy.DequeueWithPriority();

                if (dict.ContainsKey(pair.Value))
                {
                    // Keep the higher priority (smaller value) if a duplicate is found
                    if (pair.Priority.CompareTo(dict[pair.Value]) < 0)
                    {
                        dict[pair.Value] = pair.Priority;
                    }
                }
                else
                {
                    dict.Add(pair.Value, pair.Priority);
                }
            }
            return heap.ToDictionary(pair => pair.Value, pair => pair.Priority);
        }

        public LinkedList<TValue> ToLinkedList()
        {
            PriorityQueue<TValue, TPriority> copy = Clone();
            LinkedList<TValue> linkedList = new LinkedList<TValue>();
            while (copy.Count > 0)
            {
                linkedList.AddLast(copy.Dequeue());
            }
            return linkedList;
        }

        public Queue<TValue> ToQueue()
        {
            PriorityQueue<TValue, TPriority> copy = Clone();
            Queue<TValue> queue = new Queue<TValue>();
            while (copy.Count > 0)
            {
                queue.Enqueue(copy.Dequeue());
            }
            return queue;
        }

        public Stack<TValue> ToStack()
        {
            PriorityQueue<TValue, TPriority> copy = Clone();
            Stack<TValue> stack = new Stack<TValue>();

            // Dequeue elements from the PriorityQueue and push them onto the Stack
            var list = new List<TValue>();
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


        public void Merge(PriorityQueue<TValue, TPriority> other)
        {
            PriorityQueue<TValue, TPriority> copy = other.Clone();
            while (copy.Count > 0)
            {
                Enqueue(copy.DequeueWithPriority());
            }
        }

        public void MergeNoDuplicates(PriorityQueue<TValue, TPriority> other)
        {
            PriorityQueue<TValue, TPriority> copy = other.Clone();
            while (copy.Count > 0)
            {
                ValuePriorityPair<TValue, TPriority> value = other.DequeueWithPriority();
                if (!Contains(value.Value))
                {
                    Enqueue(value.Value, value.Priority);
                }
            }
        }

        public void TrimExcess()
        {
            heap.TrimExcess();
        }

        public List<TValue> FindAll(Predicate<TValue> match)
        {
            return heap.Where(pair => match(pair.Value)).Select(pair => pair.Value).ToList();
        }

        public TValue Find(Predicate<TValue> match)
        {
            ValuePriorityPair<TValue, TPriority> pair = heap.FirstOrDefault(p => match(p.Value));
            return pair == null ? default(TValue) : pair.Value;
        }

        public bool TryGetPriority(TValue item, out TPriority priority)
        {
            var pair = heap.FirstOrDefault(p => EqualityComparer<TValue>.Default.Equals(p.Value, item));
            if (pair == null)
            {
                priority = default;
                return false;
            }
            priority = pair.Priority;
            return true;
        }

        public TPriority GetPriority(TValue item)
        {
            var pair = heap.FirstOrDefault(p => EqualityComparer<TValue>.Default.Equals(p.Value, item));
            if (pair == null)
            {
                throw new InvalidOperationException("Item not found in the priority queue.");
            }
            return pair.Priority;
        }

        public void ForEach(Action<TValue> action)
        {
            foreach (var pair in heap)
            {
                action(pair.Value);
            }
        }


        public IEnumerator<TValue> GetEnumerator()
        {
            PriorityQueue<TValue, TPriority> copy = Clone();

            while (copy.Count > 0)
            {
                yield return copy.Dequeue();
            }
        }

        IEnumerator<ValuePriorityPair<TValue, TPriority>> IEnumerable<ValuePriorityPair<TValue, TPriority>>.GetEnumerator()
        {
            PriorityQueue<TValue, TPriority> copy = Clone();

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
