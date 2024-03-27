using IO_Link;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
/////
IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
{
    options.ServiceName = "Injection Molding Machine Service";
})
    .ConfigureServices((builder, services) =>
    {
		//var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		var config = builder.Configuration;
		services.Configure<IP_Model>(config.GetSection("IP_Address"));


		services.AddControllers();
		services.AddHostedService<MybackgroundService>();
		services.AddSingleton<ManagedMqtt>();
		services.AddSingleton<ManagedMqtt2>();
		services.AddCors(options =>
		{
			options.AddPolicy("AllowAll",
				builder =>
				{
					builder
					.WithOrigins("localhost", "http://localhost:5173")
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials();
				});
		});

		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();

	    

	})
	.Build();

await host.RunAsync();
#pragma warning restore CS8604 // Possible null reference argument./////
//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//var config = builder.Configuration;
//builder.Services.Configure<IP_Model>(config.GetSection("IP_Address"));


//builder.Services.AddControllers();
//builder.Services.AddHostedService<MybackgroundService>();
//builder.Services.AddSingleton<ManagedMqtt>();
//builder.Services.AddSingleton<ManagedMqtt2>();
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll",
//        builder =>
//        {
//            builder
//            .WithOrigins("localhost", "http://localhost:5173")
//            .AllowAnyMethod()
//            .AllowAnyHeader()
//            .AllowCredentials();
//        });
//});

//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();




//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.UseCors("AllowAll");

//app.MapControllers();

//app.Run();
