using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TrainingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TrainingInstituteDB"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,     
            maxRetryDelay: TimeSpan.FromSeconds(10), 
            errorNumbersToAdd: null
        )
    ));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins("http://localhost", "https://localhost:57327")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddControllers();

var app = builder.Build();
app.UseCors();

app.UseDefaultFiles();
app.MapStaticAssets();
    


//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
