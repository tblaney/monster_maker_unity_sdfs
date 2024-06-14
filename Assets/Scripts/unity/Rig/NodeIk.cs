namespace snorri
{
    using UnityEngine;
    using System.Collections.Generic;

    public class NodeIk
    {
        Bag<Node> nodes;
        Node effectorTarget;
        Node effectorPole;
        Node effectorRoot;
        Bag<float> lengths;

        Map args;
        bool isEndRotate = true;
        int iterations = 10;

        // root node wants to point to pole always

        public NodeIk(Bag<Node> nodes, Bag<float> lengths,
            Node effectorTarget, Node effectorPole, Node effectorRoot, 
            Map args = null)
        {
            this.nodes = nodes;
            this.effectorTarget = effectorTarget;
            this.effectorPole = effectorPole;
            this.effectorRoot = effectorRoot;
            this.lengths = lengths;

            isEndRotate = lengths[0] > 0.01f;

            LOG.Console("node ik setup with is end rotate: " + isEndRotate);

            this.args = args;
            if (this.args == null)
                this.args = new Map();     

            iterations = this.args.Get<int>("iterations", 10);

            InitIk();
        }
        
        void InitIk()
        {

        }

        public void UpdateLengths(Bag<float> lengths)
        {
            this.lengths = lengths;
        }

        public void SolveIk()
        {
            Vector3 rootPoint = effectorRoot.transform.position;
            nodes[nodes.Length - 1].transform.up = -(effectorPole.transform.position - nodes[nodes.Length - 1].transform.position);
            
            for (int i = nodes.Length - 2; i >= 0; i--)
            {
                nodes[i].transform.position = nodes[i + 1].transform.position + (-nodes[i + 1].transform.up * lengths[i + 1]);
                nodes[i].transform.up = -(effectorPole.transform.position - nodes[i].transform.position);
            }
            
            for (int i = 0; i < iterations; i++)
            {
                if (isEndRotate)
                    nodes[0].transform.up = -(effectorTarget.transform.position - nodes[0].transform.position);
                
                nodes[0].transform.position = effectorTarget.transform.position - (-nodes[0].transform.up * lengths[0]);
                
                for (int j = 1; j < nodes.Length; j++)
                {
                    nodes[j].transform.up = -(nodes[j - 1].transform.position - nodes[j].transform.position);
                    nodes[j].transform.position = nodes[j - 1].transform.position - (-nodes[j].transform.up * lengths[j]);
                }

                nodes[nodes.Length - 1].transform.position = rootPoint;
                for (int j = nodes.Length - 2; j >= 0; j--)
                {
                    nodes[j].transform.position = nodes[j + 1].transform.position + (-nodes[j + 1].transform.up * lengths[j + 1]);
                }

                /*
                nodes[nodes.Length - 1].transform.position = Vector3.Lerp(nodes[nodes.Length - 1].transform.position, rootPoint, TIME.Delta*8f);
                for (int j = nodes.Length - 2; j >= 0; j--)
                {
                    nodes[j].transform.position = Vector3.Lerp(nodes[j].transform.position, nodes[j + 1].transform.position + (-nodes[j + 1].transform.up * lengths[j + 1]), TIME.Delta*8f);
                }
                */
            }
        }
    }
}