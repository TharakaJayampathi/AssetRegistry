namespace AssetRegistry.DTOs
{
    public class Result
    {
        internal Result(bool succeeded, IEnumerable<string> errors)
        {
            Succeeded = succeeded;
            Messages = errors.ToArray();
        }

        internal Result(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Message = message;
        }

        public bool Succeeded { get; set; }

        public string[] Messages { get; set; }

        public string Message { get; set; }

        public static Result<TValue> Success<TValue>(TValue value) => new Result<TValue>(value, true);

        public static Result Success()
        {
            return new Result(true, Array.Empty<string>());
        }

        public static Result Success(string message)
        {
            return new Result(true, message);
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors);
        }

        public static Result Failure(string error)
        {
            return new Result(false, error);
        }

        public static Result<TValue> Failure<TValue>(TValue value) => new Result<TValue>(value, false);
    }

    public class Result<TValue> : Result
    {
        public Result(TValue value, bool succeeded) : base(succeeded, Array.Empty<string>())
        {
            Value = value;
        }

        public Result(TValue value, bool succeeded, IEnumerable<string> errors) : base(succeeded, errors)
        {
            Value = value;
        }

        public Result(TValue value, bool succeeded, string error) : base(succeeded, error)
        {
            Value = value;
        }

        public TValue Value { get; }
    }
}
