namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class SymbolExpr : Expr
    {
        public string Symbol { get; }
        public SymbolExpr(string symbol) { Symbol = symbol; }

        public override RuleValue Eval(EvalContext ctx)
        {
            if (ctx.Context.TryGetValue(Symbol, out var v)) return v;

            ctx.MissingSymbols.Add(Symbol);
            return RuleValue.Null();
        }
    }
}