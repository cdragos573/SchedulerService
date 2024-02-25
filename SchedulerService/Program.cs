
using CommonInfra;
using Quartz;
using Quartz.AspNetCore;
using StackExchange.Redis;
using SchedulerService.Services;

namespace SchedulerService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IQuartzAdapterService, QuartzAdapterService>();
        builder.Services.AddKeyedScoped<IJobExecuter, RestJobExecuterStrategy>("REST");
        builder.Services.AddKeyedScoped<IJobExecuter, MassTransitJobExecuterStrategy>("MassTransit");
        RegisterRedis(builder.Services, builder.Configuration);
        builder.Services.AddScoped<IRedisJobRepository, RedisJobRepository>();
        builder.Services.AddScoped<IJobManagementService, JobManagementService>();
        builder.Services.AddScoped<IJobSyncService, JobSyncService>();
        builder.Services.AddSingleton<IQuartzJobMetricsService, QuartzJobMetricsService>();
        builder.Services.AddHttpClient();
        builder.Services.AddHealthChecks()
            .AddRedis(builder.Configuration.GetConnectionString("Redis")!, "Redis");
        builder.Services.AddCommonInfra(builder.Configuration);

        builder.Services.AddQuartz(q =>
        {
            q.SchedulerId = "Scheduler-Core";
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            //q.UsePersistentStore(cfg =>
            //{
            //    cfg.UseNewtonsoftJsonSerializer();
            //    cfg.UsePostgres("Server=192.168.0.206;Database=quartznetdb;User Id=pi;Password=admin;");
            //});
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 2;
            });
        });

        // ASP.NET Core hosting
        builder.Services.AddQuartzServer(options =>
        {
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = true;
        });

        var app = builder.Build();

        await SyncQuartzJobsAsync(app);

        app.MapCommonInfra(builder.Configuration);
        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        app.UseSwagger();
        app.UseSwaggerUI();
        //}

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }

    private static IServiceCollection RegisterRedis(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp 
            => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost"));
        services.AddScoped(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

        return services;
    }

    private static async Task SyncQuartzJobsAsync(WebApplication app)
    {        
        using var scope = app.Services.CreateScope();
        var syncService = scope.ServiceProvider.GetRequiredService<IJobSyncService>();
        using var cts = new CancellationTokenSource(5000);
        await syncService.SynchronizeJobsAsync(cts.Token);
    }
}
