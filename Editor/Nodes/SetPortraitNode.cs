using System;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class SetPortraitNode : LinearDialogueNode
    {
        public const string PortraitInputName = "Portrait";

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);
            ctx.AddInputPort<Sprite>(PortraitInputName).Build();
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            var portraitPort = GetInputPortByName(PortraitInputName).ToInputPort<Sprite>();
            return new SetPortraitRuntimeNode(portraitPort);
        }
    }
}
