using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AssetImporters;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine.Assertions;

namespace Khairi.DialogueSystem.Editor
{
    /// <summary>
    /// Custom asset importer that converts editor-time <see cref="DialogueGraph"/> assets into runtime-optimized <see cref="DialogueGraphAsset"/> assets.
    /// This allows dialogue graphs to be designed visually in the editor and then converted to efficient runtime representations.
    /// Version is incremented when import logic changes to trigger reimport of existing assets.
    /// </summary>
    [ScriptedImporter(version: 2, ext: DialogueGraph.AssetExtension)]
    public class DialogueGraphImporter : ScriptedImporter
    {
        /// <summary>
        /// Called by Unity's asset pipeline to convert a DialogueGraph into a RuntimeDialogueGraph.
        /// </summary>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            // Load the visual graph that was created in the editor
            DialogueGraph graph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);

            // The `graph` may be null if the `LoadGraphForImporter` method
            // fails to load the asset from the specified `ctx.assetPath`.
            // This can occur under the following circumstances:
            // - The asset path is incorrect, or the asset does not exist at the specified location.
            // - The asset located at the specified path is not of type `VisualNovelDirectorGraph`.
            // - The asset file itself is problematic. For example, it is corrupted, or stored in an unsupported format.
            if (graph == null)
            {
                Debug.LogError($"Failed to load Dialogue graph asset: {ctx.assetPath}");
                return;
            }
            
            // Build the runtime asset by walking the graph and adding the relevant nodes.
            DialogueGraphAsset runtimeAsset = BuildRuntimeAsset(graph, ctx.assetPath);
            
            if (runtimeAsset == null)
            {
                Debug.LogError($"Failed to build Dialogue Graph Asset from editor graph: {ctx.assetPath}");
                return;
            }
            
            // Add the runtime object to the graph asset and set it to be the main asset.
            // This allows the same asset to be used in inspectors wherever a runtime asset is expected.
            ctx.AddObjectToAsset("RuntimeAsset", runtimeAsset);
            ctx.SetMainObject(runtimeAsset);
        }

        private DialogueGraphAsset BuildRuntimeAsset(DialogueGraph graph, string ctxAssetPath)
        {
            var runtimeAsset = ScriptableObject.CreateInstance<DialogueGraphAsset>();
            var runtimeNodes = new Dictionary<IDialogueNode, DialogueRuntimeNode>();

            if (!graph.TryGetStartNode(out var startNode))
            {
                Debug.LogWarning($"Dialogue Graph \"{graph.name}\" ({ctxAssetPath}) is missing a Start Node.", runtimeAsset);
                return runtimeAsset;
            }

            // Convert all dialogue nodes from their editor representation to runtime representation
            foreach (var node in graph.GetNodes())
            {
                if (node is IDialogueNode dialogueNode)
                {
                    var runtimeNode = dialogueNode.CreateRuntimeNode();
                    Assert.IsNotNull(runtimeNode, $"Runtime node for dialogue node {dialogueNode.GetType().Name} is null.");
                    runtimeNodes[dialogueNode] = runtimeNode;
                }
            }

            // Follow the connection from the start node to find the first dialogue node
            // This node becomes the entry point for runtime execution
            var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
            if (entryPort != null && entryPort.GetNode() is IDialogueNode entryDialogueNode)
            {
                runtimeAsset.EntryNode = runtimeNodes[entryDialogueNode];
            }

            // Link the runtime nodes together
            foreach (var kvp in runtimeNodes)
            {
                var node = kvp.Key;
                var runtimeNode = kvp.Value;
                node.LinkRuntimeNode(runtimeNode, runtimeNodes);
            }

            // Get all variable nodes and convert them to runtime variable nodes, then add them to the runtime asset
            var graphVariables = graph.GetVariables();
            var varCount = graph.variableCount;
            if (graph.variableCount > 0)
            {
                var runtimeVariables = new List<IRuntimeVariable>(varCount);

                // Debug.Log($"---------- Found {varCount} variable(s) in '{graph.name}' ----------");
                foreach (var variable in graphVariables)
                {
                    if (variable.TryGetDefaultValue(out Object unityObject))
                    {
                        // Debug.Log($"Variable Name: {variable.name} | Type: {variable.dataType} | Default Value: {unityObject}");
                        runtimeVariables.Add(new UnityObjectRuntimeVariable(variable.name, unityObject, variable.dataType));
                    }
                    else if (variable.TryGetDefaultValue(out object objectValue))
                    {
                        // Debug.Log($"Variable Name: {variable.name} | Type: {variable.dataType} | Default Value: {objectValue} | Json: {Unity.Serialization.Json.JsonSerialization.ToJson(objectValue, new() { Minified = true })}");
                        string defaultValueJson = Unity.Serialization.Json.JsonSerialization.ToJson(objectValue, new() { Minified = true });
                        runtimeVariables.Add(new SystemObjectRuntimeVariable(variable.name, defaultValueJson, variable.dataType));
                    }
                    else
                    {
                        Debug.LogError($"Failed to get default value for variable '{variable.name}' of type '{variable.dataType}' in '{graph.name}'");
                    }
                }

                runtimeAsset.Variables.AddRange(runtimeVariables);
            }

            // Add all runtime nodes to the asset
            runtimeAsset.Nodes.AddRange(runtimeNodes.Values);
            return runtimeAsset;
        }
    }
}
