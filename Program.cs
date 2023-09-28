using MySqlConnector;

namespace SelenicSparkApp_v2_WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DB instance created for every request
            builder.Services.AddTransient(_ => new MySqlConnection(builder.Configuration.GetConnectionString("MySQL"))); 
            
            // DB instance created once per request and reused afterwards
            //builder.Services.AddScoped(_ => new MySqlConnection(builder.Configuration.GetConnectionString("MySQL")));
            
            // DB instance created for all requests and reused afterwards
            //builder.Services.AddSingleton(_ => new MySqlConnection(builder.Configuration.GetConnectionString("MySQL")));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
