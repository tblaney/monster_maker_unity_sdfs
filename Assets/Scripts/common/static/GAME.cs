
using UnityEngine;
namespace snorri
{
    public static class GAME
    {
        public static Map Vars {get; set;}

        public static void Init()
        {
            Vars = Map.FromJson("game");

            Vars.Set<Map>("locations", new Map());

            Context = "loading";

            UnityEngine.Physics.gravity = new Vector3(0f, Vars.Get<float>("gravity", -15f), 0f);
        }

        public static string Stage
        {
            get
            {
                if (Vars == null)
                    return "start";

                return Vars.Get<string>("stage_current", "start");   
            }
            set
            {
                Vars.Set<string>("stage_current", value);
                new Trigger(Trigger.WhenStageChange);
            }
        }
        public static string Context
        {
            get
            {
                if (Vars == null)
                    return "loading";

                return Vars.Get<string>("context_current", "loading");   
            }
            set
            {
                Vars.Set<string>("context_current", value);
                new Trigger(Trigger.WhenContextChange);
            }
        }
        public static Map Locations
        {
            get {
                if (Vars == null)
                    return null;

                return Vars.Get<Map>("locations", new Map());
            }
        }
        public static Node GetLocation(string name)
        {
            Map locationMap = Locations;
            return locationMap.Get<Node>(name, null);
        }
    }
}