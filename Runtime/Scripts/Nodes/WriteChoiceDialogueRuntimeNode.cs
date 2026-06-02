using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class WriteChoiceDialogueRuntimeNode : DialogueRuntimeNode
    {
        [SerializeReference] public InputPort<string> SpeakerName;
        [SerializeReference] public InputPort<string> DialogueText;
        [SerializeReference] public InputPort<Sprite> Portrait;
        [SerializeReference] public InputPort<AudioSource> VoiceSource;
        [SerializeReference] public InputPort<AudioClip> VoiceClip;
        [SerializeReference] public List<DialogueChoiceData> Choices;

        /// <summary>
        /// The choice that the player has selected after this node has executed.
        /// Assigned at runtime when the player makes a choice.
        /// </summary>
        public DialogueChoiceData SelectedChoice;

        public WriteChoiceDialogueRuntimeNode(InputPort<string> speakerName, InputPort<string> dialogueText, InputPort<Sprite> portrait, InputPort<AudioSource> voiceSource, InputPort<AudioClip> voiceClip, List<DialogueChoiceData> choices) : base()
        {
            SpeakerName = speakerName;
            DialogueText = dialogueText;
            Portrait = portrait;
            VoiceSource = voiceSource;
            VoiceClip = voiceClip;
            Choices = choices;
        }

        public override async Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            SelectedChoice = null;

            // Reset dialogue
            var view = ctx.DialogueViewPreset;
            view.SetSpeakerName(SpeakerName.GetValue(ctx));
            view.SetDialogueText(string.Empty);
            view.SetPortrait(Portrait.GetValue(ctx));
            view.ClearChoices();
            
            // Show dialogue view, write dialogue while voice is playing, then show choices
            await view.ShowDialogueAsync(ct);
            using var voicePlayback = ctx.BeginVoicePlayback(VoiceSource.GetValue(ctx), VoiceClip.GetValue(ctx));
            await view.WriteDialogueTextAsync(DialogueText.GetValue(ctx), ct);
            view.ShowChoices(Choices.ConvertAll(c => c.ChoiceText.GetValue(ctx)));

            var choiceIndex = await view.WaitForChoiceSelection(ct);
            if (choiceIndex < 0 || choiceIndex >= Choices.Count)
                throw new IndexOutOfRangeException($"Choice index {choiceIndex} is out of range for {Choices.Count} choices.");

            SelectedChoice = Choices[choiceIndex];
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
        /// This is set by <see cref="DialogueGraphImporter"/> when linking runtime nodes together based on the graph connections in the editor.
        /// </summary>
        [SerializeReference] public DialogueRuntimeNode NextNode;

        public DialogueChoiceData(InputPort<string> choiceText)
        {
            ChoiceText = choiceText;
        }
    }
}
