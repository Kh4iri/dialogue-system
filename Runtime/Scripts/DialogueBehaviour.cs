
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AYellowpaper.SerializedCollections;
using EditorAttributes;
using Khairi.DialogueSystem.StartingVariables;

namespace Khairi.DialogueSystem
{
    public class DialogueBehaviour : MonoBehaviour
    {
        [SerializeField, TypeFilter(typeof(IDialogueViewPreset)), Required] private ScriptableObject _dialogueView;
        [SerializeField, Required] private DialogueGraphAsset _dialogueGraph;
        [SerializeField] private AudioSource _defaultVoiceSource;

        [Space]
        [Title("Variables")]
        [HelpBox(nameof(VariablesHelpBoxMessage), MessageMode.None, StringInputMode.Dynamic, true)]
        [SerializedDictionary("Variable Name", "Value")]
        [SerializeField] private SerializedDictionary<string, StartingVariableWrapper> _startingVariables = new();

        [Space]
        [Title("Events")]
        public UnityEvent DialogueStarted;
        public UnityEvent DialogueEnded;
        public UnityEvent<string, object[]> EventTriggered;

        public static int DialoguesRunning { get; private set; }
        public static bool IsAnyDialogueRunning => DialoguesRunning > 0;

        public DialogueGraphAsset DialogueGraph => _dialogueGraph;
        public IDialogueViewPreset DialogueViewPreset {
            get {
                if (_dialogueViewPreset == null)
                {
                    _dialogueViewPreset = _dialogueView as IDialogueViewPreset;
                    if (_dialogueViewPreset == null)
                        Debug.LogError($"The assigned dialogue view '{_dialogueView.name}' does not implement {nameof(IDialogueViewPreset)}.", this);
                }

                return _dialogueViewPreset;
            }
        }

        public AudioSource DefaultVoiceSource => _defaultVoiceSource;
        private string VariablesHelpBoxMessage { get {
            if (!_dialogueGraph)
                return "Please assign a dialogue graph.";
            
            if (_dialogueGraph.Variables.Count == 0)
                return "No variables found in the assigned dialogue graph.";
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Found <color=yellow>{_dialogueGraph.Variables.Count} variable(s)</color> in the graph. Default values:");
            for (int i = 0; i < _dialogueGraph.Variables.Count; i++)
            {
                var variable = _dialogueGraph.Variables[i];
                string text;

                // If unity object, show only the name of the referenced object, otherwise show the JSON string of the default value
                if (variable is UnityObjectRuntimeVariable unityVar)
                {
                    var defaultValue = variable.GetDefaultValue<UnityEngine.Object>();
                    string typeData = Type.GetType(unityVar.DataType)?.Name ?? "Unknown";
                    string value = defaultValue != null ? defaultValue.name : $"null";
                    text = $"• <b>{variable.Name}</b> <alpha=#99>({typeData})</alpha> = {value}";
                }
                else
                {
                    var defaultValue = variable.GetDefaultValue<object>();
                    string typeData = Type.GetType(variable.DataType)?.Name ?? "Unknown";
                    string value = $"{defaultValue ?? "null"}";
                    text = $"• <b>{variable.Name}</b> <alpha=#99>({typeData})</alpha> = {value}";
                }

                sb.AppendLine(text);
            }

            sb.Append("\nYou can override the default values above with the dictionary below. If the variable's name below doesn't match any of the variable names above, a new variable will be created at runtime.");
            return sb.ToString();
        }}

        private bool _isDialogueRunning;
        private CancellationTokenSource _cts;
        private IDialogueViewPreset _dialogueViewPreset;

        /// <summary>
        /// Holds the current runtime values of variables during the dialogue.
        /// This is initially empty and only populates when the dialogue starts and should be cleared when the dialogue ends.
        /// This is built based on the default values from the dialogue graph and starting variables set in this behaviour.
        /// </summary>
        private Dictionary<string, object> _runtimeVariables = new();

        private void Start()
        {
            Debug.Assert(_dialogueView as IDialogueViewPreset != null, $"Assigned dialogue view '{_dialogueView.name}' does not implement {nameof(IDialogueViewPreset)}.", this);
            if (_dialogueGraph)
                Debug.Assert(CheckStartingVariableTypes(), $"One or more starting variables in '{gameObject.name}' are invalid. Please fix the errors.", this);

            _dialogueViewPreset = _dialogueView as IDialogueViewPreset;
            _dialogueViewPreset.Validate();
        }

        public async Task StartDialogueAsync(CancellationToken ct = default)
        {
            if (_dialogueGraph == null)
            {
                Debug.LogError($"No dialogue graph assigned to {nameof(DialogueBehaviour)} on '{gameObject.name}'. Cannot start dialogue.", this);
                return;
            }

            if (_dialogueView == null)
            {
                Debug.LogError($"No dialogue view assigned to {nameof(DialogueBehaviour)} on '{gameObject.name}'. Cannot start dialogue.", this);
                return;
            }

            var currentNode = _dialogueGraph.EntryNode;
            if (currentNode == null)
            {
                Debug.LogError($"Dialogue graph '{_dialogueGraph.name}' has no valid entry node.", this);
                return;
            }

            if (!CheckStartingVariableTypes())
            {
                Debug.LogError($"One or more starting variables in '{gameObject.name}' are invalid. Please fix the errors.", this);
                return;
            }


            _cts?.Cancel();
            _cts?.Dispose();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);

