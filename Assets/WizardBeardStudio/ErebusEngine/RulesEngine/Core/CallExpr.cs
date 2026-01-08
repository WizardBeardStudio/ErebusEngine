using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class CallExpr : Expr
    {
        public string Name { get; }
        public IReadOnlyList<Expr> Args { get; }

        public CallExpr(string name, IReadOnlyList<Expr> args)
        {
            Name = name;
            Args = args;
        }

        public override RuleValue Eval(EvalContext ctx)
        {
            if (Args.Count == 0)
            {
                if (ctx.Functions.TryInvoke(Name, System.Array.Empty<RuleValue>(), out var r0)) return r0;
                return RuleValue.Null();
            }

            var tmp = ctx.TempArgs;
            tmp.Clear();
            for (int i = 0; i < Args.Count; i++)
                tmp.Add(Args[i].Eval(ctx));

            if (ctx.Functions.TryInvoke(Name, tmp, out var r)) return r;
            return RuleValue.Null();
        }
    }
}