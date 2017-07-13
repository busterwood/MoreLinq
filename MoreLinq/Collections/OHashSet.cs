﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusterWood.Collections
{
    public class OHashSet<T>
    {
        int[] indexes;
        T[] values;
        int count;

        public OHashSet(IEqualityComparer<T> equality = null)
        {
            indexes = new int[3];
            values = new T[3];
            count = 0;
            Equality = equality ?? EqualityComparer<T>.Default;
        }

        public IEqualityComparer<T> Equality { get; }

        public int Count => count;

        public bool Add(T item)
        {
            var hc = item.GetHashCode();
            var firstSlot = hc % indexes.Length;
            int slot = firstSlot;
            for (;;)
            {
                int valueIdx = indexes[slot] - 1; // default value is 0, so always store one more than a real index
                if (valueIdx < 0)
                {
                    // not found, add it
                    values[count] = item;
                    indexes[slot] = ++count; // add one on here, taken off above
                    return true;
                }
                if (Equality.Equals(item, values[valueIdx]))
                {
                    return false; // item already there
                }
                
                // another value is in that slot, try the next index
                if (++slot == indexes.Length)
                    slot = 0;

                // searched all possible entries and returned back to original slot, must be full, so resize
                if (slot == firstSlot)
                {
                    Resize();
                    slot = hc % indexes.Length;
                }
            }
        }

        private void Resize()
        {
            int newSize = (indexes.Length * 2) + 1;
            Array.Resize(ref values, newSize);
            indexes = new int[newSize];
            // recreate indexes from values
            for (int i = 0; i < values.Length; i++)
            {
                if (i == count) break;
                var hc = values[i].GetHashCode();
                SetIndexForValue(i, hc % indexes.Length);
            }
        }

        private void SetIndexForValue(int valueIndex, int firstSlot)
        {
            int slot = firstSlot;
            for (;;)
            {
                int valueIdx = indexes[slot] - 1; // default value is 0, so always store one more than a real index
                if (valueIdx < 0)
                {
                    indexes[slot] = valueIndex + 1; // add one on here, taken of then searching, avoids initialising indexes
                    break;
                }
                // another value is in that slot, try the next index
                if (++slot == indexes.Length)
                    slot = 0;

                // searched all possible entries and returned back to original slot, must be full, so resize
                if (slot == firstSlot)
                {
                    throw new InvalidOperationException("all slots are full after resizing, this should not happen!");
                }
            }
        }
    }
}