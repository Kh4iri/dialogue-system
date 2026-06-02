using System;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    /// <summary>
    /// A dialogue node that has a single linear path to the next node.
    /// </summary>
    [Serializable]
    public abstract class LinearDialogueRuntimeNode : DialogueRuntimeNode
    {
        /// <summary>
        /// The next node in the dialogue sequence. This is set by the <see cref="DialogueGraphImporter"/>
        /// when linking runtime nodes together based on the connections in the editor graph. It is not set by the node itself, as the node does not have direct references to other nodes in the editor graph.
        /// </summary>
        [SerializeReference]
        public DialogueRuntimeNode NextNode;
    }
}
