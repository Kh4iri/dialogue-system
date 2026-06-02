using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class FadeTransitionRuntimeNode : LinearDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<float> Duration;

        public FadeTransitionRuntimeNode(InputPort<float> durationPort)
        {
            Duration = durationPort;
        }

        public override async Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            await ctx.DialogueViewPreset.DoFadeTransition(Duration.GetValue(ctx), ct);
        }
    }
}
