using Chatter.Server;
using Chatter.Server.Configuration;
using Chatter.Server.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
	});

builder.Services.AddOptionsWithValidateOnStart<AuthenticationOptions>()
	.BindConfiguration("JWT");

builder.Services.AddOptionsWithValidateOnStart<PasswordHasherOptions>()
	.BindConfiguration("PasswordHasher");

builder.Services.ConfigureJWTBearerAuthentication(builder);

builder.Services
	.AddScoped<IChatService, ChatService>()
	.AddScoped<IUserService, UserService>()
	.AddScoped<ISecureHasher, PasswordHasher>()
	.AddSingleton<IChatSubscriptionService, ChatSubscriptionService>();

builder.Services.AddDbContext<ChatDatabaseContext>(options =>
{
	options.UseNpgsql(builder.Configuration.GetConnectionString("postgres"), postgres =>
	{
		postgres.EnableRetryOnFailure(3);
	});

	options.UseSnakeCaseNamingConvention()
		.UseLazyLoadingProxies()
		.EnableDetailedErrors(builder.Environment.IsDevelopment())
		.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

var app = builder.Build();

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

app.Use((ctx, next) =>
{
	if (ctx.Request.Method == HttpMethods.Head && ctx.Request.Path == "/ping")
	{
		ctx.Response.StatusCode = StatusCodes.Status200OK;
		return Task.CompletedTask;
	}

	return next();
});

app.MapGet("/ping", () => new { message = "pong" });

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<ChatDatabaseContext>();
	await db.Database.MigrateAsync();

	var usersWithNoPassword = db.Users.Where(user => user.PasswordHash.Length == 0).ToList();

	if (usersWithNoPassword.Count > 0)
	{
		var hasher = scope.ServiceProvider.GetRequiredService<ISecureHasher>();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

		foreach (var user in usersWithNoPassword)
		{
			var newPassword = RandomNumberGenerator.GetHexString(12);
			user.PasswordHash = hasher.HashPassword(newPassword);
			logger.LogWarning("User {Username} has been assigned a password: `{Password}`", user.Username, newPassword);
		}

		await db.SaveChangesAsync();
	}
}

app.Run();
