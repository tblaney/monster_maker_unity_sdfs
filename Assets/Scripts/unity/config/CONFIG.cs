namespace snorri
{
    public static class CONFIG
    {
        // CONFIG Map are globally accessible variables, that are not required to be saved
        public static Map Vars {get; set;}

        public static void Init(Map m)
        {
            if (m == null)
            {
                Vars = new Map();
            } else
            {
                Vars = m;
            }
        }

        public static float TimeDayLength
        {
            get
            {
                return Vars.Get<float>("time_day_length", 10f);
            }
        }
        public static UnityEngine.Material GetMaterial(string materialEntryName)
        {
            Map materialVars = Vars.Get<Map>("materials", null);
            if (materialVars == null)
                return null;

            string materialName = materialVars.Get(materialEntryName, "");
            if (materialEntryName == "")
                return null;

            return RESOURCES.Load<UnityEngine.Material>("materials/" + materialName);
        }
    }
}