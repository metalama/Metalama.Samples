using Metalama.Samples.Authorization.Demo.Sales;

namespace Metalama.Samples.Authorization.Demo.Services;

/// <summary>
/// An <see cref="IInvoiceSender"/> that stores the messages in memory instead of sending emails.
/// </summary>
public sealed class OutboxInvoiceSender : IInvoiceSender
{
    private readonly List<SentInvoice> _messages = [];

    public IReadOnlyList<SentInvoice> Messages
    {
        get
        {
            lock ( this._messages )
            {
                return [.. this._messages];
            }
        }
    }

    public Task SendAsync( Invoice invoice, CancellationToken cancellationToken )
    {
        // The invoice is readable because IInvoiceSender grants Read on it. The customer is another object: the user
        // who sends the invoice must be able to read it.
        var message = new SentInvoice( invoice.Number, invoice.Customer.Email, invoice.Total );

        lock ( this._messages )
        {
            this._messages.Add( message );
        }

        return Task.CompletedTask;
    }
}

public sealed record SentInvoice( string Number, string? To, decimal Total );
