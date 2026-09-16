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
    if (this.Discount > this.Amount)
    {
      throw new ValidationException("Discount must be smaller than or equal to Amount");
    }
  }
  protected Invoice(string number, string caption, decimal amount, decimal discount)
  {
    Number = number;
    Caption = caption;
    Amount = amount;
    Discount = discount;
  }
  public virtual Builder ToBuilder()
  {
    return new Builder(this);
  }
  public class Builder
  {
    public Builder(string number, string caption)
    {
      Number = number;
      Caption = caption;
    }
    protected internal Builder(Invoice source)
    {
      Number = source.Number;
      Caption = source.Caption;
      Amount = source.Amount;
      Discount = source.Discount;
    }
    public decimal Amount { get; set; }
    public string Caption { get; set; }
    public decimal Discount { get; set; }
    public string Number { get; set; }
    public Invoice Build()
    {
      var instance = new Invoice(Number, Caption, Amount, Discount);
      instance.Validate();
      return instance;
    }
  }
}
