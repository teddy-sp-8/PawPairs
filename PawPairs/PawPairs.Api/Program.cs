using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PawPairs.Api.External.CatApi;
using PawPairs.Api.External.DogApi;
using PawPairs.Infrastructure.Data;
using PawPairs.Application.Interfaces;
using PawPairs.Infrastructure.Services;
using PawPairs.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PawPairsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IPlaydateService, PlaydateService>();
builder.Services.AddScoped<IMatchRequestService, MatchRequestService>();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();
            
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { errors });
        };
    });
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();

builder.Services.Configure<DogApiOptions>(builder.Configuration.GetSection("DogApi"));

builder.Services.AddHttpClient<IDogApiClient, DogApiClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<DogApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.Configure<CatApiOptions>(
    builder.Configuration.GetSection("CatApi"));

builder.Services.AddHttpClient<ICatApiClient, CatApiClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<CatApiOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds);
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();
app.MapRazorPages();

app.Run();

