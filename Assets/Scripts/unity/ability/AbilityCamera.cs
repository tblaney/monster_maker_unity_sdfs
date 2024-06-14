namespace snorri
{
    public abstract class AbilityCamera : Ability
    {
        protected CameraModuleCine cm;

        protected override void Setup()
        {
            base.Setup();

            Node n = this.Node;

            string nodeName = Vars.Get<string>("node_camera", "stage_overworld.camera_overworld.camera");
            if (nodeName != "")
            {
                n = NODE.Tree.Get<Node>(nodeName, this.Node);
            }

            LOG.Console($"ability camera found node with name {n.Name}");

            this.cm = n.GetActor<CameraModuleCine>();
        }
        protected override void Launch()
        {
            base.Launch();

            return;

            if (this.cm == null)
            {
                Node n = this.Node;

                string nodeName = Vars.Get<string>("node_camera", "stage_overworld.camera_overworld.camera");
                if (nodeName != "")
                {
                    n = NODE.Tree.Get<Node>(nodeName, this.Node);
                }

                LOG.Console($"ability camera found node with name {n.Name}");

                this.cm = n.GetActor<CameraModuleCine>();
            }
        }
    }
}