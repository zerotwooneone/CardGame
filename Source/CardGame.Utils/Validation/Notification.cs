using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardGame.Utils.Validation
{
    public class Notification
    {
        private readonly IList<Error> _errors;
        private readonly List<string> _stateChanges;
        public IEnumerable<string> StateChanges => _stateChanges;

        public Notification()
        {
            _errors = new List<Error>();
            _stateChanges = new List<string>();
        }

        public void AddError(string message, Exception exception = null)
        {
            _errors.Add(new Error(message, exception));
        }

        public bool HasErrors()
        {
            return _errors.Any();
        }

        public string ErrorMessage()
        {
            if (!HasErrors()) return null;
            var sb = new StringBuilder();
            foreach (var error in _errors)
            {
                sb.Append(error.ToString());
            }
            return sb.ToString();
        }

        private class Error
        {
            public string Message { get; }
            public Exception Exception { get; }

            public Error(string message, Exception exception = null)
            {
                Message = message;
                Exception = exception;
            }

            public override string ToString()
            {
                return string.Join(Environment.NewLine, Message, Exception?.ToString());
            }
        }

        public void AddStateChange(string token)
        {
            _stateChanges.Add(token);
        }
    }
}
