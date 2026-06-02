using System;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class FadeTransitionNode : LinearDialogueNode
    {
        public const string DurationInputName = "Duration";

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);
            ctx.AddInputPort<float>(DurationInputName)
                .WithDefaultValue(1f)
                .Build();
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            var durationPort = GetInputPortByName(DurationInputName).ToInputPort<float>();
            var runtimeNode = new FadeTransitionRuntimeNode(durationPort);
            return runtimeNode;
        }
    }
}
