using System;
using UnityEngine;
using UnityEditor;
using Unity.GraphToolkit.Editor;
using System.Linq;
using UnityEngine.Assertions;

namespace Khairi.DialogueSystem.Editor
{
    public interface ICheckErrors
    {
        public void CheckErrors(GraphLogger logger);
    }

    [Serializable]
    [Graph(AssetExtension)]
    public class DialogueGraph : Graph
    {
        public const string AssetExtension = "dialoguegraph";

        [MenuItem("Assets/Create/Dialogue System/Dialogue Graph")]
        private static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DialogueGraph>("New Dialogue Graph");
        }

        /// <summary>
        /// Called when the graph changes.
        /// </summary>
        /// <param name="logger">The <see cref="GraphLogger"/> object to which errors and warnings are added.</param>
        public override void OnGraphChanged(GraphLogger logger)
        {
            CheckGraphErrors(logger);
        }

        public bool TryGetStartNode(out StartNode startNode)
        {
            startNode = GetNodes().OfType<StartNode>().FirstOrDefault();
            return startNode != null;
        }

        /// <summary>
        /// Checks the graph for errors and warnings and adds them to the result object.
        /// </summary>
        /// <param name="logger">Object implementing <see cref="GraphLogger"/> interface and containing collected errors and warnings</param>
        /// <remarks>Errors and warnings are reported by adding them to the GraphLogger object,
        /// which is the default reporting mechanism for a Graph Toolkit tool.
        /// </remarks>
        private void CheckGraphErrors(GraphLogger logger)
        {
            // Check for variable name conflicts by ensuring that all variable names are unique within the graph.
            var variables = GetVariables().ToList();
            var duplicateVariableNames = variables
                .GroupBy(v => v.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var name in duplicateVariableNames)
                logger.LogError($"Name conflict: Multiple variables share the name '{name}'.", this);

            // Check for the presence of a StartNode and log an error if one is not found, as well as if multiple are found.
            var startNodes = GetNodes().OfType<StartNode>().ToList();
            switch (startNodes.Count)
            {
                case 0:
                {
                    // Log an error if no entry is found in the graph.
                    logger.LogError("Missing StartNode. Dialogue Graphs must have one StartNode to indicate the entry point of the dialogue.", this);
                    break;
                }

                case 1:
                {
                    // If no connections are made from the StartNode, log an error.
                    var startNode = startNodes[0];
                    if (startNode.GetOutputPortByName(DialogueNode.FlowPortName).firstConnectedPort == null)
                        logger.LogError("The StartNode is not connected to any node.", startNode);

                    break;
                }

                case > 1:
                {
                    foreach (var node in startNodes.Skip(1))
                    {
                        // This will result in a warning message being logged in the console and a warning marker being displayed on the node
                        logger.LogWarning($"There must be only one {nameof(StartNode)} per graph. " +
                            "Only the first created one will be used.", node);
                    }

                    break;
                }
            }

            // Check for individual node errors by calling CheckErrors on all nodes that implement the ICheckErrors interface
            var nodesToCheck = GetNodes().OfType<ICheckErrors>().ToArray();
            foreach (var node in nodesToCheck)
                node.CheckErrors(logger);
        }
    }

    public static partial class DialogueGraphExtensions
    {
        public static InputPort<T> ToInputPort<T>(this IPort port)
        {
            Assert.IsTrue(port.direction == PortDirection.Input, $"Couldn't convert port '{port.name}' to InputPort. The port's direction must be Input.");

            // Get the source port providing input to "port" (null if no connection exists)
            var sourcePort = port.firstConnectedPort;

            switch (sourcePort?.GetNode())
            {
                case IConstantNode node:
                {
                    // Unity natively serializes UnityEngine.Object references but not fields of type object,
                    // so we check for a UnityEngine.Object value first before checking for a constant value of type T.
                    if (port.TryGetValue(out UnityEngine.Object unityObject))
                    {
                        if (unityObject == null)
                            return new ConstantInputPort<T>(default);
                            
                        return new ConstantInputPort<T, UnityEngine.Object>(unityObject);
                    }
                    else if (node.TryGetValue(out T constantValue))
                    {
                        // If the type expected is EXACTLY object, use json serialization because Unity doesn't serialize fields of type object.
                        if (typeof(T) == typeof(object))
                            return new JsonObjectInputPort(constantValue) as InputPort<T>;

                        // Debug.Log($"Creating InputPort with CONSTANT literal value: {constantValue} | Typeof(T): {typeof(T)} | Typeof(Value): {constantValue?.GetType()}");
                        return new ConstantInputPort<T>(constantValue);
                    }

                    return new ConstantInputPort<T>(default);
                }

                case IVariableNode node:
                {
                    // If connected to a variable node, return an InputPort that will look up the variable's value at runtime using the variable's name.
                    return new VariableInputPort<T>(node.variable.name);
                }

                case IEvaluatorNode node:
                {
                    // If connected to an evaluator node, create a runtime evaluator and return it wrapped in an InputPort that
                    // can be evaluated at runtime with the evaluator (source) port name to get the value of the port.
                    return new EvaluatorInputPort<T>(node.CreateRuntimeEvaluator(), sourcePort.name);
                }

                // If not connected to any node, attempt to get the port's embedded value.
                case null:
                {
                    // Unity natively serializes UnityEngine.Object references but not fields of type object,
                    // so we check for a UnityEngine.Object value first before checking for a constant value of type T.
                    if (port.TryGetValue(out UnityEngine.Object unityObject))
                    {
                        if (unityObject == null)
                            return new ConstantInputPort<T>(default);
                            
                        return new ConstantInputPort<T, UnityEngine.Object>(unityObject);
                    }
                    else if (port.TryGetValue(out T embeddedValue))
                    {
                        // If the type expected is EXACTLY object, use json serialization because Unity doesn't serialize fields of type object.
                        if (typeof(T) == typeof(object))
                            return new JsonObjectInputPort(embeddedValue) as InputPort<T>;
                        
                        return new ConstantInputPort<T>(embeddedValue);
                    }

                    return new ConstantInputPort<T>(default);
                }
            }

            return new ConstantInputPort<T>(default);
        }

        /// <summary>
        /// Resolves the value of a port without evaluating any connected evaluators.
        /// </summary>
        /// <typeparam name="T">The type of value to resolve from the port.</typeparam>
        /// <param name="port">The port whose value should be resolved.</param>
        /// <returns>
        /// The resolved value of type <typeparamref name="T"/>, determined by the following priority:
        /// <list type="number">
        /// <item><description>If the port is connected to a constant node, returns the constant value.</description></item>
        /// <item><description>If the port is connected to a variable node, returns the variable's default value.</description></item>
        /// <item><description>If the port has no connections, returns the port's embedded value.</description></item>
        /// <item><description>If no value is available through any of the above, returns the default value of type <typeparamref name="T"/>.</description></item>
        /// </list>
        /// </returns>
        public static T ResolvePortValue<T>(this IPort port)
        {
            // Get the source port providing input to "port" (null if no connection exists)
            var sourcePort = port.firstConnectedPort;

            switch (sourcePort?.GetNode())
            {
                case IConstantNode node:
                {
                    node.TryGetValue(out T constantValue);
                    return constantValue;
                }

                case IVariableNode node:
                {
                    node.variable.TryGetDefaultValue(out T variableValue);
                    return variableValue;
                }

                case null:
                {
                    // If no connection exists, try to get port's embedded value (returns type default if unavailable)
                    port.TryGetValue(out T embeddedValue);
                    return embeddedValue;
                }
            }

            return default;
        }
    }
}
