using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace Khairi.DialogueSystem.Editor
{
    /// <summary>
    /// Represents an editor node that can produce a runtime evaluator.
    /// </summary>
    /// <remarks>
    /// Implementations (editor-side nodes) create an <see cref="IEvaluatorRuntimeNode"/> which is used
    /// by the runtime to evaluate values, perform lookups or return computed results when the graph is executed.
    /// Editor node classes typically derive from <see cref="EvaluatorNode"/> and override
    /// <see cref="CreateRuntimeEvaluator"/> to return the corresponding runtime evaluator instance.
    /// </remarks>
    /// <returns>
    /// A new instance of <see cref="IEvaluatorRuntimeNode"/> that encapsulates the runtime behavior of this node.
    /// </returns>
    public interface IEvaluatorNode
    {
        IEvaluatorRuntimeNode CreateRuntimeEvaluator();
    }

    [Serializable]
    public abstract class EvaluatorNode : Node, IEvaluatorNode
    {
        public const string DefaultOutputPortName = "Output";
        public abstract IEvaluatorRuntimeNode CreateRuntimeEvaluator();

        /// <summary>
        /// Adds an output port to the node.
        /// </summary>
        protected void AddOutputPort<T>(IPortDefinitionContext ctx, string outputName = DefaultOutputPortName, string displayName = null)
        {
            ctx.AddOutputPort<T>(outputName)
                .WithDisplayName(displayName ?? string.Empty)
                .Build();
        }
    }

    #region Evaluators

    [Serializable]
    public class GetVariableNode : EvaluatorNode, ICheckErrors
    {
        public const string DataTypeOptionName = "DataType";
        public const string KeyInputName = "Key";

        protected override void OnDefineOptions(IOptionDefinitionContext ctx)
        {
            ctx.AddOption<GetVariableEvaluator.DataType>(DataTypeOptionName)
                .WithDefaultValue(GetVariableEvaluator.DataType.String)
                .WithDisplayName("Data Type")
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort<string>(KeyInputName).Build();

            // Output port type depends on the selected data type option.
            GetNodeOptionByName(DataTypeOptionName).TryGetValue(out GetVariableEvaluator.DataType dataType);
            switch (dataType)
            {
                case GetVariableEvaluator.DataType.String: AddOutputPort<string>(ctx); break;
                case GetVariableEvaluator.DataType.Int: AddOutputPort<int>(ctx); break;
                case GetVariableEvaluator.DataType.Float: AddOutputPort<float>(ctx); break;
                case GetVariableEvaluator.DataType.Bool: AddOutputPort<bool>(ctx); break;
                case GetVariableEvaluator.DataType.Vector3: AddOutputPort<Vector3>(ctx); break;
                case GetVariableEvaluator.DataType.Color: AddOutputPort<Color>(ctx); break;
                case GetVariableEvaluator.DataType.UnityObject: AddOutputPort<UnityEngine.Object>(ctx); break;
                case GetVariableEvaluator.DataType.SystemObject: AddOutputPort<object>(ctx); break;
            }
        }

        public override IEvaluatorRuntimeNode CreateRuntimeEvaluator()
        {
            GetNodeOptionByName(DataTypeOptionName).TryGetValue(out GetVariableEvaluator.DataType dataType);

            var keyPort = GetInputPortByName(KeyInputName).ToInputPort<string>();
            return new GetVariableEvaluator(dataType, keyPort);
        }

        void ICheckErrors.CheckErrors(GraphLogger logger)
        {
            // Check data type
            GetNodeOptionByName(DataTypeOptionName).TryGetValue(out GetVariableEvaluator.DataType dataType);
            if (dataType == GetVariableEvaluator.DataType.None)
                logger.LogError($"Data type is not set.", this);

            // Check key string
            var key = GetInputPortByName(KeyInputName).ResolvePortValue<string>();
            if (string.IsNullOrEmpty(key))
                logger.LogWarning($"Variable name is empty.", this);
        }
    }

    [Serializable]
    public class GetChildGameObjectNode : EvaluatorNode
    {
        public const string ChildInputName = "ChildName";

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddOutputPort<GameObject>(ctx);
            
            ctx.AddInputPort<string>(ChildInputName)
                .WithDisplayName("Child Name")
                .Build();
        }

        public override IEvaluatorRuntimeNode CreateRuntimeEvaluator()
        {
            var childNamePort = GetInputPortByName(ChildInputName).ToInputPort<string>();
            return new GetChildGameObjectEvaluator(childNamePort);
        }
    }

    [Serializable]
    public class FormatStringNode : EvaluatorNode, ICheckErrors
    {
        public const string ArgumentCountOptionName = "ArgumentCount";
        public const string ArgumentPortNamePrefix = "Arg";
        public const string InputName = "Input";

        protected override void OnDefineOptions(IOptionDefinitionContext ctx)
        {
            ctx.AddOption<int>(ArgumentCountOptionName)
                .WithDefaultValue(1)
                .WithDisplayName("Argument Count")
                .Delayed()
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddOutputPort<string>(ctx);
            ctx.AddInputPort<string>(InputName).Build();

            GetNodeOptionByName(ArgumentCountOptionName).TryGetValue(out int argumentCount);
            for (int i = 0; i < argumentCount; i++)
            {
                ctx.AddInputPort<object>($"{ArgumentPortNamePrefix}{i}").WithDisplayName($"Arg {i}").Build();
            }
        }

        public override IEvaluatorRuntimeNode CreateRuntimeEvaluator()
        {
            // Input
            var inputPort = GetInputPortByName(InputName).ToInputPort<string>();

            // Arguments
            GetNodeOptionByName(ArgumentCountOptionName).TryGetValue(out int argumentCount);
            var argumentPorts = new InputPort<object>[argumentCount];
            for (int i = 0; i < argumentCount; i++)
            {
                argumentPorts[i] = GetInputPortByName($"{ArgumentPortNamePrefix}{i}").ToInputPort<object>();
            }

            return new FormatStringEvaluator(inputPort, argumentPorts);
        }

        public void CheckErrors(GraphLogger logger)
        {
            GetNodeOptionByName(ArgumentCountOptionName).TryGetValue(out int argumentCount);
            if (argumentCount < 0)
            {
                logger.LogError($"Argument count cannot be negative.", this);
            }
        }
    }

    #endregion
}
