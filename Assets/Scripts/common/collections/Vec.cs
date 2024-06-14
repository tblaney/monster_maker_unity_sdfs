using System;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Mathematics;

namespace snorri
{
    [Serializable]
    public class Vec
    {
        public float[] vec;
        
        public Vec()
        {
            vec = new float[0];
        }
        public Vec(params float[] vals)
        {
            vec = vals;
        }
        public Vec(Bag<float> vecs)
        {
            vec = new float[vecs.Length];
            int i = 0;
            foreach (float f in vecs)
            {
                vec[i] = f;
                i++;
            }
        }
        public Vec(params int[] vals)
        {
            vec = new float[vals.Length];
            int i = 0;
            foreach (int val in vals)
            {
                Set(i, (float)val);
                i++;
            } 
        }
        public Vec(Color color)
        {
            vec = new float[4];
            vec[0] = color.r;
            vec[1] = color.g;
            vec[2] = color.b;
            vec[3] = color.a;
        }
        public Vec(Quaternion quat)
        {
            vec = new float[4];
            vec[0] = quat.x;
            vec[1] = quat.y;
            vec[2] = quat.z;
            vec[3] = quat.w;
        }
        public Vec(Vec vectorCopy)
        {
            vec = new float[vectorCopy.Length];

            int i = 0;
            foreach (float f in vectorCopy.vec)
            {
                Set(i, f);
                i++;
            }
        }
        public Vec(Vector2 vecIn)
        {
            vec = new float[]{vecIn.x, vecIn.y};
        }
        public Vec(Vector3 vecIn)
        {
            vec = new float[]{vecIn.x, vecIn.y, vecIn.z};
        }
        public Vec(VecInt vecIn)
        {
            if (vecIn.Length > 2)
            {
                vec = new float[]{vecIn.x, vecIn.y, vecIn.z};
            } else
            {
                vec = new float[]{vecIn.x, vecIn.y};
            }
        }

        [JsonIgnore]
        public float x
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
        public float y
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
        public float z
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
        [JsonIgnore]
        public float r
        {
            get
            {
                return Get("r");
            }
            set
            {
                Set(0, value);
            }
        }
        [JsonIgnore]
        public float g
        {
            get
            {
                return Get("g");
            }
            set
            {
                Set(1, value);
            }
        }
        [JsonIgnore]
        public float b
        {
            get
            {
                return Get("b");
            }
            set
            {
                Set(2, value);
            }
        }
        [JsonIgnore]
        public float a
        {
            get
            {
                return Get("a");
            }
            set
            {
                Set(3, value);
            }
        }

        public float Get(int idx)
        {
            if (idx >= 0 && idx < vec.Length)
                return vec[idx];
            return 0f;
        }
        public void Set(int i, float val)
        {
            if (i >= 0 && i < vec.Length)
                vec[i] = val;
        }
        public VecInt ToInt()
        {
            return new VecInt(this);
        }
        public Vec Add(Vec vectorAdd)
        {
            Vec vec_out = new Vec(this);
            int i = 0;
            foreach (float val in this.All())
            {
                vec_out.Set(i, val + vectorAdd.Get(i));
                i++;
            }
            return vec_out;
        }
        public Vec Subtract(Vec vectorSubtract)
        {
            Vec vec_out = new Vec(this);
            int i = 0;
            foreach (int val in this.All())
            {
                vec_out.Set(i, val - vectorSubtract.Get(i));
                i++;
            }
            return vec_out;
        }
        public float Get(string val)
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
        public Vector2 vec2
        {
            get
            {
                if (vec.Length < 2)
                {
                    return Vector3.zero;
                } else
                {
                    return new Vector3(Get(0),Get(1));
                }
            }
        }
        [JsonIgnore]
        public Vector3 vec3
        {
            get
            {
                if (vec.Length < 3)
                {
                    return Vector3.zero;
                } else
                {
                    return new Vector3(Get(0),Get(1),Get(2));
                }
            }
        }
        [JsonIgnore]
        public float3 f3
        {
            get
            {
                if (vec.Length < 3)
                {
                    return new float3(0f,0f,0f);
                } else
                {
                    return new float3(Get(0),Get(1),Get(2));
                }
            }
        }
        [JsonIgnore]
        public float2 f2
        {
            get
            {
                if (vec.Length < 2)
                {
                    return new float2(0f,0f);
                } else
                {
                    return new float2(Get(0),Get(1));
                }
            }
        }
        [JsonIgnore]
        public Color color
        {
            get
            {
                if (vec.Length > 3)
                {
                    return new Color(Get(0),Get(1),Get(2),Get(3));
                } else
                {
                    return default(Color);
                }
            }
        }
        [JsonIgnore]
        public Quaternion quat
        {
            get
            {
                if (vec.Length > 3)
                {
                    return new Quaternion(Get(0),Get(1),Get(2),Get(3));
                } else
                {
                    // make a quaternion from a rotation:
                    // LOG.Console($"vec generating quaterion! {Get(0)}, {Get(1)}, {Get(2)}");
                    Quaternion rotation = Quaternion.Euler(Get(0), Get(1), Get(2));
                    return rotation;
                }
            }
        }

