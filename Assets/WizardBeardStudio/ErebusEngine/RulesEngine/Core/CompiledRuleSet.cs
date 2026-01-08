using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class CompiledRuleSet
    {
        public IReadOnlyList<Rule> Rules { get; }

        public CompiledRuleSet(IReadOnlyList<Rule> rules)
        {
            Rules = rules;
        }
    }
}