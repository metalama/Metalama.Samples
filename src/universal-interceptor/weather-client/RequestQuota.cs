namespace Metalama.Samples.UniversalInterceptor.Weather;

// The number of requests that the application can still send. It is a mutable struct: TryConsume changes the instance on
// which it is called.
public struct RequestQuota
{
    public RequestQuota( int remaining )
    {
        this.Remaining = remaining;
    }

    public int Remaining { get; private set; }

    public bool TryConsume()
    {
        if ( this.Remaining == 0 )
        {
            return false;
        }

        this.Remaining--;

        return true;
    }

    public override readonly string ToString() => $"{this.Remaining} requests remaining";
}
