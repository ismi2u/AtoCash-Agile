global using EmailService;

using AtoCashAPI.Authentication;
using AtoCashAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Configuration;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//serilog
var _loggerconf = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("AtoCash-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.AddSerilog(_loggerconf);


//FUCOAzurePostgresSQLServer
//AzureCloudGmailServer
//PostgreSQLInLocalAppInContainer
builder.Services.AddDbContextPool<AtoCashDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("AzureCloudGmailServer")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AtoCashDbContext>()
                .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddAuthentication(options =>
            {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // setting it to false, as we dont know the users connecting to this server
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,


        ValidIssuer = "https://localhost:5000",
        ValidAudience = "https://localhost:5000",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySecretKey12323232"))
    };
});

builder.Services.AddCors(options =>
               options.AddPolicy("myCorsPolicy", builder =>
               {
                   builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
               }
               ));

//email service
//builder.Services.AddSingleton(builder.Configuration.GetSection("EmailConfiguration").Get<EmailDto>());
builder.Services.AddScoped<IEmailSender, EmailSender>();

//for file upload from Angular Form
builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});


var app = builder.Build();

IWebHostEnvironment env = app.Environment;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseAuthentication(); //add before MVC
app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, @"Images")),
    RequestPath = "/app/Images"
    //RequestPath = Path.DirectorySeparatorChar + "app" + Path.DirectorySeparatorChar + "Images"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, @"Reportdocs")),
    RequestPath = "/app/Reportdocs"
    //RequestPath = Path.DirectorySeparatorChar + "app" + Path.DirectorySeparatorChar + "Reportdocs"
});

app.UseHttpsRedirection();//https for cors


app.UseCors("myCorsPolicy");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
