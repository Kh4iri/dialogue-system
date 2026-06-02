using System;
using UnityEngine;
using Unity.Serialization.Json;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public abstract class InputPort<TResult>
    {
        public abstract TResult GetValue(DialogueBehaviour ctx);
    }

    [Serializable]
    public class ConstantInputPort<TResult> : InputPort<TResult>
    {
        public TResult Value;

        public ConstantInputPort(TResult value)
        {
            Value = value;
        }

        public override TResult GetValue(DialogueBehaviour ctx)
            => Value;
    }

    [Serializable]
    public class ConstantInputPort<TExpected, TActual> : InputPort<TExpected>
    {
        public TActual Value;

        public ConstantInputPort(TActual value)
        {
            Value = value;
        }

        public override TExpected GetValue(DialogueBehaviour ctx)
            => (TExpected)(object)Value;
    }

    [Serializable]
    public class VariableInputPort<TResult> : InputPort<TResult>
    {
        public string VariableName;

        public VariableInputPort(string variableName)
        {
            VariableName = variableName;
        }

        public override TResult GetValue(DialogueBehaviour ctx)
            => ctx.GetVariable<TResult>(VariableName);
    }

    [Serializable]
    public class JsonObjectInputPort : InputPort<object>
    {
        public string ValueJsonData;

        public JsonObjectInputPort(object value)
        {
            ValueJsonData = JsonSerialization.ToJson(value, new() { Minified = true });
        }

        public override object GetValue(DialogueBehaviour ctx)
            => JsonSerialization.FromJson<object>(ValueJsonData);
        
        public T GetValue<T>(DialogueBehaviour ctx)
            => JsonSerialization.FromJson<T>(ValueJsonData);
    }

    [Serializable]
    public class EvaluatorInputPort<TResult> : InputPort<TResult>
    {
        [SerializeReference]
        public IEvaluatorRuntimeNode Evaluator;
        public string ConnectedEvaluatorOutputName;

        public EvaluatorInputPort(IEvaluatorRuntimeNode evaluator, string connectedEvaluatorOutputName)
        {
            Evaluator = evaluator;
            ConnectedEvaluatorOutputName = connectedEvaluatorOutputName;
        }

        public override TResult GetValue(DialogueBehaviour ctx)
        {
            var result = Evaluator.Evaluate(ConnectedEvaluatorOutputName, ctx);

            if (result is TResult typedResult)
            {
                return typedResult;
            }
            else
            {
                Debug.LogError($"Evaluator '{Evaluator.GetType().Name}' output port '{ConnectedEvaluatorOutputName}' is returning {result?.GetType().Name ?? "null"}, but expected {typeof(TResult).Name}.", ctx);
                return default;
            }
        }
    }

    // [Serializable]
    // public class VariableInputPort<TResult> : InputPort<TResult>
    // {
    //     public string VariableName;

    //     public VariableInputPort(string variableName)
    //     {
    //         VariableName = variableName;
    //     }

    //     public override TResult GetValue(DialogueBehaviour ctx)
    //     {
    //     }
    // }


    

    // [Serializable]
    // public struct InputPort<TResult>
    // {
    //     [Serializable]
    //     public struct SerializedObject
    //     {
    //         public string TypeName;
    //         public string JsonValue;
    //     }

    //     // Problem: If value is literal and TResult is not serializable, the value will be lost.
    //     public bool IsLiteral;
    //     public TResult LiteralValue;

    //     [SerializeReference]
    //     public IEvaluatorRuntimeNode Evaluator;
    //     public string ConnectedEvaluatorOutputName;

    //     public InputPort(TResult literalValue)
    //     {
    //         IsLiteral = true;
    //         LiteralValue = literalValue;
    //         Evaluator = null;
    //         ConnectedEvaluatorOutputName = null;
    //     }

    //     public InputPort(IEvaluatorRuntimeNode evaluator, string connectedEvaluatorOutputName)
    //     {
    //         IsLiteral = false;
    //         LiteralValue = default;
    //         Evaluator = evaluator;
    //         ConnectedEvaluatorOutputName = connectedEvaluatorOutputName;
    //     }

    //     /// <summary>
    //     /// A helper method that gets the value of the InputPort, either the literal value or the evaluated result.
    //     /// </summary>
    //     public readonly TResult GetValue(DialogueBehaviour ctx)
    //     {
    //         if (IsLiteral)
    //         {
    //             // Debug.Log($"Returning literal value: {LiteralValue} | TResult: {typeof(TResult)} | Type of Value: {LiteralValue?.GetType().Name ?? "null"}", ctx);
    //             return LiteralValue;
    //         }
    //         else if (Evaluator != null)
    //         {
    //             object result = Evaluator.Evaluate(ConnectedEvaluatorOutputName, ctx);

    //             if (typeof(TResult) == typeof(object))
    //             {
    //                 return (TResult)result;
    //             }

    //             if (result is TResult typedResult)
    //             {
    //                 return typedResult;
    //             }
    //             else
    //             {
    //                 Debug.LogError($"Evaluator '{Evaluator.GetType().Name}' output port '{ConnectedEvaluatorOutputName}' is returning {result?.GetType().Name ?? "null"}, but expected {typeof(TResult).Name}.", ctx);
    //                 return default;
    //             }
    //         }

    //         Debug.LogError("InputPort is not properly configured.", ctx);
    //         throw new InvalidOperationException("InputPort is not properly configured.");
    //     }
    // }
}
