using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    /// <summary>
    /// A dialogue node that waits for a specified duration before proceeding.
    /// </summary>
    [Serializable]
    public class WaitForSecondsRuntimeNode : LinearDialogueRuntimeNode
    {
        public enum Mode
        {
            /// <summary>
            /// Waits using the game's time scale.
            /// </summary>
            GameTime,

            /// <summary>
            /// Waits using real time, ignoring the game's time scale.
            /// </summary>
            RealTime
        }

        public Mode WaitMode;
        [SerializeReference] public InputPort<float> WaitTime;

        public WaitForSecondsRuntimeNode(Mode waitMode, InputPort<float> waitTime)
        {
            WaitMode = waitMode;
            WaitTime = waitTime;
        }

        public override async Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            switch (WaitMode)
            {
                case Mode.GameTime:
                    await Awaitable.WaitForSecondsAsync(Mathf.Max(0, WaitTime.GetValue(ctx)), ct);
                    break;
                case Mode.RealTime:
                    await WaitForSecondsRealtimeAsync(Mathf.Max(0, WaitTime.GetValue(ctx)), ct);
                    break;
            }
        }

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
    }
}
