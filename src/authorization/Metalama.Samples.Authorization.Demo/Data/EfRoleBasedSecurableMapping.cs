using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metalama.Samples.Authorization.Demo.Data;

public static class EfRoleBasedSecurableMapping
{
    /// <summary>
    /// Maps the storage members of a class derived from <see cref="EfRoleBasedSecurable"/>: the owner and the role
    /// assignments.
    /// </summary>
    public static EntityTypeBuilder<T> ConfigureRoleBasedStorage<T>( this EntityTypeBuilder<T> entity )
        where T : EfRoleBasedSecurable
    {
        // The public members are the secured, computed views of the storage members.
        entity.Ignore( e => e.Owner );
        entity.Ignore( e => e.RoleAssignments );

        // The security model compares owners and role assignments, so they are always loaded.
        entity.HasOne<User>( EfRoleBasedSecurable.OwnerUserMember ).WithMany();
        entity.Navigation( EfRoleBasedSecurable.OwnerUserMember ).AutoInclude();

        entity.OwnsMany<RoleAssignmentRecord>(
            EfRoleBasedSecurable.RoleAssignmentRecordsMember,
            record =>
            {
                // SQLite cannot generate the composite key that Entity Framework gives to owned collections by default.
                record.Property<int>( "Id" );
                record.HasKey( "Id" );
                record.HasOne( r => r.User ).WithMany();
                record.Navigation( r => r.User ).AutoInclude();
            } );

        return entity;
    }
}
