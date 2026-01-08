using System;
using System.Collections.Generic;
using System.Globalization;
using WizardBeardStudio.ErebusEngine.RulesEngine.Core;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Parsing
{
    public sealed class PrattParser
    {
        private readonly Tokenizer _tz;
        private Token _cur;

        public PrattParser(Tokenizer tz)
        {
            _tz = tz;
            _cur = _tz.Next();
        }

        public Expr ParseExpression()
        {
            return ParseExpr(0);
        }

        private Expr ParseExpr(int minBp)
        {
            Expr left = ParsePrefix();

            while (true)
            {
                var op = _cur.Kind;
                if (!TryGetInfixBindingPower(op, out int lbp, out int rbp))
                    break;

                if (lbp < minBp)
                    break;

                Consume(op);

                // Short-circuit ops handled in eval; parsing is normal binary
                var right = ParseExpr(rbp);
                left = new BinaryExpr(MapBinary(op), left, right);
            }

            return left;
        }

        private Expr ParsePrefix()
        {
            var t = _cur;

            switch (t.Kind)
            {
                case TokenKind.KeywordNot:
                    Consume(TokenKind.KeywordNot);
                    return new UnaryExpr(UnaryOp.Not, ParseExpr(70));

                case TokenKind.Minus:
                    Consume(TokenKind.Minus);
                    return new UnaryExpr(UnaryOp.Negate, ParseExpr(70));

                case TokenKind.LParen:
                    Consume(TokenKind.LParen);
                    var inner = ParseExpr(0);
                    Consume(TokenKind.RParen);
                    return inner;

                case TokenKind.Number:
                    Consume(TokenKind.Number);
                    if (t.Text.IndexOf('.') >= 0)
                    {
                        double d = double.Parse(t.Text, CultureInfo.InvariantCulture);
                        return new LiteralExpr(RuleValue.FromDouble(d));
                    }
                    else
                    {
                        long i = long.Parse(t.Text, CultureInfo.InvariantCulture);
                        return new LiteralExpr(RuleValue.FromInt(i));
                    }

                case TokenKind.String:
                    Consume(TokenKind.String);
                    return new LiteralExpr(RuleValue.FromString(t.Text));

                case TokenKind.KeywordTrue:
                    Consume(TokenKind.KeywordTrue);
                    return new LiteralExpr(RuleValue.FromBool(true));

                case TokenKind.KeywordFalse:
                    Consume(TokenKind.KeywordFalse);
                    return new LiteralExpr(RuleValue.FromBool(false));

                case TokenKind.KeywordNull:
                    Consume(TokenKind.KeywordNull);
                    return new LiteralExpr(RuleValue.Null());

                case TokenKind.Identifier:
                    return ParseIdentifierOrCallOrSymbol();

                default:
                    throw new ParseException($"Unexpected token {t.Kind} in expression.", t.Position);
            }
        }

        private Expr ParseIdentifierOrCallOrSymbol()
        {
            // identifier ('.' identifier)*  OR  identifier '(' args ')'
            var parts = new List<string>(4);

            var id = Expect(TokenKind.Identifier);
            parts.Add(id.Text);

            while (_cur.Kind == TokenKind.Dot)
            {
                Consume(TokenKind.Dot);
                var p = Expect(TokenKind.Identifier);
                parts.Add(p.Text);
            }

            // Call: only if single-part name and next is '('
            if (parts.Count == 1 && _cur.Kind == TokenKind.LParen)
            {
                Consume(TokenKind.LParen);

                var args = new List<Expr>(4);
                if (_cur.Kind != TokenKind.RParen)
                {
                    while (true)
                    {
                        args.Add(ParseExpr(0));
                        if (_cur.Kind == TokenKind.Comma)
                        {
                            Consume(TokenKind.Comma);
                            continue;
                        }
                        break;
                    }
                }

                Consume(TokenKind.RParen);
                return new CallExpr(parts[0], args);
            }

            // Symbol path
            return new SymbolExpr(string.Join(".", parts));
        }

        private static BinaryOp MapBinary(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.KeywordOr => BinaryOp.Or,
                TokenKind.KeywordAnd => BinaryOp.And,

                TokenKind.EqEq => BinaryOp.Eq,
                TokenKind.BangEq => BinaryOp.Neq,
                TokenKind.Lt => BinaryOp.Lt,
                TokenKind.Lte => BinaryOp.Lte,
                TokenKind.Gt => BinaryOp.Gt,
                TokenKind.Gte => BinaryOp.Gte,

                TokenKind.Plus => BinaryOp.Add,
                TokenKind.Minus => BinaryOp.Sub,
                TokenKind.Star => BinaryOp.Mul,
                TokenKind.Slash => BinaryOp.Div,
                TokenKind.Percent => BinaryOp.Mod,

                _ => throw new InvalidOperationException($"No BinaryOp mapping for {kind}.")
            };
        }

        // Binding powers (precedence)
        // Higher value => tighter bind
        private static bool TryGetInfixBindingPower(TokenKind op, out int lbp, out int rbp)
        {
            switch (op)
            {
                case TokenKind.KeywordOr: lbp = 10; rbp = 11; return true;
                case TokenKind.KeywordAnd: lbp = 20; rbp = 21; return true;

                case TokenKind.EqEq:
                case TokenKind.BangEq:
                case TokenKind.Lt:
                case TokenKind.Lte:
                case TokenKind.Gt:
                case TokenKind.Gte:
                    lbp = 30; rbp = 31; return true;

                case TokenKind.Plus:
                case TokenKind.Minus:
                    lbp = 40; rbp = 41; return true;

                case TokenKind.Star:
                case TokenKind.Slash:
                case TokenKind.Percent:
                    lbp = 50; rbp = 51; return true;

                default:
                    lbp = rbp = 0;
                    return false;
            }
        }

        // Token helpers
        public Token Peek() => _cur;

        public Token Expect(TokenKind kind)
        {
            if (_cur.Kind != kind)
                throw new ParseException($"Expected {kind}, got {_cur.Kind}.", _cur.Position);

            var t = _cur;
            _cur = _tz.Next();
            return t;
        }

        public void Consume(TokenKind kind)
        {
            _ = Expect(kind);
        }
    }
}