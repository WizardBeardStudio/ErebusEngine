#nullable enable
using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed record Decision(
        string Outcome,
        IReadOnlyDictionary<string, RuleValue> Data,
        bool Matched,
        IReadOnlyList<RulesEngine.Debug.RuleTrace>? Trace = null
    )
    {
        public string Outcome { get; } = Outcome;
        public IReadOnlyDictionary<string, RuleValue> Data { get; } = Data;
        public bool Matched { get; } = Matched;
        public IReadOnlyList<RulesEngine.Debug.RuleTrace> Trace { get; } = Trace;
    }
}