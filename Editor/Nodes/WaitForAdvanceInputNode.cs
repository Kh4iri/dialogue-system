using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    /// <summary>
    /// A node that waits for the player to press the advance input before proceeding.
    /// </summary>
    [Serializable]
    public class WaitForAdvanceInputNode : LinearDialogueNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            var runtimeNode = new WaitForAdvanceInputRuntimeNode();
            return runtimeNode;
        }
    }
}
