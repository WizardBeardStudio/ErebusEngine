using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class EvalContext
    {
        public IRuleContext Context { get; }
        public IFunctionLibrary Functions { get; }
        public List<string> MissingSymbols { get; } = new List<string>(8);

        // Reused buffer for function arguments to avoid per-call allocations.
        public List<RuleValue> TempArgs { get; } = new List<RuleValue>(8);

        public EvalContext(IRuleContext ctx, IFunctionLibrary functions)
        {
            Context = ctx;
            Functions = functions;
        }

        public void Reset()
        {
            MissingSymbols.Clear();
            TempArgs.Clear();
        }
    }
}