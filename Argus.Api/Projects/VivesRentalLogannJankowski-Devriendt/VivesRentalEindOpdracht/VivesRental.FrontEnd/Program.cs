using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VivesRental.FrontEnd;
using VivesRental.Sdk.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add VivesRental SDK
builder.Services.AddVivesRentalSdk("https://localhost:7162/");

await builder.Build().RunAsync();
