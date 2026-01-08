using WizardBeardStudio.ErebusEngine.RulesEngine.Core;
using WizardBeardStudio.ErebusEngine.RulesEngine.Parsing;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Builder
{
    public sealed class RulesEngineBuilder
    {
        private string _source = string.Empty;
        private IFunctionLibrary _functions = NoFunctions.Instance;
        private bool _enableTrace = false;

        private RulesEngineBuilder() { }

        public static RulesEngineBuilder FromText(string dslText)
        {
            return new RulesEngineBuilder { _source = dslText ?? string.Empty };
        }

        public RulesEngineBuilder WithFunctions(IFunctionLibrary functions)
        {
            _functions = functions ?? NoFunctions.Instance;
            return this;
        }

        public RulesEngineBuilder EnableTrace(bool enable = true)
        {
            _enableTrace = enable;
            return this;
        }

        public IRulesEngine Build()
        {
            var parser = new RuleParser(_source);
            var ruleSet = parser.ParseRuleSet();

            return new RulesEngineRuntime(ruleSet, _functions, _enableTrace);
        }
    }
}