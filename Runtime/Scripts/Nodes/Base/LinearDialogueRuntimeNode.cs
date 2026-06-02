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
        /// The next node in the dialogue sequence.
        /// </summary>
        [SerializeReference]
        public DialogueRuntimeNode NextNode;
    }
}
