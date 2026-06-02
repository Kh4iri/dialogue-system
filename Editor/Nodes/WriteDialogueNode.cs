using System;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class WriteDialogueNode : LinearDialogueNode
    {
        public const string SpeakerInputName = "Speaker";
        public const string DialogueTextInputName = "Dialogue";
        public const string PortraitInputName = "Portrait";
        public const string VoiceSourceInputName = "VoiceSource";
        public const string VoiceClipInputName = "VoiceClip";

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);

            ctx.AddInputPort<string>(SpeakerInputName).Build();
            ctx.AddInputPort<string>(DialogueTextInputName).Build();
            ctx.AddInputPort<Sprite>(PortraitInputName).Build();
            ctx.AddInputPort<AudioSource>(VoiceSourceInputName).Build();
            ctx.AddInputPort<AudioClip>(VoiceClipInputName).Build();
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            var speakerPort = GetInputPortByName(SpeakerInputName).ToInputPort<string>();
            var dialoguePort = GetInputPortByName(DialogueTextInputName).ToInputPort<string>();
            var portraitPort = GetInputPortByName(PortraitInputName).ToInputPort<Sprite>();
            var voiceSourcePort = GetInputPortByName(VoiceSourceInputName).ToInputPort<AudioSource>();
            var voiceClipPort = GetInputPortByName(VoiceClipInputName).ToInputPort<AudioClip>();

            var runtimeNode = new WriteDialogueRuntimeNode(speakerPort, dialoguePort, portraitPort, voiceSourcePort, voiceClipPort);
            return runtimeNode;
        }
    }
}
