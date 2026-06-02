using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class WriteDialogueSequenceRuntimeNode : LinearDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<string> SpeakerName;
        [SerializeReference] public InputPort<string>[] DialogueTexts;

        public WriteDialogueSequenceRuntimeNode(InputPort<string> speaker, InputPort<string>[] dialogueTexts)
        {
            SpeakerName = speaker;
            DialogueTexts = dialogueTexts;
        }

        public override async Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            // Reset dialogue
            var view = ctx.DialogueViewPreset;
            view.SetSpeakerName(SpeakerName.GetValue(ctx));
            view.SetDialogueText(string.Empty);
            view.ClearChoices();

            // Show dialogue view and write dialogue
            await view.ShowDialogueAsync(ct);
            foreach (var dialogueText in DialogueTexts)
            {
                await view.WriteDialogueTextAsync(dialogueText.GetValue(ctx), ct);
                await view.WaitForAdvanceInput(ct);
            }
        }
    }
}
