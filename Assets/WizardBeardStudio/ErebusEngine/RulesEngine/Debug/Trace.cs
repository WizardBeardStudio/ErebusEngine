using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Debug
{
    public sealed record RuleTrace(
        string RuleName,
        int Priority,
        bool ConditionResult,
        IReadOnlyList<string> MissingSymbols
    )
    {
        public string RuleName { get; } = RuleName;
        public int Priority { get; } = Priority;
        public bool ConditionResult { get; } = ConditionResult;
        public IReadOnlyList<string> MissingSymbols { get; } = MissingSymbols;
    }
}