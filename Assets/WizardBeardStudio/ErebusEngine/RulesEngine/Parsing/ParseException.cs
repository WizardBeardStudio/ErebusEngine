using System;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Parsing
{
    public sealed class ParseException : Exception
    {
        public int Position { get; }

        public ParseException(string message, int pos) : base($"{message} (pos {pos})")
        {
            Position = pos;
        }
    }
}