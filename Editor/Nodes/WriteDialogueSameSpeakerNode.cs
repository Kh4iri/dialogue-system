using System;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class WriteDialogueSameSpeakerNode : LinearDialogueNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);

            ctx.AddInputPort<string>(WriteDialogueNode.DialogueTextInputName).Build();
            ctx.AddInputPort<AudioSource>(WriteDialogueNode.VoiceSourceInputName).Build();
            ctx.AddInputPort<AudioClip>(WriteDialogueNode.VoiceClipInputName).Build();
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            var dialogueTextPort = GetInputPortByName(WriteDialogueNode.DialogueTextInputName).ToInputPort<string>();
            var voiceSourcePort = GetInputPortByName(WriteDialogueNode.VoiceSourceInputName).ToInputPort<AudioSource>();
            var voiceClipPort = GetInputPortByName(WriteDialogueNode.VoiceClipInputName).ToInputPort<AudioClip>();

            var runtimeNode = new WriteDialogueSameSpeakerRuntimeNode(dialogueTextPort, voiceSourcePort, voiceClipPort);
            return runtimeNode;
        }
    }
}
