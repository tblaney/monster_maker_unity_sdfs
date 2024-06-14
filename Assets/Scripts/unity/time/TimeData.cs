using System;
using UnityEngine;

namespace snorri
{
    [System.Serializable]
    public class TimeData : Element
    {
        public Map Vars {get; set;}

        public TimeData()
        {
            this.Name = "time_data";
            Vars = new Map();
        }
        public TimeData(Map vars)
        {
            this.Name = "time_data";
            Vars = vars;
        }

        public float Time
        {
            get { return Vars.Get<float>("time", 0f); }
            set { Vars.Set<float>("time", value); }
        }
        public int Day
        {
            get { return Vars.Get<int>("Day", 0); }
            set { Vars.Set<int>("Day", value); }
        }
        public int Hour
        {
            get { return Vars.Get<int>("hour", 0); }
            set { Vars.Set<int>("hour", value); }
        }
        public int Minute
        {
            get { return Vars.Get<int>("min", 0); }
            set { Vars.Set<int>("min", value); }
        }
        public float DayNormalized
        {
            get { return Vars.Get<float>("day_normalized", 0f); }
            set { Vars.Set<float>("day_normalized", value); }
        }

        public void TickTime()
        {
            float time = this.Time + TIME.Delta;

            this.Time = time;

            float totalSecondsInDay = CONFIG.TimeDayLength; // Total Day length in seconds
            float r = time % totalSecondsInDay;

            if (Mathf.Equals(r, 0f))
            {
                this.Day = this.Day + 1;
                this.Hour = 0;
                this.Minute = 0;
                this.DayNormalized = 0f;

                new Trigger("time_new_day");
            } 
            else
            {
                this.DayNormalized = r / totalSecondsInDay;

                float secondsInHour = ((float)(CONFIG.TimeDayLength))/24f; // 60 minutes * 60 seconds
                float secondsInMinute = ((float)(secondsInHour))/60f;

                int hours = (int)(r / secondsInHour);
                this.Hour = hours;

                int minutes = (int)((r % secondsInHour) / secondsInMinute);
                this.Minute = minutes;
            }
        }
    }
}