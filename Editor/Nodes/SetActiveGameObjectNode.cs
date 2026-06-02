using System;
using UnityEngine;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class SetActiveGameObjectNode : LinearDialogueNode
    {
        public const string GameObjectInputName = "GameObject";
        public const string ActiveInputName = "Active";

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);

            ctx.AddInputPort<GameObject>(GameObjectInputName);
            ctx.AddInputPort<bool>(ActiveInputName);
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            var gameObjectPort = GetInputPortByName(GameObjectInputName).ToInputPort<GameObject>();
            var activePort = GetInputPortByName(ActiveInputName).ToInputPort<bool>();

            var runtimeNode = new SetActiveGameObjectRuntimeNode(gameObjectPort, activePort);
            return runtimeNode;
        }
    }
}
