using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace dnt.api.Extensions
{
    public static class EnumExtensions
    {
        public static TAttribute GetEnumAttribute<TAttribute>(this Enum enumVal) where TAttribute : Attribute
        {
            var memberInfo = enumVal.GetType().GetMember(enumVal.ToString());
            return
                memberInfo.FirstOrDefault()?
                    .GetCustomAttributes(typeof(TAttribute), false)
                    .OfType<TAttribute>()
                    .FirstOrDefault();
        }

        public static string GetEnumDescription(this Enum enumValue)
            => enumValue.GetEnumAttribute<DescriptionAttribute>()?.Description ?? enumValue.ToString();
    }
}
