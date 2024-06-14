namespace snorri
{
    public enum ActorState
    {
        Setup,
        Launch,

        Enable,
        Disable,

        Destroy,
        
        Tick,
        TickPhysics
    }
    public enum Direction
    {
        Up,
        Down,
        North,
        South,
        East,
        West
    }
}