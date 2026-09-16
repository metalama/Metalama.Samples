using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

internal class AddEmptyCloneMembersAspect : IAspect<INamedType>
{
    public void BuildAspect( IAspectBuilder<INamedType> builder )
        => builder.IntroduceMethod(
            nameof(this.CloneMembers),
            whenExists: OverrideStrategy.Override,
            args: new { T = builder.Target } );

    // ReSharper disable once UnusedParameter.Local

    [Template]
    private void CloneMembers<[CompileTime] T>( T clone )
    {
        meta.InsertComment( "Use this method to modify the 'clone' parameter." );
        meta.InsertComment( "Your code executes after the aspect." );
    }
}