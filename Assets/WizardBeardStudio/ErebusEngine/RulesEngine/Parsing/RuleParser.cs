using System;
using System.Collections.Generic;
using System.Linq;
using WizardBeardStudio.ErebusEngine.RulesEngine.Core;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Parsing
{
    public sealed class RuleParser
    {
        private readonly PrattParser _p;

        public RuleParser(string src)
        {
            var tz = new Tokenizer(src);
            _p = new PrattParser(tz);
        }

        public CompiledRuleSet ParseRuleSet()
        {
            var rules = new List<Rule>(16);

            while (_p.Peek().Kind != TokenKind.Eof)
            {
                rules.Add(ParseRule());
            }

            // Sort by priority desc, stable by original order
            var sorted = rules
                .Select((r, idx) => (r, idx))
                .OrderByDescending(x => x.r.Priority)
                .ThenBy(x => x.idx)
                .Select(x => x.r)
                .ToList();

            return new CompiledRuleSet(sorted);
        }

        private Rule ParseRule()
        {
            _p.Consume(TokenKind.KeywordRule);

            var nameTok = _p.Expect(TokenKind.String);
            string name = nameTok.Text;

            int priority = 0;
            if (_p.Peek().Kind == TokenKind.KeywordPriority)
            {
                _p.Consume(TokenKind.KeywordPriority);
                var num = _p.Expect(TokenKind.Number);
                if (!int.TryParse(num.Text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out priority))
                    throw new ParseException($"Invalid priority '{num.Text}'.", num.Position);
            }

            _p.Consume(TokenKind.KeywordWhen);
            var cond = _p.ParseExpression();

            _p.Consume(TokenKind.KeywordThen);
            _p.Consume(TokenKind.KeywordOutcome);
            var outcomeTok = _p.Expect(TokenKind.String);
            string outcome = outcomeTok.Text;

            IReadOnlyDictionary<string, RuleValue> data = new Dictionary<string, RuleValue>();

            if (_p.Peek().Kind == TokenKind.KeywordWith)
            {
                _p.Consume(TokenKind.KeywordWith);
                data = ParsePayloadObject();
            }

            bool stop = false;
            if (_p.Peek().Kind == TokenKind.KeywordStop)
            {
                _p.Consume(TokenKind.KeywordStop);
                stop = true;
            }

            // Optional: allow extra whitespace/comments between rules; parsing already skips trivia.
            return new Rule(name, priority, cond, outcome, data, stop);
        }

        private IReadOnlyDictionary<string, RuleValue> ParsePayloadObject()
        {
            var dict = new Dictionary<string, RuleValue>(StringComparer.Ordinal);

            _p.Consume(TokenKind.LBrace);

            if (_p.Peek().Kind != TokenKind.RBrace)
            {
                while (true)
                {
                    // key: identifier or string
                    string key;
                    var k = _p.Peek();

                    if (k.Kind == TokenKind.Identifier)
                        key = _p.Expect(TokenKind.Identifier).Text;
                    else if (k.Kind == TokenKind.String)
                        key = _p.Expect(TokenKind.String).Text;
                    else
                        throw new ParseException("Expected payload key (identifier or string).", k.Position);

                    _p.Consume(TokenKind.Colon);

                    // v1: payload values are literals only (no expressions)
                    dict[key] = ParseLiteralValue();

                    if (_p.Peek().Kind == TokenKind.Comma)
                    {
                        _p.Consume(TokenKind.Comma);
                        continue;
                    }

                    break;
                }
            }

            _p.Consume(TokenKind.RBrace);
            return dict;
        }

        private RuleValue ParseLiteralValue()
        {
            var t = _p.Peek();
            switch (t.Kind)
            {
                case TokenKind.Number:
                    var n = _p.Expect(TokenKind.Number);
                    if (n.Text.IndexOf('.') >= 0)
                    {
                        var d = double.Parse(n.Text, System.Globalization.CultureInfo.InvariantCulture);
                        return RuleValue.FromDouble(d);
                    }
                    else
                    {
                        var i = long.Parse(n.Text, System.Globalization.CultureInfo.InvariantCulture);
                        return RuleValue.FromInt(i);
                    }

                case TokenKind.String:
                    return RuleValue.FromString(_p.Expect(TokenKind.String).Text);

                case TokenKind.KeywordTrue:
                    _p.Consume(TokenKind.KeywordTrue);
                    return RuleValue.FromBool(true);

                case TokenKind.KeywordFalse:
                    _p.Consume(TokenKind.KeywordFalse);
                    return RuleValue.FromBool(false);

                case TokenKind.KeywordNull:
                    _p.Consume(TokenKind.KeywordNull);
                    return RuleValue.Null();

                default:
                    throw new ParseException("Payload values must be literals in v1.", t.Position);
            }
        }
    }
}