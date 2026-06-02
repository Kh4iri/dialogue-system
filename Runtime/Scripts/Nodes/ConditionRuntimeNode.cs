using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class ConditionRuntimeNode : DialogueRuntimeNode
    {
        [SerializeReference] public InputPort<bool> Predicate;

        // These are set during runtime graph construction based on the connections in the editor graph. See DialogueGraphImporter.
        [SerializeReference] public DialogueRuntimeNode TrueNextNode;
        [SerializeReference] public DialogueRuntimeNode FalseNextNode;

        /// <summary>
        /// The result of the last evaluation.
        /// Should be set by the <see cref="ConditionRuntimeNode.ExecuteAsync"/> method and used by DialogueBehaviour to determine which node to execute next.
        /// True means TrueNextNode should be executed, False means FalseNextNode should be executed.
        /// </summary>
        public bool LastEvaluatedResult { get; private set; }

        public ConditionRuntimeNode(InputPort<bool> predicate)
        {
            Predicate = predicate;
        }

        public override Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            LastEvaluatedResult = Predicate.GetValue(ctx);
            
            // Node just evaluates and stores result.
            // The DialogueBehaviour is responsible for routing to TrueNextNode or FalseNextNode based on LastEvaluatedResult.
            return Task.CompletedTask;
        }
    }
}
