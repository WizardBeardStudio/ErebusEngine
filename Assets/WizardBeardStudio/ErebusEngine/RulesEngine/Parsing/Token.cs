namespace WizardBeardStudio.ErebusEngine.RulesEngine.Parsing
{
    public readonly struct Token
    {
        public TokenKind Kind { get; }
        public string Text { get; }
        public int Position { get; }

        public Token(TokenKind kind, string text, int pos)
        {
            Kind = kind;
            Text = text;
            Position = pos;
        }

        public override string ToString() => $"{Kind} '{Text}' @{Position}";
    }
}