namespace CVBuilder.API;
using CVBuilder.API.Data;
using CVBuilder.API.Services;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowNextJS", builder =>
            {
                builder
                    .WithOrigins("http://localhost:3000", "http://localhost:3001")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddControllers();
        builder.Services.AddScoped<ICVGenerationService, CVGenerationService>();
        builder.Services.AddScoped<ICVEnhancementService, CVEnhancementService>();
        builder.Services.AddScoped<ICVStructuringService, CVStructuringService>();
        builder.Services.AddScoped<ISkillMatchingService, SkillMatchingService>();
        builder.Services.AddScoped<ICvTailoringService, CvTailoringService>();
        builder.Services.AddScoped<ICvValidationService, CvValidationService>();
        builder.Services.AddScoped<IPdfExportService, PdfExportService>();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=cvbuilder.db"));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("AllowNextJS");

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
