using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class ConditionRuntimeNode : ConditionalDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<bool> Predicate;
        [SerializeReference] public DialogueRuntimeNode TrueNextNode;
        [SerializeReference] public DialogueRuntimeNode FalseNextNode;

        public ConditionRuntimeNode(InputPort<bool> predicate)
        {
            Predicate = predicate;
        }

        public override Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            // No execution logic needed for this node, as it simply evaluates a condition to determine the next node.
            // The actual logic for determining the next node is implemented in GetNextNodeAsync.
            return Task.CompletedTask;
        }

        public override Task<DialogueRuntimeNode> GetNextNodeAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            // Return the next node based on the value of the predicate
            bool result = Predicate.GetValue(ctx);
            return Task.FromResult(result ? TrueNextNode : FalseNextNode);
        }
    }
}
