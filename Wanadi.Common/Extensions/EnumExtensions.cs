using System.ComponentModel;

namespace Wanadi.Common.Extensions;

public static class EnumExtensions
{
    public static string Description(this Enum @enum)
    {
        var enumType = @enum.GetType();
        if (enumType == null)
            return @enum.ToString();

        var infoEnum = enumType.GetField(@enum.ToString());
        if(infoEnum == null)
            return @enum.ToString();

        DescriptionAttribute[] attributesEnum = (DescriptionAttribute[])infoEnum.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributesEnum != null && attributesEnum.Length > 0)
            return attributesEnum[0].Description;
        else
            return @enum.ToString();
    }
}