using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class WriteDialogueRuntimeNode : LinearDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<string> SpeakerName;
        [SerializeReference] public InputPort<string> DialogueText;
        [SerializeReference] public InputPort<AudioSource> VoiceSource;
        [SerializeReference] public InputPort<AudioClip> VoiceClip;

        public WriteDialogueRuntimeNode(InputPort<string> speakerName, InputPort<string> dialogueText, InputPort<AudioSource> voiceSource, InputPort<AudioClip> voiceClip)
        {
            SpeakerName = speakerName;
            DialogueText = dialogueText;
            VoiceSource = voiceSource;
            VoiceClip = voiceClip;
        }

        public override async Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            var view = ctx.DialogueViewPreset;
            view.SetSpeakerName(SpeakerName.GetValue(ctx));
            view.SetDialogueText(string.Empty);
            view.ClearChoices();

            // Show dialogue view, then write dialogue while voice plays.
            await view.ShowDialogueAsync(ct);
            using var voicePlayback = ctx.BeginVoicePlayback(VoiceSource.GetValue(ctx), VoiceClip.GetValue(ctx));

            await view.WriteDialogueTextAsync(DialogueText.GetValue(ctx), ct);
            await view.WaitForAdvanceInput(ct);
        }
    }
}
