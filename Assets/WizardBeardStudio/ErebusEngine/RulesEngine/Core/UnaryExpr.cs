namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class UnaryExpr : Expr
    {
        public UnaryOp Op { get; }
        public Expr Inner { get; }

        public UnaryExpr(UnaryOp op, Expr inner)
        {
            Op = op;
            Inner = inner;
        }

        public override RuleValue Eval(EvalContext ctx)
        {
            var v = Inner.Eval(ctx);

            switch (Op)
            {
                case UnaryOp.Not:
                    return RuleValue.FromBool(!v.IsTruthy());

                case UnaryOp.Negate:
                    if (v.TryGetNumber(out var n)) return RuleValue.FromDouble(-n);
                    return RuleValue.Null();

                default:
                    return RuleValue.Null();
            }
        }
    }
}