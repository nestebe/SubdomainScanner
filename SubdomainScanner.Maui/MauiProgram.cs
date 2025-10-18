using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using SubdomainScanner.Maui.Services;

namespace SubdomainScanner.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Add MudBlazor services
		builder.Services.AddMudServices();

		// Add HTTP client factory
		builder.Services.AddHttpClient();

		// Add Scanner service
		builder.Services.AddScoped<ScannerService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
