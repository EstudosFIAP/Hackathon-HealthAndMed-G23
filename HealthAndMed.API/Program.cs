using Prometheus;
using HealthAndMed.API.Repository;
using HealthAndMed.API.Repository.Interfaces;
using HealthAndMed.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using HealthAndMed.API.Authentication;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços de controllers e endpoints
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Insira suas credenciais no formato 'Username:Password'. O Username pode ser o e-mail, CPF ou o número CRM, dependendo do tipo de conta."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});


var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DbHmContext>(options =>
    options.UseSqlServer(connection));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorsScheduleRepository, DoctorsScheduleRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();


builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//app.UseSwagger();
//app.UseSwaggerUI();
//}
//else
//{
//    app.UseHttpsRedirection();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpMetrics();
app.UseMetricServer();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
