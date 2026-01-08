using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class Rule
    {
        public string Name { get; }
        public int Priority { get; }
        public Expr Condition { get; }
        public string Outcome { get; }
        public IReadOnlyDictionary<string, RuleValue> Data { get; }
        public bool Stop { get; }

        public Rule(string name, int priority, Expr condition, string outcome, IReadOnlyDictionary<string, RuleValue> data, bool stop)
        {
            Name = name;
            Priority = priority;
            Condition = condition;
            Outcome = outcome;
            Data = data;
            Stop = stop;
        }
    }
}