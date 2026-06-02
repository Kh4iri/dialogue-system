using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class SetPortraitRuntimeNode : LinearDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<Sprite> Sprite;

        public SetPortraitRuntimeNode(InputPort<Sprite> sprite)
        {
            Sprite = sprite;
        }

        public override Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            var sprite = Sprite.GetValue(ctx);
            ctx.DialogueViewPreset.SetPortrait(sprite);
            
            return Task.CompletedTask;
        }
    }
}
