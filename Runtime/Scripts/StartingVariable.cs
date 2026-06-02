using System;
using UnityEngine;

namespace Khairi.DialogueSystem.StartingVariables
{
    /// <summary>
    /// Wrapper struct to allow serialization of <see cref="IStartingVariable"/>
    /// implementations inside the DialogueBehaviour's starting variables dictionary.
    /// </summary>
    [Serializable]
    public struct StartingVariableWrapper
    {
        [SerializeReference, SubclassSelector]
        public IStartingVariable Variable;
    }

    // --------------------------------------------------------------------- //

    public interface IStartingVariable
    {
        public T GetValue<T>();
        public Type GetValueType();
    }

    public abstract class StartingVariable<T> : IStartingVariable
    {
        [SerializeField]
        public T Value;

        public TOut GetValue<TOut>() => (TOut)(object)Value;
        public Type GetValueType() => typeof(T);
    }

    [Serializable] public class StringVariable : StartingVariable<string> {}
    [Serializable] public class IntVariable : StartingVariable<int> {}
    [Serializable] public class FloatVariable : StartingVariable<float> {}
    [Serializable] public class BoolVariable : StartingVariable<bool> {}
    [Serializable] public class Vector3Variable : StartingVariable<Vector3> {}
    [Serializable] public class ColorVariable : StartingVariable<Color> {}
    [Serializable] public class UnityObjectVariable : StartingVariable<UnityEngine.Object> {}
}
