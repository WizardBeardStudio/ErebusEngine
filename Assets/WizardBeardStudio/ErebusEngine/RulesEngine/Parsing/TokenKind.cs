namespace WizardBeardStudio.ErebusEngine.RulesEngine.Parsing
{
    public enum TokenKind : byte
    {
        Eof = 0,

        Identifier,
        Number,
        String,

        LParen, RParen,
        LBrace, RBrace,
        Comma, Colon,

        Dot,

        Plus, Minus, Star, Slash, Percent,

        EqEq, BangEq,
        Lt, Lte,
        Gt, Gte,

        KeywordRule,
        KeywordPriority,
        KeywordWhen,
        KeywordThen,
        KeywordOutcome,
        KeywordWith,
        KeywordStop,

        KeywordAnd,
        KeywordOr,
        KeywordNot,

        KeywordTrue,
        KeywordFalse,
        KeywordNull
    }
}