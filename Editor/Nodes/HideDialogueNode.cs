using System;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class HideDialogueNode : LinearDialogueNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            var runtimeNode = new HideDialogueRuntimeNode();
            return runtimeNode;
        }
    }
}
