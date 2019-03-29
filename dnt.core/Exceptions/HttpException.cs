using System;
using System.Net;

namespace dnt.core.Exceptions
{
    
    /// <summary>
    /// Describes an exception that occurred during the processing of HTTP requests.
    /// </summary>
    /// <seealso cref="Exception" />
    public class HttpException : Exception
    {
        private readonly int _httpStatusCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpException" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        public HttpException(int httpStatusCode)
        {
            _httpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpException" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        public HttpException(HttpStatusCode httpStatusCode)
        {
            _httpStatusCode = (int)httpStatusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpException" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="message">The exception message.</param>
        public HttpException(int httpStatusCode, string message) : base(message)
        {
            _httpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpException" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="message">The exception message.</param>
        public HttpException(HttpStatusCode httpStatusCode, string message) : base(message)
        {
            _httpStatusCode = (int)httpStatusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpException" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public HttpException(int httpStatusCode, string message, Exception inner) : base(message, inner)
        {
            _httpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpException" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public HttpException(HttpStatusCode httpStatusCode, string message, Exception inner) : base(message, inner)
        {
            _httpStatusCode = (int)httpStatusCode;
        }

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        /// <value>
        /// The HTTP status code.
        /// </value>
        public int StatusCode { get { return _httpStatusCode; } }
    }
}
