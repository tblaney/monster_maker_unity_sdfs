namespace snorri
{
    public class AnimateActor : Actor
    {
        public Map AnimationMap { get { return Vars.Get<Map>("animations", null); } }

        public void Link()
        {
            
        }
    }
}