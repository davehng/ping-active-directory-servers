using System;

namespace PingAllDcs
{
    public sealed class ValidationResult
    {
        public static readonly ValidationResult Success = new ValidationResult() { Value = true };
        public static readonly ValidationResult Failed = new ValidationResult() { Value = false };

        public Exception Error { get; private set; }
        public bool Value { get; private set; }

        private ValidationResult()
        {
        }

        public ValidationResult(Exception error)
        {
            Value = false;
            Error = error;
        }
    }
}