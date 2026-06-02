using System;
using UnityEngine;

namespace Khairi.DialogueSystem
{
    public interface IRuntimeVariable
    {
        public string Name { get; }
        public string DataType { get; }
        public T GetDefaultValue<T>();
    }

    [Serializable]
    public struct UnityObjectRuntimeVariable : IRuntimeVariable
    {
        [SerializeField] private string _name;
        [SerializeField] private UnityEngine.Object _defaultValue;
        [SerializeField] private string _dataType;

        public readonly string Name => _name;
        public readonly UnityEngine.Object DefaultValue => _defaultValue;
        public readonly string DataType => _dataType;

        public UnityObjectRuntimeVariable(string name, UnityEngine.Object value, Type dataType)
        {
            _name = name;
            _defaultValue = value;
            _dataType = dataType.AssemblyQualifiedName;
        }

        public readonly T GetDefaultValue<T>()
        {
            if (_defaultValue == null)
                return default;
            if (_defaultValue is T typedValue)
                return typedValue;
            else
                throw new InvalidCastException($"Failed to get default value for variable '{_name}'. Expected type: {typeof(T).Name}, actual type: {_defaultValue.GetType().Name}.");
        }
    }

    [Serializable]
    public struct SystemObjectRuntimeVariable : IRuntimeVariable
    {
        [SerializeField] private string _name;
        [SerializeField] private string _defaultValueJson;
        [SerializeField] private string _dataType;

        public readonly string Name => _name;
        public readonly string DefaultValueJson => _defaultValueJson;
        public readonly string DataType => _dataType;

        public SystemObjectRuntimeVariable(string name, string defaultValueJson, Type dataType)
        {
            _name = name;
            _defaultValueJson = defaultValueJson;
            _dataType = dataType.AssemblyQualifiedName;
        }

        public readonly T GetDefaultValue<T>()
        {
            try
            {
                return Unity.Serialization.Json.JsonSerialization.FromJson<T>(_defaultValueJson);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize default value for variable '{_name}' from JSON. Expected type: {typeof(T).Name}, JSON: {_defaultValueJson}", ex);
            }
        }
    }
}
