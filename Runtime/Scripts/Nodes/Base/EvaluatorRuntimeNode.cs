using System;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    /// <summary>
    /// Runtime counterpart for editor evaluator nodes.
    /// </summary>
    /// <remarks>
    /// Implementations perform the actual evaluation at runtime and return results for a named output port.
    /// Editor-side nodes (derive from `EvaluatorNode`) create instances of these via
    /// <see cref="Khairi.DialogueSystem.Editor.IEvaluatorNode.CreateRuntimeEvaluator"/>.
    /// </remarks>
    public interface IEvaluatorRuntimeNode
    {
        /// <summary>
        /// Evaluates the node and returns a result for the specified output port.
        /// </summary>
        /// <param name="outputName">The name of the output port to evaluate. Use <see cref="EvaluatorRuntimeNode.DefaultOutputPortName"/> by default.</param>
        /// <param name="ctx">The <see cref="DialogueBehaviour"/> providing runtime context (variables, transforms, etc.).</param>
        /// <returns>The evaluated value for the requested output, or <c>null</c> if not available.</returns>
        object Evaluate(string outputName, DialogueBehaviour ctx);
    }

    [Serializable]
    public abstract class EvaluatorRuntimeNode : IEvaluatorRuntimeNode
    {
        public const string DefaultOutputPortName = "Output";
        public abstract object Evaluate(string outputName, DialogueBehaviour ctx);

        protected virtual void ThrowIfOutputNameMismatch(string outputName, string expectedOutputName, DialogueBehaviour ctx)
        {
            if (outputName != expectedOutputName)
            {
                Debug.LogError($"Invalid output name '{outputName}' for {GetType().Name}. Expected '{expectedOutputName}'.", ctx);
                throw new ArgumentException($"Invalid output name '{outputName}' for {GetType().Name}. Expected '{expectedOutputName}'.");
            }
        }
    }

    // TODO: Add optional caching to the evaluators?
    #region Evaluators

    [Serializable]
    public class GetVariableEvaluator : EvaluatorRuntimeNode
    {
        public enum DataType { None, String, Int, Float, Bool, Vector3, Color, UnityObject, SystemObject }
        public const string OutputName = DefaultOutputPortName;

        public DataType VarType;

        [SerializeReference]
        public InputPort<string> Key;

        public GetVariableEvaluator(DataType type, InputPort<string> keyPort)
        {
            VarType = type;
            Key = keyPort;
        }

        public override object Evaluate(string outputName, DialogueBehaviour ctx)
        {
            ThrowIfOutputNameMismatch(outputName, OutputName, ctx);

            var key = Key.GetValue(ctx);
            switch (VarType)
            {
                case DataType.String: return ctx.GetVariable<string>(key);
                case DataType.Int: return ctx.GetVariable<int>(key);
                case DataType.Float: return ctx.GetVariable<float>(key);
                case DataType.Bool: return ctx.GetVariable<bool>(key);
                case DataType.Vector3: return ctx.GetVariable<Vector3>(key);
                case DataType.Color: return ctx.GetVariable<Color>(key);
                case DataType.UnityObject: return ctx.GetVariable<UnityEngine.Object>(key);
                case DataType.SystemObject: return ctx.GetVariable<object>(key);
                case DataType.None:
                    Debug.LogWarning($"Data type is set to {VarType} for {GetType().Name} with key '{key}'.", ctx);
                    return null;
                default:
                    throw new InvalidOperationException($"Unsupported data type '{VarType}' in {GetType().Name}.");
            }
        }
    }

    [Serializable]
    public class GetChildGameObjectEvaluator : EvaluatorRuntimeNode
    {
        public const string OutputName = DefaultOutputPortName;

        [SerializeReference]
        public InputPort<string> ChildName;

        public GetChildGameObjectEvaluator(InputPort<string> childNamePort)
        {
            ChildName = childNamePort;
        }

        public override object Evaluate(string outputName, DialogueBehaviour ctx)
        {
            ThrowIfOutputNameMismatch(outputName, OutputName, ctx);

            var transform = ctx.transform;
            var childName = ChildName.GetValue(ctx);
            var childTransform = transform.Find(childName);
            if (childTransform == null)
            {
                Debug.LogWarning($"Child named '{childName}' not found under '{transform.name}'.", ctx);
                return null;
            }

            return childTransform.gameObject;
        }
    }

    [Serializable]
    public class FormatStringEvaluator : EvaluatorRuntimeNode
    {
        public const string OutputName = DefaultOutputPortName;

        [SerializeReference] public InputPort<string> Input;
        [SerializeReference] public InputPort<object>[] Arguments;

        public FormatStringEvaluator(InputPort<string> inputPort, InputPort<object>[] argumentPorts)
        {
            Input = inputPort;
            Arguments = argumentPorts;
        }

        public override object Evaluate(string outputName, DialogueBehaviour ctx)
        {
            ThrowIfOutputNameMismatch(outputName, OutputName, ctx);

            var input = Input.GetValue(ctx);
            try
            {
                var argumentValues = new object[Arguments.Length];
                for (int i = 0; i < Arguments.Length; i++)
                {
                    argumentValues[i] = Arguments[i].GetValue(ctx);
                    // Debug.Log($"Argument {i}: {argumentValues[i]} | {Arguments[i].LiteralValue}", ctx);
                }

                return string.Format(input, argumentValues);
            }
            catch (FormatException ex)
            {
                Debug.LogError($"String format error in {GetType().Name}: {ex.Message}. Input: '{input}'", ctx);
                return input; // Return the unformatted string as a fallback
            }
        }
    }

    #endregion
}
