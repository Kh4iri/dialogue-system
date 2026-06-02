using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class SetVariableNode : LinearDialogueNode
    {
        public const string ValueInputName = "Value";

        protected override void OnDefineOptions(IOptionDefinitionContext ctx)
        {
            ctx.AddOption<GetVariableEvaluator.DataType>(GetVariableNode.DataTypeOptionName).Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);
            GetNodeOptionByName(GetVariableNode.DataTypeOptionName).TryGetValue(out GetVariableEvaluator.DataType dataType);

            ctx.AddInputPort<string>(GetVariableNode.KeyInputName).Build();

            switch (dataType)
            {
                case GetVariableEvaluator.DataType.String: ctx.AddInputPort<string>(ValueInputName).Build(); break;
                case GetVariableEvaluator.DataType.Int: ctx.AddInputPort<int>(ValueInputName).Build(); break;
                case GetVariableEvaluator.DataType.Float: ctx.AddInputPort<float>(ValueInputName).Build(); break;
                case GetVariableEvaluator.DataType.Bool: ctx.AddInputPort<bool>(ValueInputName).Build(); break;
                case GetVariableEvaluator.DataType.Vector3: ctx.AddInputPort<Vector3>(ValueInputName).Build(); break;
                case GetVariableEvaluator.DataType.Color: ctx.AddInputPort<Color>(ValueInputName).Build(); break;
                case GetVariableEvaluator.DataType.UnityObject: ctx.AddInputPort<UnityEngine.Object>(ValueInputName).Build(); break;
                case GetVariableEvaluator.DataType.SystemObject: ctx.AddInputPort<object>(ValueInputName).Build(); break;
            }
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            var keyPort = GetInputPortByName(GetVariableNode.KeyInputName).ToInputPort<string>();
            var valuePort = GetInputPortByName(ValueInputName).ToInputPort<object>();
            return new SetVariableRuntimeNode(keyPort, valuePort);
        }

        public override void CheckErrors(GraphLogger logger)
        {
            base.CheckErrors(logger);

            // Check data type
            GetNodeOptionByName(GetVariableNode.DataTypeOptionName).TryGetValue(out GetVariableEvaluator.DataType dataType);
            if (dataType == GetVariableEvaluator.DataType.None)
                logger.LogError($"Data type is not set.", this);
            
            // Check key string
            var key = GetInputPortByName(GetVariableNode.KeyInputName).ResolvePortValue<string>();
            if (string.IsNullOrEmpty(key))
                logger.LogWarning($"Variable name is empty.", this);
        }
    }
}
