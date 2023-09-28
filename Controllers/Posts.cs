using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SelenicSparkApp_v2_WebAPI.Models;
using SelenicSparkApp_v2_WebAPI.Utilities;
using System.IO.Pipelines;

namespace SelenicSparkApp_v2_WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Posts : ControllerBase
    {
        private readonly ILogger<Posts> _logger;
        private readonly MySqlConnection _connection;
        private SqlFactory _sqlFactory;

        public Posts(ILogger<Posts> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _sqlFactory = new SqlFactory(_connection, _logger);
        }

        [HttpGet(Name = "Posts")]
        public async Task<IEnumerable<Post>?> GetAsync()
        {
            string query = "SELECT * FROM selenicspark.posts;";
            Func<MySqlDataReader, Post> mapFunction = reader =>
            {
                return new Post
                {
                    // TODO: fix this mess
                    // Exception thrown: 'System.InvalidCastException' in System.Private.CoreLib.dll
                    PostID = (int)reader.GetValue(0),
                    Title = (string)reader.GetValue(1),
                    Text = (string)reader.GetValue(2),
                    Author = (string)reader.GetValue(3),
                    Created = DateTimeOffset.Parse((string)reader.GetValue(4))
                };
            };
            return await _sqlFactory.Select(query, mapFunction);
        }
    }
}
