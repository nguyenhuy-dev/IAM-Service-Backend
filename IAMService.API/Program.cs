using FluentValidation;
using IAMService.API.Middleware;
using IAMService.Application;
using IAMService.Application.Behaviors;
using IAMService.Application.Interfaces;
using IAMService.Application.Mappings;
using IAMService.Infrastructure.Repositories;
using IAMService.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using IAMService.Infrastructure.Settings;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<IAMServiceDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoleCloneService, RoleCloneService>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IAssemblyReference).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddProblemDetails(); // Optional
builder.Services.AddValidatorsFromAssembly(typeof(IAssemblyReference).Assembly);
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173") // FE chạy ở đây
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapScalarApiReference();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();

await app.MigrateDbContextAsync<IAMServiceDbContext>();

app.Run();
