namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public sealed class BinaryExpr : Expr
    {
        public BinaryOp Op { get; }
        public Expr Left { get; }
        public Expr Right { get; }

        public BinaryExpr(BinaryOp op, Expr left, Expr right)
        {
            Op = op;
            Left = left;
            Right = right;
        }

        public override RuleValue Eval(EvalContext ctx)
        {
            // Short-circuit
            if (Op == BinaryOp.And)
            {
                var lv = Left.Eval(ctx);
                if (!lv.IsTruthy()) return RuleValue.FromBool(false);
                var rv = Right.Eval(ctx);
                return RuleValue.FromBool(rv.IsTruthy());
            }
            if (Op == BinaryOp.Or)
            {
                var lv = Left.Eval(ctx);
                if (lv.IsTruthy()) return RuleValue.FromBool(true);
                var rv = Right.Eval(ctx);
                return RuleValue.FromBool(rv.IsTruthy());
            }

            var a = Left.Eval(ctx);
            var b = Right.Eval(ctx);

            switch (Op)
            {
                case BinaryOp.Eq: return RuleValue.FromBool(CompareEq(a, b));
                case BinaryOp.Neq: return RuleValue.FromBool(!CompareEq(a, b));
                case BinaryOp.Lt: return RuleValue.FromBool(CompareRel(a, b, Rel.Lt));
                case BinaryOp.Lte: return RuleValue.FromBool(CompareRel(a, b, Rel.Lte));
                case BinaryOp.Gt: return RuleValue.FromBool(CompareRel(a, b, Rel.Gt));
                case BinaryOp.Gte: return RuleValue.FromBool(CompareRel(a, b, Rel.Gte));

                case BinaryOp.Add: return Add(a, b);
                case BinaryOp.Sub: return Num(a, b, (x, y) => x - y);
                case BinaryOp.Mul: return Num(a, b, (x, y) => x * y);
                case BinaryOp.Div: return Num(a, b, (x, y) => y == 0 ? double.NaN : (x / y));
                case BinaryOp.Mod: return Mod(a, b);

                default:
                    return RuleValue.Null();
            }
        }

        private enum Rel : byte { Lt, Lte, Gt, Gte }

        private static bool CompareEq(RuleValue a, RuleValue b)
        {
            // Numeric cross-compare
            if (a.TryGetNumber(out var an) && b.TryGetNumber(out var bn))
                return an.Equals(bn);

            // Null equality
            if (a.Kind == RuleValueKind.Null && b.Kind == RuleValueKind.Null) return true;
            if (a.Kind == RuleValueKind.Null || b.Kind == RuleValueKind.Null) return false;

            // String strict
            if (a.Kind == RuleValueKind.String && b.Kind == RuleValueKind.String)
                return a.AsStringOrThrow() == b.AsStringOrThrow();

            // Bool strict
            if (a.Kind == RuleValueKind.Bool && b.Kind == RuleValueKind.Bool)
                return a.AsBoolOrThrow() == b.AsBoolOrThrow();

            // Fallback: kind must match
            return a == b;
        }

        private static bool CompareRel(RuleValue a, RuleValue b, Rel rel)
        {
            if (a.TryGetNumber(out var an) && b.TryGetNumber(out var bn))
            {
                return rel switch
                {
                    Rel.Lt => an < bn,
                    Rel.Lte => an <= bn,
                    Rel.Gt => an > bn,
                    Rel.Gte => an >= bn,
                    _ => false
                };
            }

            if (a.Kind == RuleValueKind.String && b.Kind == RuleValueKind.String)
            {
                var s1 = a.AsStringOrThrow();
                var s2 = b.AsStringOrThrow();
                int cmp = string.CompareOrdinal(s1, s2);
                return rel switch
                {
                    Rel.Lt => cmp < 0,
                    Rel.Lte => cmp <= 0,
                    Rel.Gt => cmp > 0,
                    Rel.Gte => cmp >= 0,
                    _ => false
                };
            }

            return false;
        }

        private static RuleValue Add(RuleValue a, RuleValue b)
        {
            if (a.Kind == RuleValueKind.String || b.Kind == RuleValueKind.String)
            {
                var sa = a.Kind == RuleValueKind.String ? a.AsStringOrThrow() : a.ToString();
                var sb = b.Kind == RuleValueKind.String ? b.AsStringOrThrow() : b.ToString();
                return RuleValue.FromString(sa + sb);
            }

            return Num(a, b, (x, y) => x + y);
        }

        private static RuleValue Num(RuleValue a, RuleValue b, System.Func<double, double, double> op)
        {
            if (!a.TryGetNumber(out var an) || !b.TryGetNumber(out var bn))
                return RuleValue.Null();

            var r = op(an, bn);
            return RuleValue.FromDouble(r);
        }

        private static RuleValue Mod(RuleValue a, RuleValue b)
        {
            if (!a.TryGetNumber(out var an) || !b.TryGetNumber(out var bn))
                return RuleValue.Null();

            if (bn == 0) return RuleValue.Null();

            // Use integer mod when both are integral
            bool aInt = a.Kind == RuleValueKind.Int || a.Kind == RuleValueKind.Bool;
            bool bInt = b.Kind == RuleValueKind.Int || b.Kind == RuleValueKind.Bool;

            if (aInt && bInt)
                return RuleValue.FromInt((long)an % (long)bn);

            return RuleValue.FromDouble(an % bn);
        }
    }
}