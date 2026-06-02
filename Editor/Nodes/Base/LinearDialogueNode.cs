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

        /// <summary>
        /// Gets the next connected node in the dialogue flow.
        /// </summary>
        public virtual INode GetNextNode()
            => GetOutputPortByName(FlowPortName).firstConnectedPort?.GetNode();
    }
}