        public Vec Lerp(Vec vec_out, float t, bool is_quaternion = false)
        {
            if (vec.Length > 3 && is_quaternion)
            {
                // this is a quaternion
                Quaternion quaternion_this = this.quat;
                Quaternion quaternion_out = vec_out.quat;

                return new Vec(Quaternion.Lerp(quaternion_this, quaternion_out, t));
            }

            Vec Vec = new Vec(this);
            int i = 0;
            foreach (float val in this.vec)
            {
                float valTemp = Mathf.Lerp(this.Get(i), vec_out.Get(i), t);
                Vec.Set(i, valTemp);
                i++;
            }
            return Vec;
        }
        public Bag<Vec> GetNeighbours()
        {
            Bag<Vec> vecs = new Bag<Vec>();
            Bag<Vec> vecsOut = new Bag<Vec>();
            if (vec.Length > 2)
            {
                vecs.Append(new Vec(1, 0, 0));
                vecs.Append(new Vec(-1, 0, 0));
                vecs.Append(new Vec(0, 1, 0));
                vecs.Append(new Vec(0, -1, 0));
                vecs.Append(new Vec(0, 0, 1));
                vecs.Append(new Vec(0, 0, -1));
            } else
            {
                vecs.Append(new Vec(1, 0));
                vecs.Append(new Vec(-1, 0));
                vecs.Append(new Vec(0, 1));
                vecs.Append(new Vec(0, -1));
            }
            foreach (Vec Vec in vecs.All())
            {
                Vec vecNew = new Vec(this);
                vecsOut.Append(vecNew.Add(Vec));
            }

            return vecsOut;
        }
        public float GetRandomRange()
        {
            return UnityEngine.Random.Range(Get(0), Get(1));
        }
        public float[] All()
        {
            return vec;
        }
        public Bag<float> AsBag(){
            return new Bag<float>(All());
        }
        [JsonIgnore]
        public int Length
        {
            get
            {
                return vec.Length;
            }
        }
        public void Set(string axis, float val)
        {
            switch (axis)
            {
                case "x":
                    Set(0, val);
                    break;
                case "y":
                    Set(1, val);
                    break;
                case "z":
                    Set(2, val);
                    break;
            }
        }
        public float Distance(Vec other)
        {
            if (Length == 2 && other.Length == 2)
            {
                return Vector2.Distance(vec2, other.vec2);
            } else
            {
                return Vector3.Distance(vec3, other.vec3);
            }
        }
        public Vec Direction(Vec other)
        {
            Vec newVec = null;
            if (Length == 2 && other.Length == 2)
            {
                newVec = new Vec(other.vec2-this.vec2);
                newVec.Normalize();
            } else
            {
                newVec = new Vec(other.vec3-this.vec3);
                newVec.Normalize();
            }
            return newVec;
        }
        public float Difference(Vec other)
        {
            Vec current = new Vec(this);
            int i = 0;
            foreach (float val in vec)
            {
                current.Set(i, Mathf.Abs(val - other.Get(i)));
                i++;
            }
            return current.Sum();
        }
        public float Sum()
        {
            float valOut = 0f;
            foreach (float val in vec)
            {
                valOut+=val;
            }
            return valOut;
        }
        public Vec Normalize()
        {
            Vec Vec = new Vec(this);
            if (Length == 2)
            {
                Vector2 vec2 = Vec.vec2;
                vec2 = vec2.normalized;
                Vec = new Vec(vec2);
            } else
            {
                Vector3 vec3 = Vec.vec3;
                vec3 = vec3.normalized;
                Vec = new Vec(vec3);
            }
            vec = Vec.vec;

            return this;
        }
        public float Magnitude()
        {
            if (Length == 2)
            {
                return vec2.magnitude;
            } else
            {
                return vec3.magnitude;
            }
        }
        public Vec Multiply(float factor)
        {
            Vec current = new Vec(this.vec);
            int i = 0;
            foreach (float val in vec)
            {
                current.Set(i, val*factor);
                i++;
            }
            return current;
        }
        public Vec Divide(float factor)
        {
            Vec current = new Vec(this.vec);
            int i = 0;
            foreach (float val in vec)
            {
                current.Set(i, val/factor);
                i++;
            }
            return current;
        }
        public Vec Mulitply(Vec other)
        {
            Vec newVec = new Vec(this.vec);
            int i = 0;
            foreach (float val in newVec.vec)
            {
                float oVal = other.Get(i);
                if (oVal != null)
                {
                    newVec.Set(i, val*oVal);
                }
                i++;
            }
            return newVec;
        }
        public float Dot(Vec other)
        {
            if (Length == 2)
            {
                return Vector2.Dot(vec2, other.vec2);
            } else
            {
                return Vector3.Dot(vec3, other.vec3);
            }
        }
        public Vec Cross(Vec other)
        {
            if (Length == 3)
            {
                return new Vec(Vector3.Cross(vec3, other.vec3));
            }
            return this;
        }
        public string Text()
        {
            string Text = "";
            int i = 1;
            foreach (float val in vec)
            {
                Text += val.ToString();
                if (i < Length)
                {
                    Text += ", ";
                }
                i++;
            }
            return Text;
        }
        [JsonIgnore]
        public static Vec zero
        { 
            get
            {
                return new Vec(0,0,0);
            }
        }
        [JsonIgnore]
        public static Vec ones
        {
            get
            {
                return new Vec(1,1,1);
            }
        }
        [JsonIgnore]
        public static Vec zero2
        {
            get
            {
                return new Vec(0,0);
            }
        }
        [JsonIgnore]
        public static Vec up
        {
            get
            {
                return new Vec(0,1,0);
            }
        }
        public Vec GetRandomInRadius(float radius = 1f)
        {
            // adds a GetRandomInRadius Vec
            Vec Vec = new Vec(this);
            Vec = Vec.Add(new Vec(UnityEngine.Random.insideUnitSphere*radius));
            return Vec;
        }
        public bool EqualsTo(Vec other)
        {
            return Vector3.Equals(this.vec3, other.vec3);
        }

        //---overrides---//
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            Vec otherVec = other as Vec;
            if (otherVec == null)
                return false;

            if (Length != otherVec.Length)
                return false;

            int i = 0;
            foreach (float val in vec)
            {
                if (!Mathf.Equals(val, otherVec.Get(i)))
                {
                    return false;
                }
                i++;
            }
            return true;
        }
        public override string ToString()
        {
            return base.ToString();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
