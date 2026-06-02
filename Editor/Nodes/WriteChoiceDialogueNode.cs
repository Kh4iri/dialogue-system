using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class WriteChoiceDialogueNode : DialogueNode, ICheckErrors
    {
        public const string ChoicesCountOptionName = "ChoicesCount";
        public const string ChoiceInputPrefix = "Choice";
        public const string ResultOutputPrefix = "Result";

        protected override void OnDefineOptions(IOptionDefinitionContext ctx)
        {
            ctx.AddOption<int>(ChoicesCountOptionName)
                .WithDefaultValue(2)
                .WithDisplayName("Number of Choices")
                .WithTooltip("The number of dialogue choices to present to the player. This will determine how many choice input ports and result output ports are generated for this node.")
                .Delayed()
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputFlowPort(ctx);

            // Base dialogue ports
            ctx.AddInputPort<string>(WriteDialogueNode.SpeakerInputName).Build();
            ctx.AddInputPort<string>(WriteDialogueNode.DialogueTextInputName).Build();
            ctx.AddInputPort<Sprite>(WriteDialogueNode.PortraitInputName).Build();
            ctx.AddInputPort<AudioSource>(WriteDialogueNode.VoiceSourceInputName).Build();
            ctx.AddInputPort<AudioClip>(WriteDialogueNode.VoiceClipInputName).Build();

            // Choice ports - dynamically defined based on the number of choices specified in the node options
            GetNodeOptionByName(ChoicesCountOptionName).TryGetValue(out int choicesCount);
            for (int i = 0; i < choicesCount; i++)
            {
                // Add input
                ctx.AddInputPort<string>($"{ChoiceInputPrefix}{i}")
                    .WithDisplayName($"{ChoiceInputPrefix} {i}")
                    .Build();

                // Add output
                ctx.AddOutputPort($"{ResultOutputPrefix}{i}")
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .WithDisplayName($"{ResultOutputPrefix} {i}")
                    .Build();
            }
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            // Parse choices from ports to runtime data
            GetNodeOptionByName(ChoicesCountOptionName).TryGetValue(out int choicesCount);
            var choices = new List<DialogueChoiceData>(choicesCount);

            for (int i = 0; i < choicesCount; i++)
            {
                var choicePort = GetInputPortByName($"{ChoiceInputPrefix}{i}").ToInputPort<string>();
                choices.Add(new DialogueChoiceData(choicePort));
            }

            var speakerPort = GetInputPortByName(WriteDialogueNode.SpeakerInputName).ToInputPort<string>();
            var dialoguePort = GetInputPortByName(WriteDialogueNode.DialogueTextInputName).ToInputPort<string>();
            var portraitPort = GetInputPortByName(WriteDialogueNode.PortraitInputName).ToInputPort<Sprite>();
            var voiceSourcePort = GetInputPortByName(WriteDialogueNode.VoiceSourceInputName).ToInputPort<AudioSource>();
            var voiceClipPort = GetInputPortByName(WriteDialogueNode.VoiceClipInputName).ToInputPort<AudioClip>();
            var runtimeNode = new WriteChoiceDialogueRuntimeNode(speakerPort, dialoguePort, portraitPort, voiceSourcePort, voiceClipPort, choices);
            return runtimeNode;
        }

        public override void LinkRuntimeNode(DialogueRuntimeNode runtimeNode, IReadOnlyDictionary<IDialogueNode, DialogueRuntimeNode> runtimeNodes)
        {
            if (runtimeNode is not WriteChoiceDialogueRuntimeNode choiceRuntimeNode)
                throw new InvalidOperationException($"Expected runtime node of type does not match editor node type.");

            GetNodeOptionByName(ChoicesCountOptionName).TryGetValue(out int choicesCount);


            for (int choiceIndex = 0; choiceIndex < choicesCount; choiceIndex++)
            {
                var nextNode = GetOutputPortByName($"{ResultOutputPrefix}{choiceIndex}").firstConnectedPort?.GetNode();
                if (nextNode is not IDialogueNode nextChoiceNode)
                    continue;

                if (runtimeNodes.TryGetValue(nextChoiceNode, out var nextRuntimeNode))
                    choiceRuntimeNode.Choices[choiceIndex].NextNode = nextRuntimeNode;
            }
        }

        public void CheckErrors(GraphLogger logger)
        {
            var choicesCountOption = GetNodeOptionByName(ChoicesCountOptionName);
            choicesCountOption.TryGetValue(out int choicesCount);

            if (choicesCount <= 0)
                logger.LogError("The number of choices must be greater than zero.", this);

            var connectedOutputPorts = new List<IPort>();
            for (int i = 0; i < choicesCount; i++)
            {
                var outputPort = GetOutputPortByName($"{ResultOutputPrefix}{i}");
                outputPort.GetConnectedPorts(connectedOutputPorts);

                if (connectedOutputPorts.Count > 1)
                {
                    logger.LogWarning($"The output port for choice {i} is connected to multiple nodes. Only the first connection will be used at runtime.", this);
                }
            }
        }
    }
}
