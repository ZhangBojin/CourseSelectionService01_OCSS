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

#region ���ݿ�������scopedע��
builder.Services.AddDbContext<CourseSelectionServiceOCSSDb>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
#endregion

#region ����Jwt
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //��Ĭ�ϵ������֤����ʧ��ʱ��Ӧ�ý�ʹ���������������ս
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

#region ������־��Ϣ����
    builder.Services.AddSingleton<RabbitMqProducer>();
#endregion

#region Redis����
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetSection("Redis:Configuration").Value;
    return ConnectionMultiplexer.Connect(configuration!);
});
#endregion

#region ����Consul
builder.Services.AddSingleton<IConsulClient, ConsulClient>(client =>
{
    return new ConsulClient(config =>
    {
        config.Address = new Uri(builder.Configuration.GetSection("Consul")["Address"]!); // Consul server��ַ
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
