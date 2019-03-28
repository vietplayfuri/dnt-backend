namespace dnt.core.Models.Response
{
    using System.Collections.Generic;

    public class ErrorResponse : OperationResponse
    {
        public string StackTrace { get; set; }

        public ErrorResponse(IEnumerable<string> messages)
            : base(false, messages)
        {
        }

        public ErrorResponse(string message)
            : base(false, message)
        {
        }

        public ErrorResponse()
            : base()
        {
            Success = false;
        }
    }
}
