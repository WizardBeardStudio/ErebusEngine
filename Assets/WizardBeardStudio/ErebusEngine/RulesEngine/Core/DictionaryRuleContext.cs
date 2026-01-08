using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class DictionaryRuleContext : IRuleContext
    {
        private readonly Dictionary<string, RuleValue> _facts;

        public DictionaryRuleContext(Dictionary<string, RuleValue> facts) => _facts = facts;

        public bool TryGetValue(string symbol, out RuleValue value) => _facts.TryGetValue(symbol, out value);
    }
}