using Serilog;

namespace SageWebApi.Models;

public static class Extension
{
    public static void ConfigureSerilog(this IHostBuilder host)
    {
        host.UseSerilog((ctx, lc) =>
        {
            lc.WriteTo.Console();
        });
    }
}
