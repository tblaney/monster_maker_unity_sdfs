using UnityEngine;
using System;
using Newtonsoft.Json;

namespace snorri
{
    [System.Serializable]
    public class Element : INameable
    {
        // kind of like indexable in the traegis setup

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public virtual int Id { get; set; }

        [JsonConstructor]
        public Element()
        {

        }

        public Element(string name)
        {
            this.Name = name;
        }
        public Element(string name, int id)
        {
            this.Name = name;
            this.Id = id;
        }
    }
}
