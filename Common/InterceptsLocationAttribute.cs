// ReSharper disable once CheckNamespace


namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
// public sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute;
#pragma warning disable CS9113 // Parameter is unread.
public sealed class InterceptsLocationAttribute(i32 version, string data) : Attribute;
#pragma warning restore CS9113 // Parameter is unread.
