using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SelenicSparkApp_v2_WebAPI.Models;
using SelenicSparkApp_v2_WebAPI.Utilities;

namespace SelenicSparkApp_v2_WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Posts : ControllerBase
    {
        private readonly ILogger<Posts> _logger;
        private readonly MySqlConnection _connection;
        private SqlFactory _sqlFactory;

        private const string PostsTable = "selenicspark.posts";

        private const int MinPostTitleLen = 4;
        private const int MaxPostTitleLen = 300;
        private const int MaxPostTextLen = 20_000;

/*
        private const int MinCommentLen = 3;
        private const int MaxCommentLen = 3000;

        private const int PostsPerPage = 25; // Default: 25
        private const int PostPreviewTextLen = 450;

        private const int MaxSearchPhraseLen = 64;
*/

        public Posts(ILogger<Posts> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _sqlFactory = new SqlFactory(_connection, _logger);
        }

        [HttpGet(Name = "Posts")]
        public async Task<IEnumerable<Post>?> GetAsync()
        {
            string query = $"SELECT * FROM {PostsTable};";
            Func<MySqlDataReader, Post> mapFunction = reader =>
            {
                return new Post
                {
                    PostID = (int)reader.GetValue(0),
                    Title = (string)reader.GetValue(1),
                    Text = reader.GetValue(2).ToString(),
                    Author = (string)reader.GetValue(3),
                    Created = (DateTime)reader.GetValue(4)
                };
            };
            return await _sqlFactory.Select(query, mapFunction);
        }

        [HttpPost(Name = "Posts")]
        public async Task<bool> PostAsync(Post post)
        {
            if (string.IsNullOrWhiteSpace(post.Title) || string.IsNullOrWhiteSpace(post.Author))
            {
                return false;
            }

            if (post.Title.Length < MinPostTitleLen)
            {
                return false;
            }

            if (post.Title.Length > MaxPostTitleLen)
            {
                post.Title = post.Title[0..MaxPostTitleLen];
            }

            if (!string.IsNullOrWhiteSpace(post.Text) && post.Text.Length > MaxPostTextLen)
            {
                post.Text = post.Text[0..MaxPostTextLen];
            }

            string query = 
                $"INSERT INTO {PostsTable} (`Title`, `Text`, `Author`, `Created`) " +
                $"VALUES ('{post.Title}', '{post.Text}', '{post.Author}', '{DateTimeOffset.UtcNow:yyyy.MM.dd HH:mm:ss}');";
            
            return await _sqlFactory.Insert(query);
        }
    }
}
