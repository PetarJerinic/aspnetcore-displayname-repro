using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;

using var listener = new ActivityListener
{
    ShouldListenTo = source => source.Name == "Microsoft.AspNetCore",
    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
    ActivityStopped = activity => Console.WriteLine($"Stopped: DisplayName = '{activity.DisplayName}'"),
};
ActivitySource.AddActivityListener(listener);

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5099");
builder.Logging.ClearProviders();
var app = builder.Build();

app.MapGet("/items/{id}", (HttpContext context) =>
{
    // The app gives the span a more useful name.
    context.Features.Get<IHttpActivityFeature>()!.Activity.DisplayName = "custom-name";
    return "ok";
});

await app.StartAsync();
using var client = new HttpClient();
await client.GetStringAsync("http://127.0.0.1:5099/items/1");
await app.StopAsync();

Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
