namespace snorri
{
    [UnityEngine.DisallowMultipleComponent]
    public class Attributes : Actor
    {
        public Map Vars {get; set;}

        protected override void Setup()
        {
            base.Setup();

            Vars = new Map();
        }

        public void Set<T>(string name, T val)
        {
            Vars.Set<T>(name, val);
        }
        public T Get<T>(string name, T defaultVal)
        {
            return Vars.Get<T>(name, defaultVal);
        }
        public void Remove(string name)
        {
            Vars.Remove(name);
        }
    }
}