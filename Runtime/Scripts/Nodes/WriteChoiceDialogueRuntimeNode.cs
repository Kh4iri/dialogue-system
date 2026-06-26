using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class WriteChoiceDialogueRuntimeNode : ConditionalDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<string> SpeakerName;
        [SerializeReference] public InputPort<string> DialogueText;
        [SerializeReference] public InputPort<AudioSource> VoiceSource;
        [SerializeReference] public InputPort<AudioClip> VoiceClip;
        [SerializeReference] public List<DialogueChoiceData> Choices;

        public WriteChoiceDialogueRuntimeNode(InputPort<string> speakerName, InputPort<string> dialogueText, InputPort<AudioSource> voiceSource, InputPort<AudioClip> voiceClip, List<DialogueChoiceData> choices) : base()
        {
            SpeakerName = speakerName;
            DialogueText = dialogueText;
            VoiceSource = voiceSource;
            VoiceClip = voiceClip;
            Choices = choices;
        }

        public override async Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            // Reset dialogue
            var view = ctx.DialogueViewPreset;
            view.SetSpeakerName(SpeakerName.GetValue(ctx));
            view.SetDialogueText(string.Empty);
            view.ClearChoices();
            
            // Show dialogue view, write dialogue while voice is playing, then show choices
            await view.ShowDialogueAsync(ct);
            using var voicePlayback = ctx.BeginVoicePlayback(VoiceSource.GetValue(ctx), VoiceClip.GetValue(ctx));
            await view.WriteDialogueTextAsync(DialogueText.GetValue(ctx), ct);

            // No need to wait for advance input here, as the choice selection will serve as the advance input for this node.
            // Just show the choices and wait for selection in GetNextNodeAsync.
            view.ShowChoices(Choices.ConvertAll(c => c.ChoiceText.GetValue(ctx)));
        }

        public override async Task<DialogueRuntimeNode> GetNextNodeAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            var choiceIndex = await ctx.DialogueViewPreset.WaitForChoiceSelection(ct);
            if (choiceIndex < 0 || choiceIndex >= Choices.Count)
                throw new IndexOutOfRangeException($"Choice index {choiceIndex} is out of range for {Choices.Count} choices.");
            
            var selectedChoice = Choices[choiceIndex];
            return selectedChoice.NextNode;
        }
    }
    
    [Serializable]
    public class DialogueChoiceData
    {
        /// <summary>
        /// The text to display for this choice in the dialogue UI.
        /// </summary>
        [SerializeReference] public InputPort<string> ChoiceText;

        /// <summary>
        /// The next dialogue node to transition to if this choice is selected.
        /// </summary>
        [SerializeReference] public DialogueRuntimeNode NextNode;

        public DialogueChoiceData(InputPort<string> choiceText)
        {
            ChoiceText = choiceText;
        }
    }
}
