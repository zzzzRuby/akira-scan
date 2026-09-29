using System;

namespace AkiraScan.Camera.Utils;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Property)]
internal sealed class NativeTypeNameAttribute(string name) : Attribute
{
    public string Name => name;
}