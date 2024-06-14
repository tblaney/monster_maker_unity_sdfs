using System;
using UnityEngine;
using Newtonsoft.Json;

namespace snorri
{
    [Serializable]
    public class VecInt : IEquatable<VecInt>
    {
        public int[] vec;

        public VecInt()
        {
            vec = new int[]{0,0,0};
        }
        public VecInt(string vec_int_string)
        {
            string[] strings = vec_int_string.Split(" ");
            vec = new int[strings.Length];
            int i = 0;
            foreach (string s in strings)
            {
                vec[i] = Int32.Parse(s);
                i++;
            }
        }
        public VecInt(Bag<int> Set)
        {
            vec = new int[Set.Length];
            int j = 0;
            foreach (int i in Set)
            {
                vec[j] = i;

                j++;
            }
        }
        public VecInt(params int[] vals)
        {
            vec = vals;
        }
        public VecInt(params float[] vals)
        {
            vec = new int[vals.Length];
            
            int i = 0;
            foreach (float f in vals)
            {
                vec[i] = (int)f;
                i++;
            }
        }
        public VecInt(VecInt vectorCopy)
        {
            vec = new int[vectorCopy.Length];
            int k = 0;
            foreach (int i in vectorCopy.vec)
            {
                vec[k] = i;
                k++;
            }
        }
        public VecInt(Vector2Int vecIn)
        {
            vec = new int[]{vecIn.x, vecIn.y};
        }
        public VecInt(Vector3Int vecIn)
        {
            vec = new int[]{vecIn.x, vecIn.y, vecIn.z};
        }
        public VecInt(Vector3 vecIn)
        {
            vec = new int[]{(int)vecIn.x, (int)vecIn.y, (int)vecIn.z};
        }
        public VecInt(Vec vec)
        {
            this.vec = new int[vec.vec.Length];
            int i = 0;
            foreach (float val in vec.vec)
            {
                Set(i, (int)val);
                i++;
            }
        }
        [JsonIgnore]
        public int x
        {
            get
            {
                return Get("x");
            }
            set
            {
                Set(0, value);
            }
        }
        [JsonIgnore]
        public int y
        {
            get
            {
                return Get("y");
            }
            set
            {
                Set(1, value);
            }
        }
        [JsonIgnore]
        public int z
        {
            get
            {
                return Get("z");
            }
            set
            {
                Set(2, value);
            }
        }
        public int Get(int idx)
        {
            if (idx >= 0 && idx < vec.Length)
                return vec[idx];
            return 0;
        }
        public void Set(int idx, int val)
        {
            if (idx >= 0 && idx < vec.Length)
                vec[idx] = val;
        }
        public int Get(string val)
        {
            switch (val)
            {
                case "x":
                    return Get(0);
                case "y":
                    return Get(1);
                case "z":
                    return Get(2);
                case "r":
                    return Get(0);
                case "g":
                    return Get(1);
                case "b":
                    return Get(2);
                case "a":
                    return Get(3);
            }
            return 0;
        }
        [JsonIgnore]
        public Vector2Int vec2
        {
            get
            {
                if (vec.Length < 2)
                {
                    return Vector2Int.zero;
                } else
                {
                    return new Vector2Int(Get(0), Get(1));
                }
            }
        }
        [JsonIgnore]
        public Vector3Int vec3
        {
            get
            {
                if (vec.Length < 3)
                {
                    return Vector3Int.zero;
                } else
                {
                    return new Vector3Int(Get(0),Get(1),Get(2));
                }
            }
            
        }
        public Bag<VecInt> GetNeighbours()
        {
            Bag<VecInt> vecs = new Bag<VecInt>();
            Bag<VecInt> vecsOut = new Bag<VecInt>();
            if (vec.Length > 2)
            {
                vecs.Append(new VecInt(1, 0, 0));
                vecs.Append(new VecInt(-1, 0, 0));
                vecs.Append(new VecInt(0, 1, 0));
                vecs.Append(new VecInt(0, -1, 0));
                vecs.Append(new VecInt(0, 0, 1));
                vecs.Append(new VecInt(0, 0, -1));
            } else
            {
                vecs.Append(new VecInt(1, 0));
                vecs.Append(new VecInt(-1, 0));
                vecs.Append(new VecInt(0, 1));
                vecs.Append(new VecInt(0, -1));
            }
            foreach (VecInt vec in vecs.All())
            {
                //VecInt vecNew = new VecInt(this);
                vecsOut.Append(this.Add(vec));
            }

            return vecsOut;
        }
        public VecInt Add(VecInt vectorAdd)
        {
            VecInt vecOut = new VecInt(this);
            int i = 0;
            foreach (int val in this.All())
            {
                vecOut.Set(i, val + vectorAdd.Get(i));
                i++;
            }
            return vecOut;
        }
        public VecInt Subtract(VecInt vectorSubtract)
        {
            VecInt vecOut = new VecInt(this);
            int i = 0;
            foreach (int val in vec)
            {
                vecOut.Set(i, val - vectorSubtract.Get(i));
                i++;
            }
            return vecOut;
        }
        public VecInt Clamp(VecInt min, VecInt max)
        {
            VecInt vecOut = new VecInt(this);            
            int i = 0;
            foreach (int val in this.All())
            {
                int newVal = val;

                if (newVal < min.Get(i))
                    newVal = min.Get(i);
                if (newVal > max.Get(i))
                    newVal = max.Get(i);

                vecOut.Set(i, newVal);
                i++;
            }
            return vecOut;
        }
        public int[] All()
        {
            return vec;
        }
        public int Length       
        {
            get { return vec.Length; }
        }
        public float GetRandomRange()
        {
            return UnityEngine.Random.Range(Get(0), Get(1));
        }
        public static VecInt GetRandomRange(VecInt min, VecInt max)
        {
            if (min.Length == 2)
            {
                return new VecInt(
                    UnityEngine.Random.Range(min.x, max.x), 
                    UnityEngine.Random.Range(min.y, max.y));
            }
            return new VecInt(
                UnityEngine.Random.Range(min.x, max.x), 
                UnityEngine.Random.Range(min.y, max.y),
                UnityEngine.Random.Range(min.z, max.z));
        }
        public float Dot(VecInt other)
        {
            if (Length == 2)
            {
                return Vector2.Dot(vec2, other.vec2);
            } else
            {
                return Vector3.Dot(vec3, other.vec3);
            }
        }
        public VecInt Diretion(VecInt other)
        {
            if (Length == 2)
            {
                VecInt newVec = new VecInt(other.vec2 - this.vec2);
                return newVec;
            } else
            {
                VecInt newVec = new VecInt(other.vec3 - this.vec3);
                return newVec;
            }
        }
        public VecInt Multiply(float factor)
        {
            VecInt newVec = new VecInt(vec);
            int j = 0;
            foreach (int i in newVec.vec)
            {
                newVec.vec[j] = (int)(i*factor);
                j++;
            }
            return newVec;
        }
        public bool EqualsTo(VecInt other)
        {
            if (Length != other.Length)
                return false;
                
            int i = 0;
            foreach (int val in vec)
            {
                if (val != other.Get(i))
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        public static VecInt zero
        {
            get
            {
                return new VecInt(0,0,0);
            }
        }
        public static VecInt zero2
        {
            get
            {
                return new VecInt(0,0);
            }
        }

        //---overrides---//
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            VecInt otherVec = other as VecInt;
            if (otherVec == null)
                return false;

            if (Length != otherVec.Length)
                return false;
                
            int i = 0;
            foreach (int val in vec)
            {
                if (val != otherVec.Get(i))
                {
                    return false;
                }
                i++;
            }
            return true;
        }
        public bool Equals(VecInt other)
        {
            return this.EqualsTo(other);
        }
        public static bool operator ==(VecInt left, VecInt right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }
        public static bool operator !=(VecInt left, VecInt right)
        {
            return !(left == right);
        }
    }
}
