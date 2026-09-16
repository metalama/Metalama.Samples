using System.ComponentModel.DataAnnotations;

namespace Metalama.Samples.Builder2.Tests.Validate;

[GenerateBuilder]
public partial class Invoice
{
    [Required]
    public string Number { get; }

    [Required]
    public string Caption { get; }

    public decimal Amount { get; }

    public decimal Discount { get; }

    protected void Validate()
    {
        if ( this.Discount > this.Amount )
        {
            throw new ValidationException( "Discount must be smaller than or equal to Amount" );
        }
    }
}