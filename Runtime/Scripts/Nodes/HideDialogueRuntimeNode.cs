using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class HideDialogueRuntimeNode : LinearDialogueRuntimeNode
    {
        public override async Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            await ctx.DialogueViewPreset.HideDialogueAsync(ct);
        }
    }
}
