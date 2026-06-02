using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using EditorAttributes;
using DG.Tweening;

namespace Khairi.DialogueSystem
{
    [RequireComponent(typeof(UIDocument))]
    public class NarrativeBoxDialogueView : MonoBehaviour
    {
        #region Singleton
        private static NarrativeBoxDialogueView _instance;
        public static NarrativeBoxDialogueView Instance {
            get {
                if (_instance == null)
                    _instance = FindAnyObjectByType<NarrativeBoxDialogueView>(FindObjectsInactive.Include);
                return _instance;
            }
        }
        #endregion

        [SerializeField, Required] private UIDocument _uiDocument;
        [SerializeField] private UnityEngine.UI.Image _fadeImage;

        [Header("Typewriter")]
        [SerializeField] private float _typewriterCharacterDelay = 0.03f;
        [SerializeField] private float _typewriterPunctuationDelayMultiplier = 6f;

        private const float UIAnimationDuration = 0.3f;
        private const string ChoiceButtonClass = "dialogue__choice";

        public Label SpeakerLabel => _speakerLabel;
        public Label DialogueLabel => _dialogueLabel;
        public bool IsVisible => _isVisible;

        private VisualElement _container;
        private VisualElement _dialogueBox;
        private VisualElement _choicesContainer;
        private VisualElement _portraitElement;
        private Label _speakerLabel;
        private Label _dialogueLabel;
        private VisualElement _clickArea;
        private StyleFontDefinition _defaultSpeakerFont;
        private StyleFontDefinition _defaultDialogueFont;
        private StyleFontDefinition? _currentDialogueFont = null;
        private bool _isVisible;

        private TypewriterEffect _typewriter;
        private TaskCompletionSource<bool> _advanceTcs;
        private TaskCompletionSource<int> _choiceTcs;

        private Tween _fadeTween;

        private void Reset()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void Awake()
        {
            _uiDocument.enabled = true;

            _container = _uiDocument.rootVisualElement.Q("Container");
            _container.style.translate = new(new Translate(0, new Length(100f, LengthUnit.Percent)));
            _container.style.display = DisplayStyle.None;

            _dialogueBox = _container.Q("DialogueBox");
            _choicesContainer = _container.Q("ChoicesContainer");
            _speakerLabel = _container.Q<Label>("SpeakerNameLabel");
            _dialogueLabel = _container.Q<Label>("DialogueTextLabel");
            _portraitElement = _container.Q("Portrait");
            _clickArea = _container.Q("ClickArea");

            _typewriter = new(_dialogueLabel, _typewriterCharacterDelay, _typewriterPunctuationDelayMultiplier);

            _defaultSpeakerFont = _speakerLabel.style.unityFontDefinition;
            _defaultDialogueFont = _dialogueLabel.style.unityFontDefinition;
        }

        private void Start()
        {
            _clickArea.RegisterCallback<ClickEvent>(OnClickToAdvanceAreaClicked);
            _dialogueBox.RegisterCallback<ClickEvent>(OnClickToAdvanceAreaClicked);
        }

        private void OnDestroy()
        {
            _advanceTcs?.TrySetCanceled();
            _choiceTcs?.TrySetCanceled();
            _typewriter.Dispose();

            _clickArea.UnregisterCallback<ClickEvent>(OnClickToAdvanceAreaClicked);
            _dialogueBox.UnregisterCallback<ClickEvent>(OnClickToAdvanceAreaClicked);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard.fKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                SkipOrAdvance();
            }
        }

        /// <summary>
        /// Shows the dialogue view.
        /// </summary>
        public async Task ShowDialogueAsync(CancellationToken ct = default)
        {
            if (_isVisible)
                return;

            ct.ThrowIfCancellationRequested();

            _isVisible = true;
            _choicesContainer.style.display = DisplayStyle.None;

            _container.style.display = StyleKeyword.Initial;
            await Task.Yield(); // Wait a few frames to ensure layout is updated before animating
            await Task.Yield();
            _container.style.translate = StyleKeyword.Initial;
            await WaitForSecondsRealtimeAsync(UIAnimationDuration, LinkCancellationTokens(ct, destroyCancellationToken));
        }

        /// <summary>
        /// Hides the dialogue view.
        /// </summary>
        public async Task HideDialogueAsync(CancellationToken ct = default)
        {
            if (!_isVisible)
                return;

            ct.ThrowIfCancellationRequested();

            _isVisible = false;
            _choicesContainer.style.display = DisplayStyle.None;

            _container.style.translate = new(new Translate(0, new Length(100f, LengthUnit.Percent)));
            await WaitForSecondsRealtimeAsync(UIAnimationDuration, LinkCancellationTokens(ct, destroyCancellationToken));
            _container.style.display = DisplayStyle.None;

        }

        public async Task WriteDialogueTextAsync(string text, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _dialogueLabel.text = string.Empty;
            await _typewriter.WriteAsync(text, LinkCancellationTokens(ct, destroyCancellationToken));
        }

        /// <summary>
        /// Shows the dialogue choices in the dialogue view.
        /// Use with <see cref="WaitForChoiceSelection"/> to get the selected choice.
        /// </summary>
        public void ShowChoices(List<string> choices)
        {
            _choicesContainer.Clear();

            for (int i = 0; i < choices.Count; i++)
            {
                int choiceIndex = i; // Capture loop variable for use in lambda
                var button = new Button(() =>
                {
                    if (_choiceTcs != null && !_choiceTcs.Task.IsCompleted)
                        _choiceTcs.SetResult(choiceIndex);
                });

                button.AddToClassList("btn");
                button.AddToClassList(ChoiceButtonClass);
                button.text = choices[i];

                if (_currentDialogueFont.HasValue)
                    button.style.unityFontDefinition = _currentDialogueFont.Value;

                _choicesContainer.Add(button);
            }

            _choicesContainer.style.display = StyleKeyword.Initial;
        }

