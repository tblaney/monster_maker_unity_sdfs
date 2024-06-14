namespace snorri
{
    public class MotionRig : Element
    {
        public Map Vars;
        public Node Node { get; set; }

        public MotionRig(Map vars, Node n)
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