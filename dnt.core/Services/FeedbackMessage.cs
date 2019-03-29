namespace dnt.core.Services
{
    using System.Collections.Generic;

    public class FeedbackMessage
    {
        public FeedbackMessage(string message)
        {
            Message = message;
        }

        public string Message { get; }
        public IList<string> Suggestions { get; private set; }

        public void AddSuggestion(string suggestion)
        {
            if (!string.IsNullOrEmpty(suggestion))
            {
                // Create only if required so that we don't send it over the wire as an empty array.
                if (Suggestions == null)
                {
                    Suggestions = new List<string>();
                }

                Suggestions.Add(suggestion);
            }
        }
    }
}
