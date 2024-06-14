// custom dictionary, with any type of data as long as 
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using System;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;

namespace snorri
{
    [System.Serializable]
    public class Map : INameable
    {   
        public string Name { get; set; }
        public Dictionary<string, object> Elements {get; set;}
        
        public Map()
        {
            Elements = new Dictionary<string, object>();
        }

        [JsonIgnore]
        public int Length 
        {
            get { if (Elements == null) return 0; return Elements.Count; }
        }

        public T Get<T>(string name, T defaultVal = default(T))
        {
            if (Elements == null)
            {
                return defaultVal;
            }
     
            if (name.Contains(":"))
            {
                string[] nameComponents = name.Split(":");
                Map m = Get<Map>(nameComponents[0], null);
                if (m == null)
                    return defaultVal;

                Bag<string> nameComponentsRemaining = new Bag<string>();
                for (int k = 1; k < nameComponents.Length; k++)
                {
                    nameComponentsRemaining.Append(nameComponents[k]);
                }
                string newName = FormName(nameComponentsRemaining);
                return m.Get<T>(newName, defaultVal);
            }   
                
            bool hasVal = Elements.TryGetValue(name, out object o);

            if (hasVal && o is T val)
            {
                return val;
            } else
            {
                return defaultVal;
            }
        }
        public void Set<T>(string name, T val)
        {
            if (Elements == null)
            {
                Elements = new Dictionary<string, object>();
            }

            if (name.Contains(":"))
            {
                string[] nameComponents = name.Split(":");
                Map m = Get<Map>(nameComponents[0], null);
                if (m == null)
                {
                    m = new Map();
                    Set<Map>(nameComponents[0], m);
                }
                
                Bag<string> nameComponentsRemaining = new Bag<string>();
                for (int k = 1; k < nameComponents.Length; k++)
                {
                    nameComponentsRemaining.Append(nameComponents[k]);
                }
                string newName = FormName(nameComponentsRemaining);
                m.Set<T>(newName, val);
                return;
            }    

            bool hasVal = Elements.TryGetValue(name, out object o);
            if (hasVal)
            {
                Elements.Remove(name);
            }

            Elements.Add(name, val as object);
        }

        string FormName(Bag<string> nameComponents)
        {
            if (nameComponents.Length == 1)
                return nameComponents[0];

            string newName = "";
            int i = 0;
            foreach (string s in nameComponents)
            {
                newName += s;
                if (i < nameComponents.Length - 1)
                {
                    newName+=":";
                }
                i++;    
            }

            return newName;
        }

        public bool Has(string name)
        {
            bool hasVal = Elements.ContainsKey(name);
            return hasVal;
        }
        public void Remove(string name)
        {
            if (Elements == null)
                return;

            if (name.Contains(":"))
            {
                string[] nameComponents = name.Split(":");
                Map m = Get<Map>(nameComponents[0], null);
                if (m == null)
                    return;

                Bag<string> nameComponentsRemaining = new Bag<string>();
                for (int k = 1; k < nameComponents.Length; k++)
                {
                    nameComponentsRemaining.Append(nameComponents[k]);
                }
                string newName = FormName(nameComponentsRemaining);
                m.Remove(newName);
                return;
            }  

            Elements.Remove(name);
        }
        public void Log()
        {
            if (Elements == null)
                return;

            LOG.Console($"___MAP-{Name}___");
            LOG.Console(JSON.ToJson(this));
        }
        public void Write(string name)
        {
            JSON.SaveMap(name, this);
        }

        public object GetObject(string name)
        {
            bool hasVal = Elements.TryGetValue(name, out object o);
            if (hasVal)
                return o;
            
            return default(object);
        }

        public void Sync(Map other, Bag<string> exceptions = null)
        {
            foreach (string keyOther in other.Elements.Keys)
            {
                if (exceptions != null && exceptions.Contains(keyOther))
                    continue;
                    
                if (this.Has(keyOther))
                {
                    object o = this.Elements[keyOther];
                    object objectOther = other.Elements[keyOther];
                    //LOG.Console($"map sync found match '{keyOther}', this has type {o.GetType().Name}, other has type {objectOther.GetType().Name}");
                    if (o is Map m)
                    {
                        if (objectOther is Map mOther)
                        {
                            m.Sync(mOther);
                        } else
                        {
                            // other is not a map but ours is
                            this.Remove(keyOther);
                            this.Elements.Add(keyOther, objectOther);
                        }
                    } else
                    {
                        // not a map, so we remove our version
                        this.Remove(keyOther);
                        this.Elements.Add(keyOther, other.Elements[keyOther]);
                    }
                } else
                {
                    //LOG.Console($"map sync did not find match for key {keyOther}");
                    this.Elements.Add(keyOther, other.Elements[keyOther]);
                }
            }
        }

        public static Map FromJson(string name, string typeName = "")
        {
            Map map = JSON.GetResourceMap(name, typeName);
        
            return map;
        }
        public static Map FromJsonSave(string name)
        {
            Map map = JSON.GetSaveMap(name);

            return map;
        }
    }
}