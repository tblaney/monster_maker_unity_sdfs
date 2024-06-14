using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace snorri
{
    [Serializable]
    public class Bag<T> : IEnumerable<T>
    {
        public T[] items;
        //[ReadOnly]
        //public int _count;

        public static int Capacity = 16;

        //------------------------------------------------------
        // constructors
        public Bag()
        {
            items = new T[]{};
        }
        public Bag(int count)
        {
            items = new T[count];
        }
        public Bag(params T[] vals)
        {
            items = new T[vals.Length];
            Array.Copy(vals, items, vals.Length);
        }
        public Bag(Bag<T> itemsIn)
        {
            items = new T[itemsIn.Length];
            Array.Copy(itemsIn.items, items, itemsIn.Length);
        }
        public Bag(System.Collections.Generic.List<T> itemsIn)
        {
            items = new T[itemsIn.Count];
            Array.Copy(itemsIn.ToArray(), items, itemsIn.Count);
        }
        public Bag(IEnumerable<T> enumerable)
        {
            int count = 0;
            foreach (T val in enumerable)
            {
                count++;
            }
            items = new T[count];
            //_count = count;
            int i = 0;
            foreach (T val in enumerable)
            {   
                items[i] = val;
                i++;
            }
        }

        //------------------------------------------------------
        // index

        [JsonIgnore]
        public T this[int i]
        {
            get
            {
                return this.Get(i);
            }   
            set
            {
                this.Set(i, value);
            }
        }
        [JsonIgnore]
        public int Length
        {
            get
            {
                return items.Length;
            }
        }
        [JsonIgnore]
        public T x { get { return this[0]; } set { this[0] = value; } }
        [JsonIgnore]
        public T y { get { return this[1]; } set { this[1] = value; } }
        [JsonIgnore]
        public T z { get { return this[2]; } set { this[2] = value; } }

        public T GetGrid(VecInt position, int width)
        {
            return Get(ConvertIndex(position.x, position.y, width));
        }
        public int ConvertIndex(int x, int y, int width)
        {
            return x * width + y;
        }

        //------------------------------------------------------
        // methods

        public T Get(int idx)
        {
            if (idx >= 0 && idx < Length)
            {
                return items[idx];
            } 
            return default(T);
        }
        public T Last()
        {
            int idx = Length - 1;
            if (idx < 0)
                return default(T);

            return Get(idx);
        }
        public T First()
        {
            if (Length > 0)
                return Get(0);

            return default(T);
        }
        public T Pop()
        {
            if (Length > 0)
            {
                T val = Get(0);
                Remove(val);
                return val;
            }
            return default(T);
        }
        public T Get(string name) 
        {
            foreach (T val in items)
            {
                //indexable idx = val as indexable;
                INameable nameable = val as INameable;
                if (nameable != null && nameable.Name == name)
                {
                    return val;
                }
            } return default(T);
        }
        public T Get(string name, out bool foundSave) 
        {
            foundSave = true;
            foreach (T val in items)
            {
                //indexable idx = val as indexable;
                INameable nameable = val as INameable;
                if (nameable != null && nameable.Name == name)
                {
                    return val;
                }
            } 
            foundSave = false;
            return default(T);
        }
        public void Resize(int size)
        {
            Array.Resize(ref items, size);
           // _count = size;
            //LOG.Console("resize Bag to " + size + ", and count is " + _count);
        }
        public void Set(string name, T val)
        {
            int i = Index(name);
            if (i > -1)
            {
                items[i] = val;
            } else
            {
                Append(val);
            }
        }
        public void Set(int index, T val)
        {
            if (index >= 0 && index < Length)
            {
                items[index] = val;
            }
        }
        public bool HasElement(string name)
        {
            foreach (T val in items)
            {
                //indexable idx = val as indexable;
                INameable nameable = val as INameable;
                if (nameable != null && nameable.Name == name)
                {
                    return true;
                } else if (val is string)
                {
                    if ((val as string) == name)
                        return true;
                }
            } return false;
        }
        public int Index(T valIn)
        {
            return Array.IndexOf(items, valIn);
        }
        public int Index(string name)
        {
            int i = 0;
            foreach (T val in items)
            {
                INameable nameable = val as INameable;
                if (nameable != null && nameable.Name == name)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
        public void Remove(T valIn)
        {
            int i = Index(valIn);
            if (i < 0)
                return;
                
            for (int j = i + 1; j < items.Length; j++)
            {
                items[j - 1] = items[j];
            }
            Resize(Length-1);
        }
        public void Remove(Bag<T> vals)
        {
            //LOG.Console("Bag removal of Length: " + vals.Length);
            foreach (T val in vals)
            {
                Remove(val);
            }
        }
        public void RemoveByName(string name)
        {
            T val = Get(name);
            Remove(val);
        }
        public void Append(T valIn, bool performCheck = true)
        {
            if (performCheck && Has(valIn))
                return;

            Resize(Length+1);
            items[Length-1] = valIn;
        }
        public void AppendBag(Bag<T> valIn, bool performCheck = true)
        {
            foreach (T val in valIn)
            {
                Append(val, performCheck);
            }
        }
        public bool Has(T valIn)
        {
            return Index(valIn) > -1;
        }
        public bool Contains(T valIn)
        {
            return Index(valIn) > -1;
        }
        public T[] All()
        {
            return items;
        }
        public List<T> ToList()
        {
            return new List<T>(items);
        }
        public Vector3 ToVector()
        {
            if (typeof(T) != typeof(float) && typeof(T) != typeof(int))
            {
                throw new InvalidOperationException("ToVector can only be called when T is float.");
            }
            
            // Explicitly cast T to float, which is safe under the assumption that T is float when this method is called.
            float fx = (float)(object)this[0];
            float fy = (float)(object)this[1];
            float fz = (float)(object)this[2];

            return new Vector3(fx, fy, fz);
        }
        public Vec ToVec()
        {
            if (typeof(T) != typeof(float) && typeof(T) != typeof(int))
            {
                throw new InvalidOperationException("ToVector can only be called when T is float.");
            }
            
            // Explicitly cast T to float, which is safe under the assumption that T is float when this method is called.
            float fx = (float)(object)this[0];
            float fy = (float)(object)this[1];
            float fz = (float)(object)this[2];

            return new Vec(fx, fy, fz);
        }
        public float Min()
        {
            if (typeof(T) != typeof(float))
            {
                throw new InvalidOperationException("ToVector can only be called when T is float.");
            }
            
            float min = 10000f;
            foreach (T tval in items)
            {
                float f = (float)(object)tval;
                if (f < min)
                {
                    min = f;
                }
            }

            return min;
        }
        public float Max()
        {
            if (typeof(T) != typeof(float))
            {
                throw new InvalidOperationException("ToVector can only be called when T is float.");
            }
            
            float max = -1000f;
            foreach (T tval in items)
            {
                float f = (float)(object)tval;
                if (f > max)
                {
                    max = f;
                }
            }

            return max;
        }
        public void Reverse()
        {
            Array.Reverse(items);
        }
        public void Clear()
        {
            items = new T[]{};
        }
        public T GetRandom()
        {
            return items[UnityEngine.Random.Range(0, Length)];
        }
        public void Sort()
        {
            List<T> newList = new List<T>();
            foreach (T val in items)
            {
                IId id = val as IId;
                if (id == null)
                    return;

                if (newList.Count > 0)
                {
                    bool added = false;
                    int i = 0;
                    foreach (T other in newList)
                    {
                        IId idOther = val as IId;
                        if (idOther.Id > id.Id)
                        {
                            // insert before
                            newList.Insert(i, val);
                            added = true;
                            break;
                        }
                        i++;
                    }
                    if (!added)
                    {
                        newList.Add(val);
                    }
                } else
                {
                    newList.Add(val);
                }
            }
            items = newList.ToArray();
        }

        public string GetLog()
        {
            string log_out = "Set log:";
            foreach (T val in items)
            {
                log_out += " " + val.ToString();
            }
            return log_out;
        }

        //-----------------------------------
        // enumerator
        public IEnumerator<T> GetEnumerator()
        {
            return new BagEnum<T>(items);
        }
        private IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            // call the generic version of the method
            return GetEnumerator1();
        }
    }

    public class BagEnum<T> : IEnumerator<T>
    {
        public T[] _items;
        int _position = -1;

        public BagEnum(T[] Bag)
        {
            _items = Bag;
        }

        public T Current
        {
            get
            {
                try
                {
                    return _items[_position];
                }
                catch (IndexOutOfRangeException)
                {
                    return default(T);
                }
            }
        }
        private object Current1
        {
            get { return this.Current; }
        }
        object IEnumerator.Current
        {
            get { return Current1; }
        }

        public bool MoveNext()
        {
            _position++;
            return (_position < _items.Length);
        }
        public void Reset()
        {
            _position = -1;
        }
        public void Dispose()
        {
            _items = null;
            Reset();
        }

    }
}