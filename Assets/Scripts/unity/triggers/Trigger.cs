using UnityEngine;
using System;

namespace snorri
{
    [System.Serializable]
    public class Trigger
    {
        public static event EventHandler<Element> e;

        public Trigger(string name)
        {
            Element elem = new Element(name);
            e?.Invoke(null, elem);
        }
        public Trigger(string name, Element elem)
        {
            Element element = new Element(name);
            Attribute<Element> package = new Attribute<Element>(name, elem);
            e?.Invoke(null, package as Element);
        }

             // triggers
        public static readonly string WhenStageChange = "trigger_stage_change";
        public static readonly string WhenContextChange = "trigger_context_change";
        public static readonly string WhenSave = "trigger_save";
        public static readonly string WhenCellUpdate = "trigger_cell_update";
    }
}