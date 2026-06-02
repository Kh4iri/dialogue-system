using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    /// <summary>
    /// A dialogue node that waits for the player to press the advance input before proceeding.
    /// </summary>
    [Serializable]
    public class WaitForAdvanceInputRuntimeNode : LinearDialogueRuntimeNode
    {
        public override async Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            await ctx.DialogueViewPreset.WaitForAdvanceInput(ct);
        }
    }
}
