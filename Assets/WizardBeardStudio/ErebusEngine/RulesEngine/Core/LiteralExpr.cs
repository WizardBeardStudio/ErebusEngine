namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class LiteralExpr : Expr
    {
        public RuleValue Value { get; }
        public LiteralExpr(RuleValue v) { Value = v; }
        public override RuleValue Eval(EvalContext ctx) => Value;
    }
}