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

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext
builder.Services.AddDbContext<AppDBContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

// Au then ti cây sừn
var publicKey = System.IO.File.ReadAllText("./Keys/public.key");
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
		ValidIssuer = "http://localhost:5013", // Thay bằng issuer của bạn
		ValidateAudience = true,
		ValidAudience = "http://localhost:3030", // Thay bằng audience của bạn
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new RsaSecurityKey(rsa),
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

// dung ca 2 authen

builder.Services.AddAuthorization(options =>
{ 
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator")); 
	options.AddPolicy("GuestOnly", policy => policy.RequireRole("Guest")); 
});

// Con trôn lơ
builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
	options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
	//options.JsonSerializerOptions.Converters.Add(new NullConverter<object>());
	options.JsonSerializerOptions.WriteIndented = true; // Giúp JSON dễ đọc
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

builder.Services.AddSingleton<IUdpMessageStore, UdpMessageStore>();
builder.Services.AddSingleton<IDecodedAISStore, DecodedAISStore>();
builder.Services.AddSingleton<IDM_HanhTrinh_Store, DM_HanhTrinh_Store>();
builder.Services.AddSingleton<IDM_Tau_HS_Store, DM_Tau_HS_Store>();
builder.Services.AddSingleton<IDM_Tau_Store, DM_Tau_Store>();


builder.Services.AddHostedService<UdpListenerService>();
builder.Services.AddHostedService<AisDecoderHostedService>();
builder.Services.AddHostedService<AisDBService>();
//builder.Services.AddSingleton<AisDecoderService>();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowReactApp",
		policy =>
		{
			policy.WithOrigins("http://localhost:3030") // Thay đổi URL nếu React chạy trên domain khác
				  .AllowAnyMethod()
				  .AllowAnyHeader();
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

app.Run();
