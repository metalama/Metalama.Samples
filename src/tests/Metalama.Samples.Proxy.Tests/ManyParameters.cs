using Metalama.Samples.Proxy;
using Metalama.Samples.Proxy.Tests.ManyParameters;

[assembly: GenerateProxyAspect( typeof(ISomeInterface), "SomeProxy", "Metalama.Samples.Proxy.Tests" )]

namespace Metalama.Samples.Proxy.Tests.ManyParameters;

public interface ISomeInterface
{
    void Void10Params(
        int p1,
        string p2,
        long p3,
        DateOnly p4,
        TimeSpan p5,
        Guid p6,
        short p7,
        int? p8,
        TimeZoneInfo? p9,
        object p10 );

    void NonVoid10Params(
        int p1,
        string p2,
        long p3,
        DateOnly p4,
        TimeSpan p5,
        Guid p6,
        short p7,
        int? p8,
        TimeZoneInfo? p9,
        object p10 );

    void Void20Params(
        int p1,
        string p2,
        long p3,
        DateOnly p4,
        TimeSpan p5,
        Guid p6,
        short p7,
        int? p8,
        TimeZoneInfo? p9,
        object p10,
        string? p11,
        string p12,
        DateTime p13,
        IDisposable p14,
        DateTime p15,
        DateTime p16,
        DateTime p17,
        long? p18,
        object p19,
        object p20 );

    long NonVoid20Params(
        int p1,
        string p2,
        long p3,
        DateOnly p4,
        TimeSpan p5,
        Guid p6,
        short p7,
        int? p8,
        TimeZoneInfo? p9,
        object p10,
        string? p11,
        string p12,
        DateTime p13,
        IDisposable p14,
        DateTime p15,
        DateTime p16,
        DateTime p17,
        long? p18,
        object p19,
        object p20 );
}