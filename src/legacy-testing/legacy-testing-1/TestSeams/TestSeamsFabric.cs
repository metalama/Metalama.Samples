using Metalama.Extensions.Interceptors;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using System.Net.Mail;

namespace Metalama.Samples.LegacyTesting1.TestSeams;

// Redirects the calls to the hard-wired dependencies of the legacy code to the fakes.
internal class TestSeamsFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        // The project defines the TEST_SEAMS symbol only in the Test configuration.
        if ( !amender.Project.PreprocessorSymbols.Contains( "TEST_SEAMS" ) )
        {
            return;
        }

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(Thread) )
                .Methods( nameof(Thread.Sleep) )
                .Where( m => m.Parameters is [{ Type.SpecialType: SpecialType.Int32 }] )
                .RedirectToExistingMethod( GetFake( typeof(FakeClock), nameof(FakeClock.Sleep) ) ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(File) )
                .Methods( nameof(File.ReadAllText) )
                .Where( m => m.Parameters.Count == 1 )
                .RedirectToExistingMethod( GetFake( typeof(FakeFileSystem), nameof(FakeFileSystem.ReadAllText) ) ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(BillingDatabase) )
                .Methods( nameof(BillingDatabase.GetCustomerEmail) )
                .RedirectToExistingMethod(
                    GetFake( typeof(FakeBillingDatabase), nameof(FakeBillingDatabase.GetCustomerEmail) ) ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(BillingDatabase) )
                .Methods( nameof(BillingDatabase.GetOverdueInvoices) )
                .RedirectToExistingMethod(
                    GetFake( typeof(FakeBillingDatabase), nameof(FakeBillingDatabase.GetOverdueInvoices) ) ) );

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(SmtpClient) )
                .Methods( nameof(SmtpClient.Send) )
                .Where( m => m.Parameters.Count == 4 )
                .RedirectToExistingMethod( GetFake( typeof(FakeMailServer), nameof(FakeMailServer.Send) ) ) );
    }

    private static IMethod GetFake( Type type, string methodName )
        => type.AsINamedType().Methods.OfName( methodName ).Single();
}
