using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace Khairi.DialogueSystem.Editor
{
    public interface IDialogueNode
    {
        /// <summary>
        /// Creates the runtime representation of this dialogue node.
        /// Note: This method is called before other runtime nodes are created and linked together.
        /// </summary>
        public DialogueRuntimeNode CreateRuntimeNode();
    }

    [Serializable]
    public abstract class DialogueNode : Node, IDialogueNode
    {
        public const string FlowPortName = "Flow";

        /// <summary>
        /// Adds an input flow port to the node.
        /// </summary>
        protected void AddInputFlowPort(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort(FlowPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }

        /// <summary>
        /// Adds an output flow port to the node.
        /// </summary>
        protected void AddOutputFlowPort(IPortDefinitionContext ctx)
        {
            ctx.AddOutputPort(FlowPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }

        /// <summary>
        /// Adds both input and output flow ports to the node.
        /// </summary>
        protected void AddInputOutputFlowPorts(IPortDefinitionContext ctx)
        {
            AddInputFlowPort(ctx);
            AddOutputFlowPort(ctx);
        }

        public abstract DialogueRuntimeNode CreateRuntimeNode();
    }
}
