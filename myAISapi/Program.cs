//using myAISapi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using myAISapi.Data;
using myAISapi.Models;
using myAISapi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.OpenApi.Models;
using Cassandra;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext
builder.Services.AddDbContext<AppDBContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<CassandraSettings>(builder.Configuration.GetSection("Cassandra"));

builder.Services.AddSingleton<Cassandra.ISession>(sp =>
{
	var settings = sp.GetRequiredService<IOptions<CassandraSettings>>().Value;

	var clusterBuilder = Cluster.Builder()
		.AddContactPoints(settings.ContactPoints) // ví dụ: ["127.0.0.1"]
		.WithPort(settings.Port);

	if (!string.IsNullOrEmpty(settings.Username))
	{
		clusterBuilder = clusterBuilder.WithCredentials(settings.Username, settings.Password);
	}

	var cluster = clusterBuilder.Build();
	var session = cluster.Connect(settings.Keyspace); // "ais"
	return session;
});
builder.Services.AddSingleton<ICassandraHanhTrinhRepository, CassandraHanhTrinhRepository>();


// Add services to the container.

// Au then ti cây sừn
var PublicKeyPath = builder.Configuration["Jwt:PublicKeyPath"];
var publicKey = System.IO.File.ReadAllText(PublicKeyPath);
var rsa = RSA.Create();
rsa.ImportFromPem(publicKey);

builder.Services.AddAuthentication(options =>
{
	//defaut
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"], // Thay bằng issuer của bạn
		ValidateAudience = true,
		ValidAudience = builder.Configuration["Jwt:Audience"], // Thay bằng audience của bạn
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new RsaSecurityKey(rsa),
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

// dung ca 2 authen

builder.Services.AddAuthorization(options =>
{ 
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("Admin&Guest", policy => policy.RequireRole("Guest,Admin"));
	options.AddPolicy("PaidUserOnly", policy =>
		policy.RequireClaim("PlanType", "Pro"));
});

// Con trôn lơ
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
	options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
	//options.JsonSerializerOptions.Converters.Add(new NullConverter<object>());
	options.JsonSerializerOptions.WriteIndented = true; // Giúp JSON dễ đọc
});

builder.Services.AddSignalR();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle



builder.Services.AddSingleton<IUdpMessageStore, UdpMessageStore>();
builder.Services.AddSingleton<IDecodedAISStore, DecodedAISStore>();
builder.Services.AddSingleton<IDM_HanhTrinh_Store, DM_HanhTrinh_Store>();
builder.Services.AddSingleton<IDM_Tau_Store, DM_Tau_Store>();
builder.Services.AddSingleton<UdpListenerService>();
builder.Services.AddScoped<myAISapi.Services.IAlertService, myAISapi.Services.AlertService>();
builder.Services.AddScoped<BeaconDriftService>();


builder.Services.AddHostedService<UdpListenerService>(provider => provider.GetRequiredService<UdpListenerService>());
builder.Services.AddHostedService<AisDecoderHostedService>();
builder.Services.AddHostedService<AisDBService>();
builder.Services.AddHostedService<BeaconDriftHostedService>();

//builder.Services.AddCors(options =>
//{
//	options.AddPolicy("AllowFrontend", policy =>
//	{
//		policy
//			.WithOrigins(
//				"http://localhost:5173",
//				"http://localhost:3030"
//			)
//			.AllowAnyHeader()
//			.AllowAnyMethod()
//			.AllowCredentials(); // ⚠️ cần cho SignalR + cookie/token
//	});
//});

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowReactApp",
		policy =>
		{
			policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()) // Thay đổi URL nếu React chạy trên domain khác
				  .AllowAnyMethod()
				  .AllowAnyHeader()
				  .AllowCredentials();
		});
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your AIS API", Version = "v1" });

	// Thêm security definition cho JWT Bearer
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT"
	});

	// Thêm security requirement cho các endpoint yêu cầu xác thực
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
			new string[] {}
		}
	});
});

//builder.Services.AddScoped<ProcMaster>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers(); 

app.MapHub<myAISapi.Hubs.NotifyHub>("/notifyHub");

app.Run();
