using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Khairi.DialogueSystem
{
    public class TypewriterEffect : IDisposable
    {
        const int UpdateIntervalMs = 1000 / 60; // 60 FPS
        private static readonly char[] PunctuationMarks = { '.', ',', '!', '?', ';', ':', '-' };

        public float CharacterDelay = 0.05f;
        public float PunctuationDelayMultiplier = 3f;

        public bool IsWriting => _isWriting;

        private Label _label;
        private int _visibleCharacters;
        private int _totalCharacters;
        private float _timeAccumulator;
        private bool _isWriting;
        private IVisualElementScheduledItem _animationJob;
        private TaskCompletionSource<bool> _completionSource;
        private CancellationTokenSource _cts;

        public TypewriterEffect(Label label)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));

            label.PostProcessTextVertices += OnPostProcessTextVertices;
            _label = label;
            _animationJob = label.schedule.Execute(UpdateTime).Every(UpdateIntervalMs);
            _animationJob.Pause(); // Pause the job until writing starts
        }

        public TypewriterEffect(Label label, float characterDelay = 0.05f, float punctuationDelayMultiplier = 3f) : this(label)
        {
            CharacterDelay = characterDelay;
            PunctuationDelayMultiplier = punctuationDelayMultiplier;
        }

        ~TypewriterEffect()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_label != null)
                _label.PostProcessTextVertices -= OnPostProcessTextVertices;

            _animationJob?.Pause();
            _cts?.Cancel();
            _cts?.Dispose();
            _completionSource?.TrySetCanceled();
        }

        private void UpdateTime(TimerState state)
        {
            if (!_isWriting || _label == null)
                return;

            _timeAccumulator += state.deltaTime / 1000f;

            float currentDelay = GetCurrentCharacterDelay();
            while (_timeAccumulator >= currentDelay && _visibleCharacters < _totalCharacters)
            {
                if (_cts != null && _cts.Token.IsCancellationRequested)
                {
                    Stop();
                    return;
                }

                _visibleCharacters++;
                _timeAccumulator -= currentDelay;
                currentDelay = GetCurrentCharacterDelay();
            }

            if (_visibleCharacters >= _totalCharacters)
                CompleteWriting();

            _label.MarkDirtyRepaint();
        }

        private void Write(string text)
        {
            _label.text = text;
            _visibleCharacters = 0;
            _totalCharacters = text.Length;
            _timeAccumulator = 0f;
            _isWriting = true;
            _animationJob?.Resume();
        }

        public async Task WriteAsync(string text, CancellationToken ct = default)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _completionSource = new TaskCompletionSource<bool>();

            Write(text);

            try
            {
                await _completionSource.Task;
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled
            }

            _cts?.Dispose();
            _cts = null;
        }

        public void Skip()
        {
            if (!_isWriting)
                return;

            _visibleCharacters = _totalCharacters;
            CompleteWriting();
        }

        private void CompleteWriting()
        {
            _isWriting = false;
            _animationJob?.Pause();
            _completionSource?.TrySetResult(true);
            _label.MarkDirtyRepaint();
        }

        public void Stop()
        {
            if (!_isWriting)
                return;

            _isWriting = false;
            _animationJob?.Pause();
            _cts?.Cancel();
            _completionSource?.TrySetCanceled();
        }

        private float GetCurrentCharacterDelay()
        {
            if (_visibleCharacters >= _totalCharacters)
                return CharacterDelay;

            char currentChar = GetCharacterAtIndex(_visibleCharacters);
            
            if (IsPunctuation(currentChar))
                return CharacterDelay * PunctuationDelayMultiplier;
            
            return CharacterDelay;
        }

        private char GetCharacterAtIndex(int index)
        {
            var text = _label.text;
            return (index < 0 || index >= text?.Length) ? '\0' : text[index];
        }

        private bool IsPunctuation(char c)
            => Array.IndexOf(PunctuationMarks, c) >= 0;

        private void OnPostProcessTextVertices(TextElement.GlyphsEnumerable glyphs)
        {
            if (!_isWriting || _visibleCharacters >= _totalCharacters)
                return;

            int currentIndex = 0;
            foreach (TextElement.Glyph glyph in glyphs)
            {
                if (currentIndex >= _visibleCharacters)
                {
                    var verts = glyph.vertices;
                    for (int i = 0; i < verts.Length; i++)
                    {
                        var v = verts[i];
                        var tint = v.tint;
                        tint.a = 0;
                        v.tint = tint;
                        verts[i] = v;
                    }
                }
                
                currentIndex++;
            }
        }
    }
}