namespace dnt.core.Services
{
    using System.Collections.Generic;
    using MoreLinq;

    public class ServiceResult<T> : ServiceResult where T : class, new()
    {
        public static ServiceResult<T> CreateSuccessfulResult(T result)
        {
            var serviceResult = new ServiceResult<T>(result) { Success = true };
            return serviceResult;
        }

        public static ServiceResult<T> CreateFailedResult(string error)
        {
            var serviceResult = new ServiceResult<T> { Success = false };
            serviceResult.AddError(error);
            return serviceResult;
        }

        /// <summary>
        /// Shallow copies the <see cref="result"/> into a new generically typed instance.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static ServiceResult<T> CloneFailedResult(ServiceResult result)
        {
            var serviceResult = new ServiceResult<T>(result) { Success = false };
            return serviceResult;
        }

        private ServiceResult()
        {
        }

        private ServiceResult(ServiceResult result)
        {
            Success = result.Success;
            Warnings = result.Warnings;
            Errors = result.Errors;
        }

        public ServiceResult(T result) : base(true)
        {
            Result = result;
        }
        
        public T Result { get; }
    }

    public class ServiceResult
    {
        internal ServiceResult()
        {
            Warnings = new List<FeedbackMessage>();
            Errors = new List<FeedbackMessage>();
        }

        public ServiceResult(bool success) : this()
        {
            Success = success;
        }

        public bool Success { get; internal set; }
        public ICollection<FeedbackMessage> Warnings { get; internal set; }
        public ICollection<FeedbackMessage> Errors { get; internal set; }

        public bool HasWarnings => Warnings.Count > 0;
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// Adds the <paramref name="warning"/> but leaves <see cref="Success"/> unchanged.
        /// </summary>
        /// <param name="warning"></param>
        public void AddWarning(string warning)
        {
            if (string.IsNullOrWhiteSpace(warning))
            {
                return;
            }

            Warnings.Add(new FeedbackMessage(warning));
        }

        /// <summary>
        /// Adds the <paramref name="warning"/> but leaves <see cref="Success"/> unchanged.
        /// </summary>
        /// <param name="warning"></param>
        public void AddWarning(FeedbackMessage warning)
        {
            if (warning == null)
            {
                return;
            }

            Warnings.Add(warning);
        }

        /// <summary>
        /// Adds the <paramref name="error"/> and sets <see cref="Success"/> to false.
        /// </summary>
        /// <param name="error"></param>
        public void AddError(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return;
            }

            Errors.Add(new FeedbackMessage(error));
            Success = false;
        }

        /// <summary>
        /// Adds the <paramref name="error"/> and sets <see cref="Success"/> to false.
        /// </summary>
        /// <param name="error"></param>
        public void AddError(FeedbackMessage error)
        {
            if (error == null)
            {
                return;
            }

            Errors.Add(error);
            Success = false;
        }

        public void AddErrorRange(IEnumerable<FeedbackMessage> errors)
        {
            if (errors == null)
            {
                return;
            }

            errors.ForEach(e => AddError(e));
        }
    }
}
