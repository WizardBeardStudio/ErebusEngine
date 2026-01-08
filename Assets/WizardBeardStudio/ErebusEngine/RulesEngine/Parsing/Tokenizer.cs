using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Parsing
{
    public sealed class Tokenizer
    {
        private readonly string _src;
        private int _i;

        private static readonly Dictionary<string, TokenKind> Keywords = new Dictionary<string, TokenKind>(StringComparer.Ordinal)
        {
            ["rule"] = TokenKind.KeywordRule,
            ["priority"] = TokenKind.KeywordPriority,
            ["when"] = TokenKind.KeywordWhen,
            ["then"] = TokenKind.KeywordThen,
            ["outcome"] = TokenKind.KeywordOutcome,
            ["with"] = TokenKind.KeywordWith,
            ["stop"] = TokenKind.KeywordStop,

            ["and"] = TokenKind.KeywordAnd,
            ["or"] = TokenKind.KeywordOr,
            ["not"] = TokenKind.KeywordNot,

            ["true"] = TokenKind.KeywordTrue,
            ["false"] = TokenKind.KeywordFalse,
            ["null"] = TokenKind.KeywordNull
        };

        public Tokenizer(string src)
        {
            _src = src ?? string.Empty;
            _i = 0;
        }

        public Token Next()
        {
            SkipTrivia();

            if (_i >= _src.Length)
                return new Token(TokenKind.Eof, string.Empty, _i);

            int start = _i;
            char c = _src[_i];

            // Single-char punctuation
            switch (c)
            {
                case '(': _i++; return new Token(TokenKind.LParen, "(", start);
                case ')': _i++; return new Token(TokenKind.RParen, ")", start);
                case '{': _i++; return new Token(TokenKind.LBrace, "{", start);
                case '}': _i++; return new Token(TokenKind.RBrace, "}", start);
                case ',': _i++; return new Token(TokenKind.Comma, ",", start);
                case ':': _i++; return new Token(TokenKind.Colon, ":", start);
                case '.': _i++; return new Token(TokenKind.Dot, ".", start);
                case '+': _i++; return new Token(TokenKind.Plus, "+", start);
                case '-': _i++; return new Token(TokenKind.Minus, "-", start);
                case '*': _i++; return new Token(TokenKind.Star, "*", start);
                case '/': _i++; return new Token(TokenKind.Slash, "/", start);
                case '%': _i++; return new Token(TokenKind.Percent, "%", start);
                case '"': return ReadString();
            }

            // Two-char operators
            if (c == '=')
            {
                if (Peek2("==")) { _i += 2; return new Token(TokenKind.EqEq, "==", start); }
                throw new ParseException("Unexpected '='. Use '=='.", start);
            }

            if (c == '!')
            {
                if (Peek2("!=")) { _i += 2; return new Token(TokenKind.BangEq, "!=", start); }
                throw new ParseException("Unexpected '!'. Use '!='.", start);
            }

            if (c == '<')
            {
                if (Peek2("<=")) { _i += 2; return new Token(TokenKind.Lte, "<=", start); }
                _i++;
                return new Token(TokenKind.Lt, "<", start);
            }

            if (c == '>')
            {
                if (Peek2(">=")) { _i += 2; return new Token(TokenKind.Gte, ">=", start); }
                _i++;
                return new Token(TokenKind.Gt, ">", start);
            }

            // Number
            if (char.IsDigit(c))
                return ReadNumber();

            // Identifier / keyword
            if (IsIdentStart(c))
                return ReadIdentifierOrKeyword();

            throw new ParseException($"Unexpected character '{c}'.", start);
        }

        private void SkipTrivia()
        {
            while (_i < _src.Length)
            {
                char c = _src[_i];

                // whitespace
                if (char.IsWhiteSpace(c))
                {
                    _i++;
                    continue;
                }

                // line comment: //
                if (c == '/' && _i + 1 < _src.Length && _src[_i + 1] == '/')
                {
                    _i += 2;
                    while (_i < _src.Length && _src[_i] != '\n')
                        _i++;
                    continue;
                }

                // block comment: /* ... */
                if (c == '/' && _i + 1 < _src.Length && _src[_i + 1] == '*')
                {
                    _i += 2;
                    while (_i + 1 < _src.Length && !(_src[_i] == '*' && _src[_i + 1] == '/'))
                        _i++;
                    if (_i + 1 >= _src.Length)
                        throw new ParseException("Unterminated block comment.", _i);
                    _i += 2;
                    continue;
                }

                break;
            }
        }

        private Token ReadString()
        {
            int start = _i;
            _i++; // consume "

            var sb = new StringBuilder();
            while (_i < _src.Length)
            {
                char c = _src[_i++];

                if (c == '"')
                    return new Token(TokenKind.String, sb.ToString(), start);

                if (c == '\\')
                {
                    if (_i >= _src.Length) throw new ParseException("Unterminated string escape.", _i);

                    char e = _src[_i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        default: throw new ParseException($"Unknown escape '\\{e}'.", _i - 1);
                    }
                    continue;
                }

                sb.Append(c);
            }

            throw new ParseException("Unterminated string literal.", start);
        }

        private Token ReadNumber()
        {
            int start = _i;
            bool hasDot = false;

            while (_i < _src.Length)
            {
                char c = _src[_i];
                if (char.IsDigit(c)) { _i++; continue; }
                if (c == '.' && !hasDot)
                {
                    hasDot = true;
                    _i++;
                    continue;
                }
                break;
            }

            var text = _src.Substring(start, _i - start);
            // Validate parse early for better errors
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                throw new ParseException($"Invalid number literal '{text}'.", start);

            return new Token(TokenKind.Number, text, start);
        }

        private Token ReadIdentifierOrKeyword()
        {
            int start = _i;
            _i++;

            while (_i < _src.Length && IsIdentPart(_src[_i]))
                _i++;

            var text = _src.Substring(start, _i - start);

            if (Keywords.TryGetValue(text, out var kind))
                return new Token(kind, text, start);

            return new Token(TokenKind.Identifier, text, start);
        }

        private bool Peek2(string s)
        {
            if (_i + 1 >= _src.Length) return false;
            return _src[_i] == s[0] && _src[_i + 1] == s[1];
        }

        private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_' ;
        private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_' ;
    }
}