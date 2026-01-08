#nullable enable
using System;
using System.Globalization;

namespace WizardBeardStudio.ErebusEngine.RulesEngine.Core
{
    public readonly struct RuleValue : IEquatable<RuleValue>
    {
        public RuleValueKind Kind { get; }

        private readonly long _i64;
        private readonly double _f64;
        private readonly string? _str;

        private RuleValue(RuleValueKind kind, long i64, double f64, string? str)
        {
            Kind = kind;
            _i64 = i64;
            _f64 = f64;
            _str = str;
        }

        public static RuleValue Null() => new RuleValue(RuleValueKind.Null, 0, 0, null);
        public static RuleValue FromBool(bool v) => new RuleValue(RuleValueKind.Bool, v ? 1 : 0, 0, null);
        public static RuleValue FromInt(long v) => new RuleValue(RuleValueKind.Int, v, 0, null);
        public static RuleValue FromDouble(double v) => new RuleValue(RuleValueKind.Double, 0, v, null);
        public static RuleValue FromString(string v) => new RuleValue(RuleValueKind.String, 0, 0, v ?? string.Empty);

        public bool AsBoolOrThrow()
        {
            return Kind switch
            {
                RuleValueKind.Bool => _i64 != 0,
                _ => throw new InvalidOperationException($"RuleValue is {Kind}, not Bool.")
            };
        }

        public long AsIntOrThrow()
        {
            return Kind switch
            {
                RuleValueKind.Int => _i64,
                RuleValueKind.Bool => _i64,
                _ => throw new InvalidOperationException($"RuleValue is {Kind}, not Int.")
            };
        }

        public double AsDoubleOrThrow()
        {
            return Kind switch
            {
                RuleValueKind.Double => _f64,
                RuleValueKind.Int => _i64,
                RuleValueKind.Bool => _i64,
                _ => throw new InvalidOperationException($"RuleValue is {Kind}, not Double.")
            };
        }

        public string AsStringOrThrow()
        {
            return Kind switch
            {
                RuleValueKind.String => _str ?? string.Empty,
                _ => throw new InvalidOperationException($"RuleValue is {Kind}, not String.")
            };
        }

        public bool TryGetNumber(out double v)
        {
            switch (Kind)
            {
                case RuleValueKind.Double: v = _f64; return true;
                case RuleValueKind.Int: v = _i64; return true;
                case RuleValueKind.Bool: v = _i64; return true;
                default: v = 0; return false;
            }
        }

        public bool IsTruthy()
        {
            return Kind switch
            {
                RuleValueKind.Null => false,
                RuleValueKind.Bool => _i64 != 0,
                RuleValueKind.Int => _i64 != 0,
                RuleValueKind.Double => Math.Abs(_f64) > 0.0,
                RuleValueKind.String => !string.IsNullOrEmpty(_str),
                _ => false
            };
        }

        public override string ToString()
        {
            return Kind switch
            {
                RuleValueKind.Null => "null",
                RuleValueKind.Bool => (_i64 != 0) ? "true" : "false",
                RuleValueKind.Int => _i64.ToString(CultureInfo.InvariantCulture),
                RuleValueKind.Double => _f64.ToString(CultureInfo.InvariantCulture),
                RuleValueKind.String => _str ?? string.Empty,
                _ => string.Empty
            };
        }

        public bool Equals(RuleValue other)
        {
            if (Kind != other.Kind) return false;

            return Kind switch
            {
                RuleValueKind.Null => true,
                RuleValueKind.Bool => _i64 == other._i64,
                RuleValueKind.Int => _i64 == other._i64,
                RuleValueKind.Double => _f64.Equals(other._f64),
                RuleValueKind.String => string.Equals(_str, other._str, StringComparison.Ordinal),
                _ => false
            };
        }

        public override bool Equals(object? obj) => obj is RuleValue rv && Equals(rv);

        public override int GetHashCode()
        {
            return Kind switch
            {
                RuleValueKind.Null => 0,
                RuleValueKind.Bool => HashCode.Combine((byte)Kind, _i64),
                RuleValueKind.Int => HashCode.Combine((byte)Kind, _i64),
                RuleValueKind.Double => HashCode.Combine((byte)Kind, _f64),
                RuleValueKind.String => HashCode.Combine((byte)Kind, _str),
                _ => 0
            };
        }

        public static bool operator ==(RuleValue a, RuleValue b) => a.Equals(b);
        public static bool operator !=(RuleValue a, RuleValue b) => !a.Equals(b);
    }
}
