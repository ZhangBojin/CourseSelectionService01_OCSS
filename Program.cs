using Consul;
using CourseSelectionService01_OCSS.Infrastructure.Consul;
using CourseSelectionService01_OCSS.Infrastructure.EfCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CourseSelectionService01_OCSS.Infrastructure.RabbitMq;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

#region 数据库上下文scoped注入
builder.Services.AddDbContext<CourseSelectionServiceOCSSDb>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
#endregion

#region 配置Jwt
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //当默认的身份验证方案失败时，应用将使用这个方案发起挑战
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetSection("JwtSettings")["Issuer"],
        ValidAudience = builder.Configuration.GetSection("JwtSettings")["Audience"],
        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtSettings")["SecretKey"]!))
    };
});
#endregion

#region 配置日志消息队列
    builder.Services.AddSingleton<RabbitMqProducer>();
#endregion

#region Redis配置
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetSection("Redis:Configuration").Value;
    return ConnectionMultiplexer.Connect(configuration!);
});
#endregion

#region 配置Consul
builder.Services.AddSingleton<IConsulClient, ConsulClient>(client =>
{
    return new ConsulClient(config =>
    {
        config.Address = new Uri(builder.Configuration.GetSection("Consul")["Address"]!); // Consul server地址
    });
});
#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var consulClient = app.Services.GetRequiredService<IConsulClient>();
var config = app.Services.GetService<IConfiguration>();
var consulServiceRegistration = new ConsulServiceRegistration(consulClient, config!);
await consulServiceRegistration.RegisterServiceAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
