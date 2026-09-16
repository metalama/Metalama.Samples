using Metalama.Framework.Aspects;

namespace Metalama.Samples.MauiExceptionTracking;

/// <summary>
/// Attribute that can be applied to methods of a type decorated with <see cref="TrackExceptionsAttribute"/>, means
/// that the exception handling should be applied to that method as well, even if it does not match the usual criteria.
/// </summary>
[AttributeUsage( AttributeTargets.Method )]
[RunTimeOrCompileTime]
public class EntryPointAttribute : Attribute;
