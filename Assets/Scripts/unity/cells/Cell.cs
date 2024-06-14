namespace snorri
{
    public class Cell
    {
        public int x, y;
        public Map Vars {get; set;}
        public Cell(string key, Map vars)
        {
            string[] splits = key.Split('.');
            x = System.Int32.Parse(splits[0]);
            y = System.Int32.Parse(splits[1]);

            Vars = vars;
        }

        public string Key 
        {
            get { return $"{this.x}.{this.y}"; }
        }

    }
}