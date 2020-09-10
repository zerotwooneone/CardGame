namespace CardGame.Utils.Factory
{
    public class FactoryResult<T>
    {
        public T Value { get; }
        public string ErrorMessage { get; }
        public bool IsError { get; }
        protected FactoryResult(T value,
            string errorMessage = default)
        {
            Value = value;
            ErrorMessage = errorMessage;
            IsError = !string.IsNullOrWhiteSpace(errorMessage);
        }

        public static FactoryResult<T> Success(T value)
        {
            return new FactoryResult<T>(value);
        }

        public static FactoryResult<T> Error(string errorMessage)
        {
            var error = string.IsNullOrWhiteSpace(errorMessage)
                ? $"{(typeof(T))} has no error message"
                : errorMessage;
            return new FactoryResult<T>(default(T), errorMessage);
        }
    }
}