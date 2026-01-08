#nullable enable
using System.Collections.Generic;
using WizardBeardStudio.ErebusEngine.RulesEngine.Debug;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class RulesEngineRuntime : IRulesEngine
    {
        private readonly CompiledRuleSet _ruleSet;
        private readonly IFunctionLibrary _functions;
        private readonly bool _enableTrace;

        public RulesEngineRuntime(CompiledRuleSet ruleSet, IFunctionLibrary functions, bool enableTrace)
        {
            _ruleSet = ruleSet;
            _functions = functions ?? NoFunctions.Instance;
            _enableTrace = enableTrace;
        }

        public Decision Evaluate(IRuleContext context)
        {
            var eval = new EvalContext(context, _functions);

            List<RuleTrace>? traces = _enableTrace ? new List<RuleTrace>(_ruleSet.Rules.Count) : null;

            for (int i = 0; i < _ruleSet.Rules.Count; i++)
            {
                eval.Reset();

                var r = _ruleSet.Rules[i];
                var cond = r.Condition.Eval(eval).IsTruthy();

                if (_enableTrace)
                {
                    traces!.Add(new RuleTrace(
                        r.Name,
                        r.Priority,
                        cond,
                        eval.MissingSymbols.Count == 0 ? System.Array.Empty<string>() : new List<string>(eval.MissingSymbols)
                    ));
                }

                if (!cond) continue;

                // First-match semantics after priority sort.
                return new Decision(r.Outcome, r.Data, true, traces);
            }

            return new Decision("NoMatch", new Dictionary<string, RuleValue>(), false, traces);
        }
    }
}