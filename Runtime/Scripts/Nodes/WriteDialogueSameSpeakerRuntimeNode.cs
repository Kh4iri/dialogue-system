using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class WriteDialogueSameSpeakerRuntimeNode : LinearDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<string> DialogueText;
        [SerializeReference] public InputPort<AudioSource> VoiceSource;
        [SerializeReference] public InputPort<AudioClip> VoiceClip;

        public WriteDialogueSameSpeakerRuntimeNode(InputPort<string> dialogueText, InputPort<AudioSource> voiceSource, InputPort<AudioClip> voiceClip)
        {
            DialogueText = dialogueText;
            VoiceSource = voiceSource;
            VoiceClip = voiceClip;
        }

        public override async Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            // Reset dialogue
            var view = ctx.DialogueViewPreset;
            view.SetDialogueText(string.Empty);
            view.ClearChoices();

            // Show dialogue view and write dialogue
            await view.ShowDialogueAsync(ct);
            using var voicePlayback = ctx.BeginVoicePlayback(VoiceSource.GetValue(ctx), VoiceClip.GetValue(ctx));
            await view.WriteDialogueTextAsync(DialogueText.GetValue(ctx), ct);
            await view.WaitForAdvanceInput(ct);
        }
    }
}
