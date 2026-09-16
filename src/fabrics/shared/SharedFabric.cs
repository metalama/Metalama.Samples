using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using Metalama.Shared;

public class SharedFabric : ProjectFabric
{
    /// <summary>
    /// The user can implement this method to analyze types in the current project, add aspects, and report or suppress diagnostics.
    /// </summary>
    public override void AmendProject(IProjectAmender amender)
    {
        amender.Outbound
            .SelectMany(compilation => compilation.AllTypes)
            .Where(type => (type.Accessibility is Accessibility.Public or Accessibility.Internal))
            .SelectMany(type => type.Methods)
            .Where(method => method.Accessibility == Accessibility.Public && method.Name != "ToString")
            .AddAspectIfEligible<LogAttribute>();
    }
}