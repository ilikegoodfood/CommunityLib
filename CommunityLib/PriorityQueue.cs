using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommunityLib
{
    public class PriorityQueue<T, U> : IEnumerable<T>, IEnumerable<ItemPriorityPair<T, U>> where U : IComparable<U>
    {
        private List<ItemPriorityPair<T, U>> heap = new List<ItemPriorityPair<T, U>>();

        public int Count => heap.Count;

        public void Enqueue(T item, U priority)
        {
            Enqueue(new ItemPriorityPair<T, U>(item, priority));
        }

        public void Enqueue(ItemPriorityPair<T, U> pair)
        {
            heap.Add(pair);
            HeapifyUp(heap.Count - 1);
        }

        public void Enqueue((T, U) tuple)
        {
            Enqueue(new ItemPriorityPair<T, U>(tuple.Item1, tuple.Item2));
        }

        public T Dequeue()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            T root = heap[0].Item;
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            HeapifyDown(0);
            return root;
        }

        public bool TryDequeue(out T result)
        {
            if (heap.Count == 0)
            {
                result = default(T);
                return false;
            }

            result = Dequeue();
            return true;
        }

        public ItemPriorityPair<T, U> DequeueWithPriority()
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

        public bool TryDequeueWithPriority(out T item, out U priority)
        {
            if (heap.Count == 0)
            {
                item = default(T);
                priority = default(U);
                return false;
            }

            var result = DequeueWithPriority();
            item = result.Item;
            priority = result.Priority;
            return true;
        }

        public T Peek()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            return heap[0].Item;
        }

        public bool TryPeek(out T result)
        {
            if (heap.Count == 0)
            {
                result = default(T);
                return false;
            }

            result = Peek();
            return true;
        }

        public ItemPriorityPair<T, U> PeekWithPriority()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            return heap[0];
        }

        public bool TryPeekWithPriority(out T item, out U priority)
        {
            if (Count == 0)
            {
                item = default(T);
                priority = default(U);
                return false;
            }

            var result = PeekWithPriority();
            item = result.Item;
            priority = result.Priority;
            return true;
        }

        public bool Contains(T item)
        {
            return heap.Any(pair => EqualityComparer<T>.Default.Equals(item, pair.Item));
        }

        public bool UpdatePriority(T item, U newPriority)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(item, heap[i].Item))
                {
                    U oldPriority = heap[i].Priority;
                    heap[i] = new ItemPriorityPair<T, U>(item, newPriority);
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

        public bool Remove(T item)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(item, heap[i].Item))
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
            ItemPriorityPair<T, U> temp = heap[index1];
            heap[index1] = heap[index2];
            heap[index2] = temp;
        }

        public PriorityQueue<T, U> Clone()
        {
            PriorityQueue<T, U> copy = new PriorityQueue<T, U>();

            foreach (ItemPriorityPair<T, U> element in heap)
            {
                copy.Enqueue(element.Item, element.Priority);
            }

            return copy;
        }

        public List<T> ToList()
        {
            PriorityQueue<T, U> copy = Clone();
            List<T> list = new List<T>();
            while(copy.Count > 0)
            {
                list.Add(copy.Dequeue());
            }
            return list;
        }

        public T[] ToArray()
        {
            PriorityQueue<T, U> copy = Clone();
            int count = copy.Count;
            T[] array = new T[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = copy.Dequeue();
            }
            return array;
        }

        public Dictionary<T, U> ToDictionary()
        {
            PriorityQueue<T, U> copy = Clone();
            Dictionary<T, U> dict = new Dictionary<T, U>();
            while (copy.Count > 0)
            {
                ItemPriorityPair<T, U> pair = copy.DequeueWithPriority();

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

        public LinkedList<T> ToLinkedList()
        {
            PriorityQueue<T, U> copy = Clone();
            LinkedList<T> linkedList = new LinkedList<T>();
            while (copy.Count > 0)
            {
                linkedList.AddLast(copy.Dequeue());
            }
            return linkedList;
        }

        public Queue<T> ToQueue()
        {
            PriorityQueue<T, U> copy = Clone();
            Queue<T> queue = new Queue<T>();
            while (copy.Count > 0)
            {
                queue.Enqueue(copy.Dequeue());
            }
            return queue;
        }

        public Stack<T> ToStack()
        {
            PriorityQueue<T, U> copy = Clone();
            Stack<T> stack = new Stack<T>();

            // Dequeue elements from the PriorityQueue and push them onto the Stack
            var list = new List<T>();
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


        public void Merge(PriorityQueue<T, U> other)
        {
            PriorityQueue<T, U> copy = other.Clone();
            while (copy.Count > 0)
            {
                Enqueue(copy.DequeueWithPriority());
            }
        }

        public void MergeNoDuplicates(PriorityQueue<T, U> other)
        {
            PriorityQueue<T, U> copy = other.Clone();
            while (copy.Count > 0)
            {
                ItemPriorityPair<T, U> value = other.DequeueWithPriority();
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

        public List<T> FindAll(Predicate<T> match)
        {
            return heap.Where(pair => match(pair.Item)).Select(pair => pair.Item).ToList();
        }

        public T Find(Predicate<T> match)
        {
            ItemPriorityPair<T, U> pair = heap.FirstOrDefault(p => match(p.Item));
            return pair == null ? default(T) : pair.Item;
        }

        public bool TryGetPriority(T item, out U priority)
        {
            var pair = heap.FirstOrDefault(p => EqualityComparer<T>.Default.Equals(p.Item, item));
            if (pair == null)
            {
                priority = default;
                return false;
            }
            priority = pair.Priority;
            return true;
        }

        public U GetPriority(T item)
        {
            var pair = heap.FirstOrDefault(p => EqualityComparer<T>.Default.Equals(p.Item, item));
            if (pair == null)
            {
                throw new InvalidOperationException("Item not found in the priority queue.");
            }
            return pair.Priority;
        }

        public void ForEach(Action<T> action)
        {
            foreach (var pair in heap)
            {
                action(pair.Item);
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            PriorityQueue<T, U> copy = Clone();

            while (copy.Count > 0)
            {
                yield return copy.Dequeue();
            }
        }

        IEnumerator<ItemPriorityPair<T, U>> IEnumerable<ItemPriorityPair<T, U>>.GetEnumerator()
        {
            PriorityQueue<T, U> copy = Clone();

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
