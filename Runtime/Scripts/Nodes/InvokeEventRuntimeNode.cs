using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class InvokeEventRuntimeNode : LinearDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<string> EventName;
        [SerializeReference] public InputPort<object>[] Parameters;

        public InvokeEventRuntimeNode(InputPort<string> eventNamePort, InputPort<object>[] parameterPorts)
        {
            EventName = eventNamePort;
            Parameters = parameterPorts;
        }

        public override Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            var eventName = EventName.GetValue(ctx);
            var parameters = new object[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++)
            {
                parameters[i] = Parameters[i].GetValue(ctx);
            }

            ctx.InvokeEvent(eventName, parameters);
            return Task.CompletedTask;
        }
    }
}
