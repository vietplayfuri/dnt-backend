namespace dnt.core.Extensions
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            // If there are 0 or 1 characters, just return the string.
            if (str == null || str.Length < 2)
            {
                return str;
            }

            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static T ToEnum<T>(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }

            return default(T);
        }

        public static string ToSnakeCase(this string value)
        {
            return !string.IsNullOrEmpty(value) ? string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower() : string.Empty;
        }

        public static TimeSpan ToTimespanFromDuration(this string value)
        {
            var duration = TimeSpan.Zero;

            if (string.IsNullOrEmpty(value))
            {
                return duration;
            }

            var testInt = 0;

            // let's try and first parse the given duration (00:12:12 will be fine)
            // if it fails (>24h value?), let's see if all the chars apart from ":" are numbers
            if (!TimeSpan.TryParseExact(value, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out duration)
                && int.TryParse(value.Replace(":", ""), out testInt))
            {
                // simple TimeSpan parsing failed but all chars are numbers, let's do it manually
                var split = value.Split(':').Select(x => int.Parse(x)).ToArray();
                if (split.Length > 0)
                {
                    duration = new TimeSpan(split[0], split[1], split[2]);
                }
            }

            return duration;
        }

        public static string AddSpacesToSentence(this string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    {
                        newText.Append(' ');
                    }
                }

                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}