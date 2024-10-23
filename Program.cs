using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DiscussedApi.Configuration;
using DiscussedApi.Models;
using DiscussedApi.Processing.UserPocessing;
using DiscussedApi.Processing.UserProcessing;
using DiscussedApi.Services.Email;
using DiscussedApi.Services.Tokens;
using System.Text;
using DiscussedApi.Data.Identity;
using DiscussedApi.Data.UserComments;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using DiscussedApi.Processing.Comments;
using DiscussedApi.Reopisitory;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

//Nlog Set up
var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config");
builder.Logging.AddNLog();

Settings.Initialize(builder.Configuration);



//Entity Framework
builder.Services.AddDbContext<ApplicationIdentityDBContext>(options =>
{
    options.UseMySql(Settings.ConnectionString.UserInfo, ServerVersion.AutoDetect(Settings.ConnectionString.UserInfo));
});

builder.Services.AddDbContext<CommentsDBContext>(options =>
{
    options.UseMySql(Settings.ConnectionString.UserInfo, ServerVersion.AutoDetect(Settings.ConnectionString.UserInfo));
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
}).AddEntityFrameworkStores<ApplicationIdentityDBContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = 
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme = 
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Settings.JwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = Settings.JwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSettings.Key))
    };
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(g =>
{
    g.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Personal Finance Manager Api V1",
        Version = "V1"
    });

    g.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Autherization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    g.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { 
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new String[]{ }
        }

    });
});

//Dependcy Injections
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IUserProcessing, UserProcessing>();
builder.Services.AddTransient<ICommentProcessing, CommentProcessing>();
builder.Services.AddTransient<ICommentDataAccess, CommentDataAccess>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
