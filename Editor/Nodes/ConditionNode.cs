using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class ConditionNode : DialogueNode, ICheckErrors
    {
        public const string PredicateInputName = "Predicate";
        public const string TrueOutputName = "TrueFlow";
        public const string FalseOutputName = "FalseFlow";

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputFlowPort(ctx);

            ctx.AddInputPort<bool>(PredicateInputName)
                .WithDisplayName("Predicate")
                .Build();

            ctx.AddOutputPort(TrueOutputName)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .WithDisplayName("True")
                .Build();

            ctx.AddOutputPort(FalseOutputName)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .WithDisplayName("False")
                .Build();
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            var predicatePort = GetInputPortByName(PredicateInputName).ToInputPort<bool>();
            var runtimeNode = new ConditionRuntimeNode(predicatePort);
            return runtimeNode;
        }

        public override void LinkRuntimeNode(DialogueRuntimeNode runtimeNode, IReadOnlyDictionary<IDialogueNode, DialogueRuntimeNode> runtimeNodes)
        {
            if (runtimeNode is not ConditionRuntimeNode conditionRuntimeNode)
                throw new InvalidOperationException($"Expected runtime node of type does not match editor node type.");

            if (GetOutputPortByName(TrueOutputName).firstConnectedPort?.GetNode() is IDialogueNode trueNextNode &&
                runtimeNodes.TryGetValue(trueNextNode, out var trueRuntimeNode))
            {
                conditionRuntimeNode.TrueNextNode = trueRuntimeNode;
            }

            if (GetOutputPortByName(FalseOutputName).firstConnectedPort?.GetNode() is IDialogueNode falseNextNode &&
                runtimeNodes.TryGetValue(falseNextNode, out var falseRuntimeNode))
            {
                conditionRuntimeNode.FalseNextNode = falseRuntimeNode;
            }
        }

        public virtual void CheckErrors(GraphLogger logger)
        {
            var connectedOutputPorts = new List<IPort>();
            foreach (var portName in new[] { TrueOutputName, FalseOutputName })
            {
                GetOutputPortByName(portName).GetConnectedPorts(connectedOutputPorts);
                if (connectedOutputPorts.Count > 1)
                {
                    logger.LogWarning($"This node can only connect to one dialogue flow node.", this);
                }
            }
        }
    }
}
