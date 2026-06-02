using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Khairi.DialogueSystem
{
    [CreateAssetMenu(fileName = "NarrativeBoxDialogueViewPreset", menuName = "Dialogue System/Presets/Narrative Box Dialogue")]
    public class NarrativeBoxDialogueViewPreset : DialogueViewPreset
    {
        public override string SpeakerName => NarrativeBoxDialogueView.Instance.SpeakerLabel.text;
        public override string DialogueText => NarrativeBoxDialogueView.Instance.DialogueLabel.text;
        public override bool IsVisible => NarrativeBoxDialogueView.Instance.IsVisible;

        public override void Validate()
        {
            if (NarrativeBoxDialogueView.Instance == null)
            {
                Debug.LogError($"{nameof(NarrativeBoxDialogueViewPreset)} requires a {nameof(NarrativeBoxDialogueView)} in the scene.");
            }
        }

        // All of the methods below just call the corresponding method on the singleton instance of VisualNovelDialogueView.
        public override void ClearChoices()
            => NarrativeBoxDialogueView.Instance.ClearChoices();

        public override Task DoFadeTransition(float duration = 1, CancellationToken ct = default)
            => NarrativeBoxDialogueView.Instance.DoFadeTransition(duration, ct);

        public override Task HideDialogueAsync(CancellationToken ct = default)
            => NarrativeBoxDialogueView.Instance.HideDialogueAsync(ct);

        public override void ShowChoices(List<string> choices)
            => NarrativeBoxDialogueView.Instance.ShowChoices(choices);

        public override Task ShowDialogueAsync(CancellationToken ct = default)
            => NarrativeBoxDialogueView.Instance.ShowDialogueAsync(ct);

        public override Task WaitForAdvanceInput(CancellationToken ct = default)
            => NarrativeBoxDialogueView.Instance.WaitForAdvanceInput(ct);

        public override Task<int> WaitForChoiceSelection(CancellationToken ct = default)
            => NarrativeBoxDialogueView.Instance.WaitForChoiceSelection(ct);

        public override Task WriteDialogueTextAsync(string text, CancellationToken ct = default)
            => NarrativeBoxDialogueView.Instance.WriteDialogueTextAsync(text, ct);

        public override void SetSpeakerName(string name)
            => NarrativeBoxDialogueView.Instance.SetSpeakerName(name);
        
        public override void SetDialogueText(string text)
            => NarrativeBoxDialogueView.Instance.SetDialogueText(text);
        
        public override void SetPortrait(Sprite portrait)
            => NarrativeBoxDialogueView.Instance.SetPortrait(portrait);

        public override void SetFont(Font speakerFont, Font dialogueFont)
            => NarrativeBoxDialogueView.Instance.SetFont(speakerFont, dialogueFont);

        public override void SetFont(FontAsset speakerFont, FontAsset dialogueFont)
            => NarrativeBoxDialogueView.Instance.SetFont(speakerFont, dialogueFont);
    }
}
