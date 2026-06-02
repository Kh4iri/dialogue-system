using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    /// <summary>
    /// A dialogue node that determines the next node to execute based on some condition or logic.
    /// The logic for determining the next node is implemented in the <see cref="GetNextNodeAsync"/> method,
    /// which must be overridden by derived classes.
    /// </summary>
    [Serializable]
    public abstract class ConditionalDialogueRuntimeNode : DialogueRuntimeNode
    {
        /// <summary>
        /// Determines the next dialogue node to execute based on some condition or logic.
        /// </summary>
        public abstract Task<DialogueRuntimeNode> GetNextNodeAsync(DialogueBehaviour ctx, CancellationToken ct = default);
    }
}
