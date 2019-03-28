namespace dnt.core.Exceptions
{
    using System;

    public class XmgException : Exception
    {
        public XmgException()
        { }
        public XmgException(string message, Exception innException) : base(message, innException)
        { }

        public XmgException(string message) : base(message)
        { }

        public string ClientName { get; set; }
    }
}