        public void ClearChoices()
        {
            _choicesContainer.style.display = DisplayStyle.None;
            _choicesContainer.Clear();
        }

        /// <summary>
        /// Creates a <see cref="TaskCompletionSource{bool}"/> to wait for the player to indicate they want to advance (continue).
        /// </summary>
        public Task WaitForAdvanceInput(CancellationToken ct = default)
        {
            if (_advanceTcs == null || _advanceTcs.Task.IsCompleted)
            {
                _advanceTcs = new TaskCompletionSource<bool>();
                var registration = ct.Register(() => _advanceTcs.TrySetCanceled(ct));

                // Optional: Ensure the registration is disposed when the task completes
                _advanceTcs.Task.ContinueWith(
                    continuationAction: _ => registration.Dispose(),
                    cancellationToken: CancellationToken.None,
                    continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
                    scheduler: TaskScheduler.Default
                );
            }
            
            return _advanceTcs.Task;
        }

        /// <summary>
        /// Creates a <see cref="TaskCompletionSource{int}"/> to wait for the player to select a dialogue choice.
        /// Returns the index of the selected choice.
        /// </summary>
        public Task<int> WaitForChoiceSelection(CancellationToken ct = default)
        {
            if (_choiceTcs == null || _choiceTcs.Task.IsCompleted)
            {
                _choiceTcs = new TaskCompletionSource<int>();
                var registration = ct.Register(() => _choiceTcs.TrySetCanceled(ct));

                // Optional: Ensure the registration is disposed when the task completes
                _choiceTcs.Task.ContinueWith(
                    continuationAction: _ => registration.Dispose(),
                    cancellationToken: CancellationToken.None,
                    continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
                    scheduler: TaskScheduler.Default
                );
            }
            
            return _choiceTcs.Task;
        }

        public void SetSpeakerName(string name)
            => _speakerLabel.text = name;

        public void SetDialogueText(string text)
            => _dialogueLabel.text = text;

        public void SetPortrait(Sprite portrait)
        {
            if (portrait != null)
            {
                _portraitElement.style.backgroundImage = new StyleBackground(portrait);
                _portraitElement.style.display = StyleKeyword.Initial;
            }
            else
            {
                _portraitElement.style.display = DisplayStyle.None;
            }
        }

        public void SetFont(Font speakerFont, Font dialogueFont)
        {
            _speakerLabel.style.unityFontDefinition = speakerFont != null ? new StyleFontDefinition(speakerFont) : _defaultSpeakerFont;
            _dialogueLabel.style.unityFontDefinition = dialogueFont != null ? new StyleFontDefinition(dialogueFont) : _defaultDialogueFont;
            _currentDialogueFont = dialogueFont != null ? new StyleFontDefinition(dialogueFont) : null;
        }

        public void SetFont(FontAsset speakerFont, FontAsset dialogueFont)
        {
            _speakerLabel.style.unityFontDefinition = speakerFont != null ? new StyleFontDefinition(speakerFont) : _defaultSpeakerFont;
            _dialogueLabel.style.unityFontDefinition = dialogueFont != null ? new StyleFontDefinition(dialogueFont) : _defaultDialogueFont;
            _currentDialogueFont = dialogueFont != null ? new StyleFontDefinition(dialogueFont) : null;
        }

        /// <summary>
        /// Performs a fade-in to black and fade-out from black transition over the specified duration.
        /// The task completes after the fade-in is done. This is not affected by time scale.
        /// </summary>
        public async Task DoFadeTransition(float duration = 1f, CancellationToken ct = default)
        {
            ct = LinkCancellationTokens(ct, destroyCancellationToken);
            ct.ThrowIfCancellationRequested();

            _fadeImage.gameObject.SetActive(true);
            _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 0f);

            _fadeTween?.Complete();
            _fadeTween = _fadeImage.DOFade(1f, duration / 2f)
                .SetEase(Ease.InOutSine)
                .SetLink(_fadeImage.gameObject)
                .SetUpdate(true)
                .OnUpdate(() =>
                {
                    if (ct.IsCancellationRequested)
                    {
                        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 0f);
                        _fadeImage.gameObject.SetActive(false);
                        throw new System.OperationCanceledException(ct);
                    }
                });

            await _fadeTween.AsyncWaitForCompletion();

            _fadeTween?.Complete();
            _fadeTween = _fadeImage.DOFade(0f, duration / 2f)
                .SetEase(Ease.InOutSine)
                .SetLink(_fadeImage.gameObject)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _fadeImage.gameObject.SetActive(false);
                });
        }

        /// <summary>
        /// Skips the typewriter effect if it's writing, otherwise signals to advance.
        /// </summary>
        private void SkipOrAdvance()
        {
            if (_typewriter.IsWriting)
            {
                _typewriter.Skip();
            }
            else
            {
                _advanceTcs?.TrySetResult(true);
            }
        }

        private CancellationToken LinkCancellationTokens(CancellationToken ct1, CancellationToken ct2)
            => CancellationTokenSource.CreateLinkedTokenSource(ct1, ct2).Token;

        /// <summary>
        /// Waits for the specified duration in real time, ignoring the game's time scale.
        /// </summary>
        public static async Task WaitForSecondsRealtimeAsync(float seconds, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < seconds)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        private void OnClickToAdvanceAreaClicked(ClickEvent evt)
        {
            SkipOrAdvance();
            evt.StopPropagation();
        }
    }
}
