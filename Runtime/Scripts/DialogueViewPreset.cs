using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Khairi.DialogueSystem
{
    public interface IDialogueViewPreset
    {
        /// <summary>
        /// Gets the current speaker name from the dialogue view.
        /// </summary>
        public string SpeakerName { get; }

        /// <summary>
        /// Gets the current dialogue text from the dialogue view.
        /// </summary>
        public string DialogueText { get; }

        /// <summary>
        /// Gets whether the dialogue view is currently visible. Useful for showing & hiding the dialogue-box
        /// in the middle of a conversation without interrupting the flow of the dialogue.
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// Checks that the dialogue view preset is properly set up and can be used without errors.
        /// Usually called by <see cref="DialogueBehaviour"/> on Start() to validate the assigned preset before using it.
        /// </summary>
        public void Validate();

        /// <summary>
        /// Shows the dialogue view. The task completes after the show animation is done.
        /// </summary>
        public Task ShowDialogueAsync(CancellationToken ct = default);

        /// <summary>
        /// Hides the dialogue view. The task completes after the hide animation is done.
        /// </summary>
        public Task HideDialogueAsync(CancellationToken ct = default);

        /// <summary>
        /// Writes the text to the dialogue with a typewriter effect, keeping the speaker and others unchanged.
        /// </summary>
        public Task WriteDialogueTextAsync(string text, CancellationToken ct = default);

        /// <summary>
        /// Shows the dialogue choices in the dialogue view.
        /// Use with <see cref="WaitForChoiceSelection"/> to get the selected choice.
        /// </summary>
        public void ShowChoices(List<string> choices);
        public void ClearChoices();

        /// <summary>
        /// Creates a <see cref="TaskCompletionSource{bool}"/> to wait for the player to indicate they want to continue.
        /// </summary>
        public Task WaitForAdvanceInput(CancellationToken ct = default);

        /// <summary>
        /// Creates a <see cref="TaskCompletionSource{int}"/> to wait for the player to select a dialogue choice.
        /// Returns the index of the selected choice.
        /// </summary>
        public Task<int> WaitForChoiceSelection(CancellationToken ct = default);

        // Manual setters
        public void SetSpeakerName(string name);
        public void SetDialogueText(string text);
        public void SetPortrait(Sprite portrait);
        public void SetFont(Font speakerFont, Font dialogueFont);
        public void SetFont(FontAsset speakerFont, FontAsset dialogueFont);

        /// <summary>
        /// Performs a fade-in to black and fade-out from black transition over the specified duration.
        /// The task completes after the fade-in is done. This is not affected by time scale.
        /// </summary>
        public Task DoFadeTransition(float duration = 1f, CancellationToken ct = default);
    }

    public abstract class DialogueViewPreset : ScriptableObject, IDialogueViewPreset
    {
        public abstract string SpeakerName { get; }
        public abstract string DialogueText { get; }
        public abstract bool IsVisible { get; }

        public abstract void Validate();
        public abstract Task ShowDialogueAsync(CancellationToken ct = default);
        public abstract Task HideDialogueAsync(CancellationToken ct = default);
        public abstract Task WriteDialogueTextAsync(string text, CancellationToken ct = default);

        public abstract void ShowChoices(List<string> choices);
        public abstract void ClearChoices();

        public abstract Task WaitForAdvanceInput(CancellationToken ct = default);
        public abstract Task<int> WaitForChoiceSelection(CancellationToken ct = default);

        public abstract void SetSpeakerName(string name);
        public abstract void SetDialogueText(string text);
        public abstract void SetPortrait(Sprite portrait);
        public abstract void SetFont(Font speakerFont, Font dialogueFont);
        public abstract void SetFont(FontAsset speakerFont, FontAsset dialogueFont);
        public abstract Task DoFadeTransition(float duration = 1f, CancellationToken ct = default);
    }
}
