using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public interface IRuleContext
    {
        bool TryGetValue(string symbol, out RuleValue value);
    }

    public interface IFunctionLibrary
    {
        bool TryInvoke(string name, IReadOnlyList<RuleValue> args, out RuleValue result);
    }

    public interface IRulesEngine
    {
        Decision Evaluate(IRuleContext context);
    }

    public sealed class NoFunctions : IFunctionLibrary
    {
        public static readonly NoFunctions Instance = new NoFunctions();
        private NoFunctions() { }

        public bool TryInvoke(string name, IReadOnlyList<RuleValue> args, out RuleValue result)
        {
            result = RuleValue.Null();
            return false;
        }
    }
}