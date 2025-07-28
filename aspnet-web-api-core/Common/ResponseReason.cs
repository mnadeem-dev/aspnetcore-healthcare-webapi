using System.ComponentModel;
using System.Reflection;

namespace AspNetWebApiCore.Common
{
    public static class ResponseReason
    {
        public enum EnumReason
        {
            [Description("Empty Parameter")]
            EmptyParameter = 1,

            [Description("Success")]
            Success = 2,

            [Description("Internal Server Error")]
            Error = 3,

            [Description("Record not found")]
            NotFound = 4,

            [Description("Record already exists")]
            RecordAlreadyExist = 5,

            [Description("Sql Server Exception")]
            SqlException = 6        

        }
       
        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
        public static string ToDescriptionString(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
        public static T FromDescriptionString<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
        }
    }
}
