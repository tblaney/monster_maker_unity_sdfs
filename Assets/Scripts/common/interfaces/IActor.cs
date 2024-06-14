using System;
using UnityEngine;

namespace snorri
{
    public interface IActor : INameable
    {
        Node Node { get; set; }
        Map Vars { get; set; }

        void Link(Node n, Map vars);
        void Inform(ActorState state);
    }
}