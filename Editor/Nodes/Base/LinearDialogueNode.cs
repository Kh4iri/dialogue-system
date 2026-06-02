using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace Khairi.DialogueSystem.Editor
{
    /// <summary>
    /// A dialogue node that connects to only one other dialogue node.
    /// </summary>
    [Serializable]
    public abstract class LinearDialogueNode : DialogueNode, ICheckErrors
    {
        public virtual void CheckErrors(GraphLogger logger)
        {
            var connectedOutputPorts = new List<IPort>();
            GetOutputPortByName(FlowPortName).GetConnectedPorts(connectedOutputPorts);

            if (connectedOutputPorts.Count > 1)
            {
                logger.LogWarning($"This node can only connect to one dialogue flow node.", this);
            }
        }

        public override void LinkRuntimeNode(DialogueRuntimeNode runtimeNode, IReadOnlyDictionary<IDialogueNode, DialogueRuntimeNode> runtimeNodes)
        {
            if (runtimeNode is not LinearDialogueRuntimeNode linearRuntimeNode)
                throw new InvalidOperationException($"Expected runtime node of type does not match editor node type.");

            // Link next node
            var nextNode = GetOutputPortByName(FlowPortName).firstConnectedPort?.GetNode();
            if (nextNode is not IDialogueNode nextFlowNode)
                return;

            if (runtimeNodes.TryGetValue(nextFlowNode, out var nextRuntimeNode))
                linearRuntimeNode.NextNode = nextRuntimeNode;
        }
    }
}
