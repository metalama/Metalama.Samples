using Metalama.Samples.Authorization.Demo.Finance;
using Metalama.Samples.Authorization.Demo.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// The Entity Framework model of the invoicing application (code first).
/// </summary>
public sealed class InvoicingDbContext( DbContextOptions<InvoicingDbContext> options ) : DbContext( options )
{
    public DbSet<User> Users => this.Set<User>();

    public DbSet<Company> Companies => this.Set<Company>();

    public DbSet<Department> Departments => this.Set<Department>();

    public DbSet<Customer> Customers => this.Set<Customer>();

    public DbSet<Invoice> Invoices => this.Set<Invoice>();

    public DbSet<Budget> Budgets => this.Set<Budget>();

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        // Entity Framework must never go through the secured accessors. Loading an object, detecting changes and
        // saving are infrastructure operations, not user operations: they often run without a security context,
        // and they read and write every property. Field access makes Entity Framework read and write the backing
        // fields directly, including the fields that Metalama generates for the overridden auto-properties.
        modelBuilder.UsePropertyAccessMode( PropertyAccessMode.Field );

        // By convention, Entity Framework maps only the properties that have a setter. The get-only properties that
        // expose the state of an object, such as Invoice.Status, must be mapped explicitly, or they are not saved.

        modelBuilder.Entity<User>( user => user.HasIndex( u => u.Name ).IsUnique() );

        ConfigureSecurable<Company>( modelBuilder );

        ConfigureSecurable<Department>( modelBuilder )
            .Entity<Department>(
                department =>
                {
                    department.Property( d => d.Code );
                    department.HasIndex( d => d.Code ).IsUnique();
                    department.HasOne( d => d.Company ).WithMany();
                    department.Property<int>( nameof(Department.LastInvoiceNumber) );
                } );

        ConfigureSecurable<Customer>( modelBuilder )
            .Entity<Customer>( customer => customer.HasOne( c => c.Department ).WithMany() );

        ConfigureSecurable<Invoice>( modelBuilder )
            .Entity<Invoice>(
                invoice =>
                {
                    invoice.Property( i => i.Number );
                    invoice.Property( i => i.Status );
                    invoice.Property( i => i.AmountPaid );
                    invoice.HasIndex( i => i.Number ).IsUnique();
                    invoice.HasOne( i => i.Department ).WithMany();
                    invoice.HasOne( i => i.Customer ).WithMany();
                    invoice.HasOne<User>( "CreatorUser" ).WithMany();
                    invoice.Ignore( i => i.Creator );
                    invoice.OwnsMany( i => i.Lines, line => UseSingleColumnKey( line ) );
                } );

        ConfigureSecurable<Budget>( modelBuilder )
            .Entity<Budget>(
                budget =>
                {
                    budget.HasOne( b => b.Department ).WithMany();
                    budget.Property( b => b.FiscalYear );
                    budget.Property( b => b.Amount );
                    budget.Property( b => b.Spent );
                } );

        // The security model walks the parent chain and compares owners and role assignments. Loading these
        // navigations automatically means that a role inherited from an unloaded parent is never missed.
        modelBuilder.Entity<Department>().Navigation( d => d.Company ).AutoInclude();
        modelBuilder.Entity<Customer>().Navigation( c => c.Department ).AutoInclude();
        modelBuilder.Entity<Invoice>().Navigation( i => i.Department ).AutoInclude();
        modelBuilder.Entity<Invoice>().Navigation( i => i.Customer ).AutoInclude();
        modelBuilder.Entity<Invoice>().Navigation( "CreatorUser" ).AutoInclude();
        modelBuilder.Entity<Budget>().Navigation( b => b.Department ).AutoInclude();
    }

    // SQLite cannot generate the composite key that Entity Framework gives to owned collections by default.
    private static void UseSingleColumnKey<TOwner, TOwned>( OwnedNavigationBuilder<TOwner, TOwned> owned )
        where TOwner : class
        where TOwned : class
    {
        owned.Property<int>( "Id" );
        owned.HasKey( "Id" );
    }

    // Maps the storage members of Entity: the owner and the role assignments.
    private static ModelBuilder ConfigureSecurable<T>( ModelBuilder modelBuilder )
        where T : Entity
        => modelBuilder.Entity<T>(
            entity =>
            {
                // The public members are the secured, computed views of the storage members.
                entity.Ignore( e => e.Owner );
                entity.Ignore( e => e.RoleAssignments );
                entity.Ignore( e => e.SecurityLabel );

                entity.HasOne<User>( "OwnerUser" ).WithMany();
                entity.Navigation( "OwnerUser" ).AutoInclude();

                entity.OwnsMany<RoleAssignmentRecord>(
                    "RoleAssignmentRecords",
                    record =>
                    {
                        UseSingleColumnKey( record );
                        record.HasOne( r => r.User ).WithMany();
                        record.Navigation( r => r.User ).AutoInclude();
                    } );
            } );
}
