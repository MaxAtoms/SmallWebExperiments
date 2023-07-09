using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudblazorOidc.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
            
builder.Services.AddAuthentication(sharedOptions =>
	{
		sharedOptions.DefaultAuthenticateScheme = 
			CookieAuthenticationDefaults.AuthenticationScheme;
		sharedOptions.DefaultSignInScheme = 
			CookieAuthenticationDefaults.AuthenticationScheme;
		sharedOptions.DefaultChallengeScheme = 
			OpenIdConnectDefaults.AuthenticationScheme;
	})
	.AddCookie()
	.AddOpenIdConnect("oidc", options =>
	{
		var config = builder.Configuration.GetSection("Oidc").Get<OpenIdConnectOptions>();;
		options.Authority = config.Authority;
		options.ClientId = config.ClientId;
		options.ClientSecret = config.ClientSecret;
		
		options.ResponseType = "code";
		options.SaveTokens = true;
		options.GetClaimsFromUserInfoEndpoint = false;
		options.UseTokenLifetime = false;
		options.Scope.Add("email");
		options.Scope.Add("openid");
		options.Scope.Add("phone");
                    
		options.Events = new OpenIdConnectEvents
		{
			OnAccessDenied = context =>
			{
				context.HandleResponse();
				context.Response.Redirect("/");
				return Task.CompletedTask;
			}
		};
	});

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();