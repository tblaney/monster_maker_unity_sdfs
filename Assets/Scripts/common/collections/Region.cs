namespace snorri
{
    public class Region : Element
    {
        public VecInt[] Limits { get; set; }
        public VecInt Min { get { return Limits[0]; } set { Limits[0] = value; } }
        public VecInt Max { get { return Limits[1]; } set { Limits[1] = value; } }
        public Region(
            VecInt limitMin, VecInt limitMax
        )
        {
            Limits = new VecInt[2];

            Limits[0] = limitMin;
            Limits[1] = limitMax;
        }
        public bool Contains(VecInt position)
        {
            if (position.x >= Limits[0].x && position.x < Limits[1].x
                && position.y >= Limits[0].y && position.y < Limits[1].y
                && position.z >= Limits[0].z && position.z < Limits[1].z)
            {
                return true;
            }
            return false;
        }
        public bool Contains(Vec position)
        {
            if (position.x >= Limits[0].x && position.x < Limits[1].x
                && position.y >= Limits[0].y && position.y < Limits[1].y
                && position.z >= Limits[0].z && position.z < Limits[1].z)
            {
                return true;
            }
            return false;
        }
    }
}