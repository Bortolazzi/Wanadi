namespace Wanadi.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DependencyInjectionAttribute(InjectionType injectionType) : Attribute
{
    public readonly InjectionType InjectionType = injectionType;
}