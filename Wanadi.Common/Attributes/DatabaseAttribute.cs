namespace Wanadi.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class DatabaseAttribute : Attribute
{
    public DatabaseAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException("name", "Name cannot be empty/null.");

        Name = name;
    }

    public string Name { get; set; }
}