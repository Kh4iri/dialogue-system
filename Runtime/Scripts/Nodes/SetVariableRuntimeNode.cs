using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class SetVariableRuntimeNode : LinearDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<string> Key;
        [SerializeReference] public InputPort<object> Value;

        public SetVariableRuntimeNode(InputPort<string> key, InputPort<object> value)
        {
            Key = key;
            Value = value;
        }

        public override Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            var key = Key.GetValue(ctx);
            var value = Value.GetValue(ctx);
            ctx.SetRuntimeVariable(key, value);
            
            return Task.CompletedTask;
        }
    }
}
