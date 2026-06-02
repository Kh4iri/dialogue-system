using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class InvokeEventNode : LinearDialogueNode
    {
        public const string ParameterCountOptionName = "ParameterCount";
        public const string EventNameInputName = "EventName";
        public const string ParameterInputPrefix = "Parameter";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(ParameterCountOptionName)
                .WithDisplayName("Parameter Count")
                .WithDefaultValue(0)
                .WithTooltip("The number of parameters to pass when invoking the event.")
                .Delayed()
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);

            // Event name input
            ctx.AddInputPort<string>(EventNameInputName)
                .WithDisplayName("Event Name")
                .Build();
            
            // Parameter inputs
            GetNodeOptionByName(ParameterCountOptionName).TryGetValue<int>(out var parameterCount);
            for (int i = 0; i < parameterCount; i++)
            {
                ctx.AddInputPort<object>($"{ParameterInputPrefix}{i}")
                    .WithDisplayName($"Parameter {i}")
                    .Build();
            }
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            // Event name
            var eventNamePort = GetInputPortByName(EventNameInputName).ToInputPort<string>();

            // Parameters
            GetNodeOptionByName(ParameterCountOptionName).TryGetValue<int>(out var parameterCount);
            var parameterPorts = new InputPort<object>[parameterCount];
            for (int i = 0; i < parameterCount; i++)
            {
                parameterPorts[i] = GetInputPortByName($"{ParameterInputPrefix}{i}").ToInputPort<object>();
            }

            return new InvokeEventRuntimeNode(eventNamePort, parameterPorts);
        }

        public override void CheckErrors(GraphLogger logger)
        {
            base.CheckErrors(logger);

            // Check event name
            var eventNamePort = GetInputPortByName(EventNameInputName).ResolvePortValue<string>();
            if (string.IsNullOrEmpty(eventNamePort))
            {
                logger.LogError("Event name cannot be empty.", this);
            }

            // Check parameter count
            GetNodeOptionByName(ParameterCountOptionName).TryGetValue<int>(out var parameterCount);
            if (parameterCount < 0)
            {
                logger.LogError("Parameter count cannot be negative.", this);
            }
        }
    }
}