            DialoguesRunning++;
            _isDialogueRunning = true;
            InitializeRuntimeVariables();
            Debug.Log($"Dialogue '{_dialogueGraph.name}' started. {_runtimeVariables.Count} variables initialized.", this);
            DialogueStarted?.Invoke();

            try
            {
                while (currentNode != null)
                {
                    switch (currentNode)
                    {
                        case LinearDialogueRuntimeNode linearNode:
                        {
                            await linearNode.ExecuteAsync(this, _cts.Token);
                            currentNode = linearNode.NextNode;
                            break;
                        }

                        case WriteChoiceDialogueRuntimeNode choiceNode:
                        {
                            await choiceNode.ExecuteAsync(this, _cts.Token);
                            var selectedChoice = choiceNode.SelectedChoice;
                            currentNode = selectedChoice.NextNode;
                            break;
                        }

                        case ConditionRuntimeNode conditionNode:
                        {
                            await conditionNode.ExecuteAsync(this, _cts.Token);
                            currentNode = conditionNode.LastEvaluatedResult ? conditionNode.TrueNextNode : conditionNode.FalseNextNode;
                            break;
                        }

                        default:
                        {
                            Debug.LogError($"[{nameof(DialogueBehaviour)}] Unsupported node type: {currentNode.GetType().Name}");
                            currentNode = null;
                            break;
                        }
                    }
                }

                await _dialogueViewPreset.HideDialogueAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"Dialogue '{_dialogueGraph.name}' was cancelled.", this);
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred during dialogue '{_dialogueGraph.name}'!", this);
                Debug.LogException(ex, this);
                throw;
            }
            finally
            {
                if (_dialogueViewPreset.IsVisible)
                {
                    _ = _dialogueViewPreset.HideDialogueAsync(destroyCancellationToken);
                }

                _isDialogueRunning = false;
                DialoguesRunning--;
                Debug.Log($"Dialogue '{_dialogueGraph.name}' has ended.", this);
                DialogueEnded?.Invoke();
                ClearRuntimeVariables();
            }
        }

        public void StartDialogue()
        {
            _ = StartDialogueAsync();
        }

        public void CancelDialogue()
        {
            if (!_isDialogueRunning)
                return;

            Debug.Log($"Cancelling dialogue '{_dialogueGraph.name}'...", this);
            _cts?.Cancel();
        }

        public void InvokeEvent(string eventName, params object[] parameters)
        {
            #if UNITY_EDITOR
            var parameterLog = parameters.Length > 0
                ? string.Join(" | ", parameters.Select(p => $"{p} <color=yellow>({p?.GetType().Name ?? "null"})</color>"))
                : "(No parameters)";
            Debug.Log($"Invoked dialogue event '{eventName}'. Parameters: {parameterLog}", this);
            #endif
            
            EventTriggered?.Invoke(eventName, parameters);
        }

        /// <summary>
        /// Gets the value of a variable by name.
        /// If dialogue is running, it will look in the runtime variables (which includes the starter variables set in this behaviour).
        /// Otherwise, it falls back to the default value from the dialogue graph and starting variables.
        /// </summary>
        public T GetVariable<T>(string variableName)
        {
            // If dialogue is running, use runtime variables.
            // Otherwise, use the default values from the dialogue graph & starting variables.
            if (_isDialogueRunning)
            {
                if (_runtimeVariables.TryGetValue(variableName, out var runtimeValue))
                {
                    if (runtimeValue is T typedValue)
                        return typedValue;

                    throw new InvalidCastException($"Runtime variable '{variableName}' cannot be cast to type {typeof(T).Name}. Actual type: {runtimeValue?.GetType().Name ?? "null"}.");
                }

                Debug.LogError($"Runtime variable '{variableName}' not found.", this);
                throw new KeyNotFoundException($"Runtime variable '{variableName}' not found.");
            }
            else
            {
                if (_startingVariables.TryGetValue(variableName, out var variableWrapper))
                {
                    if (variableWrapper.Variable != null)
                        return variableWrapper.Variable.GetValue<T>();

                    return default;
                }

                var graphVariable = _dialogueGraph.Variables.FirstOrDefault(v => v.Name == variableName);
                if (graphVariable != null)
                    return graphVariable.GetDefaultValue<T>();

                Debug.LogError($"Variable '{variableName}' not found.", this);
                throw new KeyNotFoundException($"Variable '{variableName}' not found.");
            }
        }

        public void SetRuntimeVariable<T>(string variableName, T value)
        {
            if (!_isDialogueRunning)
            {
                Debug.LogError($"Cannot set variable '{variableName}' because the dialogue is not running.", this);
                throw new InvalidOperationException($"Cannot set variable '{variableName}' because the dialogue is not running.");
            }

            _runtimeVariables[variableName] = value;
        }

        public bool TryGetRuntimeVariable(string variableName, out object value)
        {
            if (!_isDialogueRunning)
            {
                Debug.LogError($"Cannot get variable '{variableName}' because the dialogue is not running.", this);
                value = null;
                return false;
            }

            return _runtimeVariables.TryGetValue(variableName, out value);
        }

        /// <summary>
        /// Checks if the variables set in this Behaviour are valid type-wise against the variables defined in the dialogue graph.
        /// Logs errors for any invalid variables found. Returns true if all starter variables are valid, false otherwise.
        /// </summary>
        public bool CheckStartingVariableTypes()
        {
            bool isValid = true;
            foreach (var kvp in _startingVariables)
            {
                var variableName = kvp.Key;
                var graphVariable = _dialogueGraph.Variables.FirstOrDefault(v => v.Name == variableName);

                // If the variable doesn't exist in the dialogue graph, we should skip it
                // because this will make a new variable for later use.
                if (graphVariable == null)
                    continue;

                var variableWrapper = kvp.Value;
                var variable = variableWrapper.Variable;
                var expectedType = Type.GetType(graphVariable.DataType);

                // If starting variable override is null, but the expected type is value type,
                // this is invalid because value types cannot be null
                bool isNullable = !expectedType.IsValueType || Nullable.GetUnderlyingType(expectedType) != null;
                if (variable == null && !isNullable)
                {
                    Debug.LogError($"Starting variable '{variableName}' is null but the expected type '{expectedType.Name}' is a value type and cannot be null.", this);
                    isValid = false;
                    continue;
                }

                // Special case for string that can be null (variable.GetValueType() will throw if variable is null, but we already know this is valid if expectedType is string)
                // Now that we know the variable value is nullable or not, if the variable value is null, we can skip the type check because null is a valid value for this variable.
                if (variable == null)
                    continue;

                // The variable type must be assignable to the type of the variable defined in the dialogue graph
                var variableValueType = variable.GetValueType();
                if (!variableValueType.IsAssignableFrom(expectedType))
                {
                    Debug.LogError($"Type mismatch for starting variable '{variableName}'. Expected type: {expectedType.Name}, provided type: {variableValueType.Name}.", this);
                    isValid = false;
                    continue;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Builds the runtime variable dictionary based on the default values from the dialogue graph and any starting variables set in this behaviour.
        /// This is called when the dialogue starts to initialize the runtime variables that will be used during dialogue execution.
        /// </summary>
        public void InitializeRuntimeVariables()
        {
            _runtimeVariables.Clear();
            _runtimeVariables.TrimExcess();

            // First add all variables from the dialogue graph with their default values
            foreach (var variable in _dialogueGraph.Variables)
                _runtimeVariables[variable.Name] = variable.GetDefaultValue<object>();

            // Then update the runtime variables with any starting variables set in this behaviour.
            // This will add new variables that don't exist in the dialogue graph as well, which can be useful for storing temporary values during dialogue execution that aren't defined in the graph.
            foreach (var kvp in _startingVariables)
            {
                var varName = kvp.Key;
                var varWrapper = kvp.Value;
                if (varWrapper.Variable != null)
                {
                    _runtimeVariables[varName] = varWrapper.Variable.GetValue<object>();
                }
                else
                {
                    _runtimeVariables[varName] = null;
                }
            }
        }

        public void ClearRuntimeVariables()
        {
            if (_runtimeVariables != null)
            {
                _runtimeVariables.Clear();
                _runtimeVariables.TrimExcess();
            }
        }

        #region Voice
        public IDisposable BeginVoicePlayback(AudioSource voiceSource, AudioClip voiceClip)
        {
            if (voiceSource == null)
                voiceSource = _defaultVoiceSource;

            if (voiceSource == null || voiceClip == null)
                return EmptyDisposable.Instance;

            voiceSource.PlayOneShot(voiceClip);
            return new VoicePlaybackScope(voiceSource);
        }

        private sealed class VoicePlaybackScope : IDisposable
        {
            private AudioSource _voiceSource;

            public VoicePlaybackScope(AudioSource voiceSource)
            {
                _voiceSource = voiceSource;
            }

            public void Dispose()
            {
                if (_voiceSource == null)
                    return;

                _voiceSource.Stop();
                _voiceSource = null;
            }
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new();
            public void Dispose() {}
        }
        #endregion
    
        [Button("Log Current Variables", makeDirty: false)]
        private void EditorLogCurrentVariables()
        {
            if (!CheckStartingVariableTypes())
                return;

            var allVariableNames = _dialogueGraph.Variables.Select(v => v.Name).Union(_startingVariables.Keys).Distinct().ToList();
            if (allVariableNames.Count == 0)
            {
                Debug.Log("No variables found in the dialogue graph or starting variables.", this);
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Found {allVariableNames.Count} variable(s) for dialogue graph '{_dialogueGraph.name}':");
            foreach (var variableName in allVariableNames)
            {
                var value = GetVariable<object>(variableName);
                sb.AppendLine($"• <b>{variableName}</b> = {value ?? "null"}");
            }

            Debug.Log(sb.ToString(), this);
        }
    }
}
