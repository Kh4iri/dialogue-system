using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace Khairi.DialogueSystem.Editor
{
    /// <summary>
    /// Represents the Start Node in the Dialogue graph tool.
    /// </summary>
    /// <remarks>
    /// The start node serves as the entry point to the dialogue graph.
    /// </remarks>
    [Serializable]
    public class StartNode : Node, ICheckErrors
    {
        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            ctx.AddOutputPort(DialogueNode.FlowPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }

        public void CheckErrors(GraphLogger logger)
        {
            var connectedOutputPorts = new List<IPort>();
            GetOutputPortByName(DialogueNode.FlowPortName).GetConnectedPorts(connectedOutputPorts);

            if (connectedOutputPorts.Count > 1)
            {
                logger.LogWarning($"This node can only connect to one dialogue flow node.", this);
            }
        }
    }
}
