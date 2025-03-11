namespace Wanadi.Common.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class CsvFieldAttribute : Attribute
{
    public CsvFieldAttribute(
        int index,
        bool removeStartAndEndQuotes = false,
        bool replaceEmptyToNull = false,
        bool removeLeftZerosCharacters = false,
        bool removeNotNumericCharacters = false,
        bool removeDoubleSpacesAndEmptyCharacters = false,
        bool convertData = false,
        bool useConvertHelper = false,
        Type? typeToConvert = null,
        int? padLeftLength = null,
        char? padLeftCharacter = null)
    {
        if (index < 0)
            throw new IndexOutOfRangeException($"Zero-based index cannot be less than zero. Index: {index}");

        if ((convertData || useConvertHelper) && typeToConvert == null)
            throw new ArgumentNullException("typeToConvert");

        if (typeToConvert != null && (!convertData && !useConvertHelper))
            throw new Exception("When the type is entered, it is necessary to inform whether the data will be converted using the native class Convert or ConvertHelper (UseConvertHelper).");

        if (padLeftLength.HasValue && !padLeftCharacter.HasValue)
            throw new ArgumentNullException("padLeftCharacter");

        if (!padLeftLength.HasValue && padLeftCharacter.HasValue)
            throw new ArgumentNullException("padLeftLength");

        if (padLeftLength.HasValue && padLeftCharacter.HasValue && padLeftLength.Value == 0)
            throw new IndexOutOfRangeException($"PadLeftLength cannot be less or equals than zero. PadLeftLength: {padLeftLength}");

        Index = index;
        RemoveStartAndEndQuotes = removeStartAndEndQuotes;
        ReplaceEmptyToNull = replaceEmptyToNull;
        RemoveLeftZerosCharacters = removeLeftZerosCharacters;
        RemoveNotNumericCharacters = removeNotNumericCharacters;
        RemoveDoubleSpacesAndEmptyCharacters = removeDoubleSpacesAndEmptyCharacters;
        ConvertData = convertData;
        UseConvertHelper = useConvertHelper;
        PadLeftLength = padLeftLength;
        PadLeftCharacter = padLeftCharacter;

        if (useConvertHelper && !convertData)
        {
            ConvertData = true;
        }

        TypeToConvert = typeToConvert;
    }

    public CsvFieldAttribute(int index, bool removeStartAndEndQuotes, Type typeToConvert)
    {
        if (index < 0)
            throw new IndexOutOfRangeException($"Zero-based index cannot be less than zero. Index: {index}");

        Index = index;
        TypeToConvert = typeToConvert;
        RemoveStartAndEndQuotes = removeStartAndEndQuotes;

        if (typeof(int) == typeToConvert)
        {
            ReplaceEmptyToNull = false;
            RemoveLeftZerosCharacters = true;
            RemoveNotNumericCharacters = true;
            RemoveDoubleSpacesAndEmptyCharacters = true;
            ConvertData = true;
            UseConvertHelper = true;
            PadLeftLength = null;
            PadLeftCharacter = null;
            return;
        }

        if (typeof(decimal) == typeToConvert)
        {
            ReplaceEmptyToNull = false;
            RemoveLeftZerosCharacters = true;
            RemoveNotNumericCharacters = false;
            RemoveDoubleSpacesAndEmptyCharacters = true;
            ConvertData = true;
            UseConvertHelper = true;
            PadLeftLength = null;
            PadLeftCharacter = null;
            return;
        }
    }

    public CsvFieldAttribute(int index, bool removeStartAndEndQuotes, bool replaceEmptyToNull, bool removeDoubleSpacesAndEmptyCharacters) : this(
        index: index,
        removeStartAndEndQuotes: removeStartAndEndQuotes,
        replaceEmptyToNull: replaceEmptyToNull,
        removeLeftZerosCharacters: false,
        removeNotNumericCharacters: false,
        removeDoubleSpacesAndEmptyCharacters: removeDoubleSpacesAndEmptyCharacters,
        convertData: false,
        useConvertHelper: false,
        typeToConvert: null,
        padLeftLength: null,
        padLeftCharacter: null)
    { }

    public CsvFieldAttribute(int index, bool removeStartAndEndQuotes, int padLeftLength, char padLeftCharacter) : this(
        index: index,
        removeStartAndEndQuotes: removeStartAndEndQuotes,
        replaceEmptyToNull: false,
        removeLeftZerosCharacters: true,
        removeNotNumericCharacters: true,
        removeDoubleSpacesAndEmptyCharacters: true,
        convertData: false,
        useConvertHelper: false,
        typeToConvert: null,
        padLeftLength: padLeftLength,
        padLeftCharacter: padLeftCharacter)
    { }

    public int Index { get; private set; }
    public bool RemoveStartAndEndQuotes { get; private set; }
    public bool ReplaceEmptyToNull { get; private set; }
    public bool RemoveLeftZerosCharacters { get; private set; }
    public bool RemoveNotNumericCharacters { get; private set; }
    public bool RemoveDoubleSpacesAndEmptyCharacters { get; private set; }
    public bool ConvertData { get; private set; }
    public bool UseConvertHelper { get; private set; }
    public Type? TypeToConvert { get; private set; }
    public int? PadLeftLength { get; private set; }
    public char? PadLeftCharacter { get; private set; }
}