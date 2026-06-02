using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    public interface IDialogueRuntimeNode
    {
        /// <summary>
        /// Executes the dialogue node's logic.
        /// </summary>
        Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default);
    }

    /// <summary>
    /// The base class for all runtime dialogue nodes.
    /// </summary>
    [Serializable]
    public abstract class DialogueRuntimeNode
    {
        public abstract Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default);
    }
}
