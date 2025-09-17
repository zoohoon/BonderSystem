using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace EnumHelperModule
{

    public static class EnumHelper
    {
        public static string Description(this Enum value)
        {
            try
            {
                var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Any())
                    return (attributes.First() as DescriptionAttribute).Description;

                // If no description is found, the least we can do is replace underscores with spaces
                // You can add your own custom default formatting logic here
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions(Type t)
        {
            try
            {
                if (!t.IsEnum)
                    throw new ArgumentException($"{nameof(t)} must be an enum type");

                return Enum.GetValues(t).Cast<Enum>().Select((e) => new ValueDescription() { Value = e, Description = e.Description() }).ToList();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
