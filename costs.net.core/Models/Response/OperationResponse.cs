namespace dnt.core.Models.Response
{
    using System;
    using System.Collections.Generic;

    public class OperationResponse
    {
        private readonly List<string> _messages;

        public IEnumerable<string> Messages => _messages;

        public bool Success { get; set; }

        public object Object { get; set; }

        public OperationResponse()
        {
            _messages = new List<string>();
        }

        public OperationResponse(bool success, string message)
        {
            Success = success;
            _messages = new List<string> { message };
        }

        public OperationResponse(bool success, IEnumerable<string> messages)
        {
            Success = success;
            _messages = new List<string>(messages);
        }

        public void AddMessage(string message)
        {
            _messages.Add(message);
        }

        public static OperationResponse Ok => new OperationResponse { Success = true };

        public static readonly Func<object, OperationResponse> OkObject = obj => new OperationResponse { Success = true, Object = obj};
    }
}
