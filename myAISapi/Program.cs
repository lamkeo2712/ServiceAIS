//using myAISapi.Services;
using Microsoft.EntityFrameworkCore;
using myAISapi.Data;
using myAISapi.Models;
using myAISapi.Services;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext
builder.Services.AddDbContext<AppDBContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

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
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
