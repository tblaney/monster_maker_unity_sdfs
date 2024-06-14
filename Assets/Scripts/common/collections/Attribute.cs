using Newtonsoft.Json;
namespace snorri
{
    [System.Serializable]
    public class Attribute<T> : Element
    {
        [JsonProperty("val")]
        public T Val { get; set;}

        [JsonConstructor]
        public Attribute()
        {

        }
        public Attribute(T val)
        {
            Val = val;
        }
        public Attribute(string name, T val)
        {
            this.Name = name;
            this.Val = val;
        }
    }
}