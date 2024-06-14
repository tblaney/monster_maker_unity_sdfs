using UnityEngine;
using System;

namespace snorri
{
    public class NodeStage
    {
        public Node Origin {get; set;}
        public Map Vars {get; set;}

        public string NodeName { get {return Vars.Get<string>("node", ""); } }
        public bool IsPersisitent { get {return Vars.Get<bool>("is_persistent", false); } }

        public NodeStage(Map vars)
        {
            Vars = vars;
        }

        public void Build()
        {
            LOG.Console("node stage build! " + NodeName);

            Vars.Log(); 

            Origin = new Node(
                nodeName:NodeName, 
                isLinkToTree:true
            );
            Origin.Build();

            NODE.Tree.Log();
        }
        public void Terminate()
        {
            if (Origin != null)
            {
                Origin.Terminate();
            }

            Origin = null;
        }

    }
}