using System.Collections.Generic;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    /// <summary>
    /// The runtime representation of a dialogue graph.
    /// </summary>
    public class DialogueGraphAsset : ScriptableObject
    {
        /// <summary>
        /// The entry node of the dialogue graph.
        /// </summary>
        [SerializeReference]
        public DialogueRuntimeNode EntryNode;

        /// <summary>
        /// All nodes in the dialogue graph.
        /// </summary>
        [SerializeReference]
        public List<DialogueRuntimeNode> Nodes = new();

        /// <summary>
        /// All the local variables used in the dialogue graph.
        /// </summary>
        [SerializeReference]
        public List<IRuntimeVariable> Variables = new();
    }
}
