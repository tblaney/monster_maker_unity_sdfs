namespace snorri
{
    using UnityEngine;

    public class BodyActor : Actor
    {
        BodyModule module;
        Body body;

        protected override void Setup()
        {
            base.Setup();

            module = Node.GetActor<BodyModule>();

            body = new Body(module, Vars);

            Node.Body = body;
        }
    }
}