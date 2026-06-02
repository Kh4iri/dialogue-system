using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class WriteDialogueSequenceNode : LinearDialogueNode
    {
        public const string DialogueCountOptionName = "DialogueCount";
        public const string DialogueTextInputPrefix = "DialogueText";

        protected override void OnDefineOptions(IOptionDefinitionContext ctx)
        {
            ctx.AddOption<int>(DialogueCountOptionName)
                .WithDefaultValue(2)
                .WithDisplayName("Dialogue Count")
                .WithTooltip("The number of dialogue texts to write in sequence.")
                .Delayed()
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);
            ctx.AddInputPort<string>(WriteDialogueNode.SpeakerInputName).Build();

            GetNodeOptionByName(DialogueCountOptionName).TryGetValue<int>(out var dialogueCount);
            for (int i = 0; i < dialogueCount; i++)
            {
                ctx.AddInputPort<string>($"{DialogueTextInputPrefix}{i}")
                    .WithDisplayName($"Dialogue Text {i + 1}")
                    .Build();
            }
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            // Speaker
            var speakerPort = GetInputPortByName(WriteDialogueNode.SpeakerInputName).ToInputPort<string>();

            // Dialogue texts
            GetNodeOptionByName(DialogueCountOptionName).TryGetValue<int>(out var dialogueCount);
            var dialogueTexts = new InputPort<string>[dialogueCount];
            for (int i = 0; i < dialogueCount; i++)
            {
                dialogueTexts[i] = GetInputPortByName($"{DialogueTextInputPrefix}{i}").ToInputPort<string>();
            }

            var runtimeNode = new WriteDialogueSequenceRuntimeNode(speakerPort, dialogueTexts);
            return runtimeNode;
        }

        public override void CheckErrors(GraphLogger logger)
        {
            base.CheckErrors(logger);

            // Check dialogue count
            GetNodeOptionByName(DialogueCountOptionName).TryGetValue<int>(out var dialogueCount);
            if (dialogueCount < 1)
            {
                logger.LogError("Dialogue count must be at least 1.", this);
            }
        }
    }
}
