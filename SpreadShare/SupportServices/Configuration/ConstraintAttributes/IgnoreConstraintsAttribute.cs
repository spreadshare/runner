using System;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Attributes that indicates that the ConfigurationValidator should ignore this value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreConstraintsAttribute : Attribute
    {
    }
}