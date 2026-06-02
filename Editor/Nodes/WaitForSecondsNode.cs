using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    /// <summary>
    /// A dialogue node that waits for a specified number of seconds before continuing.
    /// </summary>
    [Serializable]
    public class WaitForSecondsNode : LinearDialogueNode
    {
        public const string WaitModeOptionName = "WaitMode";
        public const string WaitTimeInputName = "WaitTime";

        protected override void OnDefineOptions(IOptionDefinitionContext ctx)
        {
            ctx.AddOption<WaitForSecondsRuntimeNode.Mode>(WaitModeOptionName).Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);
            ctx.AddInputPort<float>(WaitTimeInputName).Build();
        }

        public override void CheckErrors(GraphLogger logger)
        {
            base.CheckErrors(logger);

            var waitTime = GetInputPortByName(WaitTimeInputName).ResolvePortValue<float>();
            if (waitTime < 0f)
            {
                logger.LogError("Wait time cannot be negative.", this);
            }
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            GetNodeOptionByName(WaitModeOptionName).TryGetValue(out WaitForSecondsRuntimeNode.Mode waitMode);
            
            var waitTimePort = GetInputPortByName(WaitTimeInputName).ToInputPort<float>();
            var runtimeNode = new WaitForSecondsRuntimeNode(waitMode, waitTimePort);

            return runtimeNode;
        }
    }
}
