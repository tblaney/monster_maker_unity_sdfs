using UnityEngine;

namespace snorri
{
    public static class TIME
    {
        public static float scale = 1f;

         public static float Delta
        {
            get
            {
                return Time.deltaTime*scale;
            }
        }
        public static float DeltaPhysics
        {
            get
            {
                return Time.fixedDeltaTime;
            }
        }
        public static TimeData Data
        {
            get
            {
                return GAME.Vars.Get<TimeData>("time_data", null);
            }
            set
            {
                GAME.Vars.Set<TimeData>("time_data", value);
            }
        }

    }
}