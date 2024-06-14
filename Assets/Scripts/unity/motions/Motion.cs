namespace snorri
{
    public class Motion : Element
    {
        public Map Vars;

        public float GetDuration(int fps)
        {
            int frameAmount = Vars.Get<int>("data:frame_count", 1);
            float duration = ((float)frameAmount)/((float)fps);
            return duration;
        }

        public Motion(Map vars)
        {
            Vars = new Map();
            Vars.Set<Map>("data", vars);
            this.Name = vars.Get<string>("name", "");
        }

        public PointState GetState(Map conditions)
        {
            string rigName = conditions.Get<string>("rig", "");
            
            // first decide upper animation if this is not a singlet

            // lerp motion
            return null;
        }   
    }   
}