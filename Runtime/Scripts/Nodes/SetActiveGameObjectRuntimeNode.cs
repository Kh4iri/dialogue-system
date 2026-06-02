using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class SetActiveGameObjectRuntimeNode : LinearDialogueRuntimeNode
    {
        [SerializeReference] public InputPort<GameObject> GameObject;
        [SerializeReference] public InputPort<bool> Active;

        public SetActiveGameObjectRuntimeNode(InputPort<GameObject> gameObject, InputPort<bool> active) : base()
        {
            GameObject = gameObject;
            Active = active;
        }

        public override Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            var gameObject = GameObject.GetValue(ctx);
            if (gameObject != null)
            {
                gameObject.SetActive(Active.GetValue(ctx));
            }
            else
            {
                Debug.LogWarning($"{nameof(SetActiveGameObjectRuntimeNode)}: GameObject is null.", ctx);
            }

            return Task.CompletedTask;
        }
    }
}
