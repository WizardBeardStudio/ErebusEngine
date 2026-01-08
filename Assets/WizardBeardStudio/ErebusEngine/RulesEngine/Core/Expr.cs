namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public abstract class Expr
    {
        public abstract RuleValue Eval(EvalContext ctx);
    }
}