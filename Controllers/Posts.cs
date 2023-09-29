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

/*      private const int MinCommentLen = 3;
        private const int MaxCommentLen = 3000;

        private const int PostsPerPage = 25; // Default: 25
        private const int PostPreviewTextLen = 450;

        private const int MaxSearchPhraseLen = 64;*/

        private readonly Func<MySqlDataReader, Post> PostMapFunction = reader =>
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

        public Posts(ILogger<Posts> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _sqlFactory = new SqlFactory(_connection, _logger);
        }

        // GET: /Posts - Get all post entries
        [HttpGet(Name = "Posts")]
        public async Task<IEnumerable<Post>?> GetAsync()
        {
            string query = $"SELECT * FROM {PostsTable};";
            return await _sqlFactory.Select(query, PostMapFunction);
        }

        // POST: /Posts - Create new post entry
        [HttpPost(Name = "Posts")]
        public async Task<bool> PostAsync(Post post)
        {
            if (PostIsValid(post))
            {
                FormatPost(ref post);
            }
            else
            {
                return false;
            }

            string query = 
                $"INSERT INTO {PostsTable} (`Title`, `Text`, `Author`, `Created`) " +
                $"VALUES ('{post.Title}', '{post.Text}', '{post.Author}', '{DateTimeOffset.UtcNow:yyyy.MM.dd HH:mm:ss}');";
            
            return await _sqlFactory.Insert(query);
        }

        // PUT: /Posts - Update selected post entry
        [HttpPut(Name = "Posts")]
        public async Task<bool> PutAsync(int PID, Post post)
        {
            if (PID == 0)
            {
                return false;
            }

            if (PostIsValid(post))
            {
                FormatPost(ref post);
            }
            else
            {
                return false;
            }

            string selectQuery = $"SELECT * FROM {PostsTable} WHERE (`PostID` = '{PID}')"; // Be careful: <``> marks COLUMN, <''> marks VALUE
            var oldPost = (await _sqlFactory.Select(selectQuery, PostMapFunction))?.First();

            if (oldPost == null)
            {
                return false;
            }

            string updateQuery = $"UPDATE {PostsTable} SET "; // Dynamically built
            bool wasAnythingChanged = false;

            if (post.Title != oldPost.Title)
            {
                updateQuery += $"`Title` = '{post.Title}'";
                wasAnythingChanged = true;
            }

            if (post.Text != oldPost.Text)
            {
                if (wasAnythingChanged)
                {
                    updateQuery += ", ";
                }
                updateQuery += $"`Text` = '{post.Text}'";
                wasAnythingChanged = true;
            }

            if (wasAnythingChanged)
            {
                // Complete updateQuery string and send DB request
                return await _sqlFactory.Update($"{updateQuery} WHERE (`PostID` = '{PID}')");
            }
            else
            {
                return false;
            }
        }

        // DELETE: /Posts - Delete selected post entry
        [HttpDelete(Name = "Posts")]
        public async Task<bool> DeleteAsync(int PID)
        {
            if (PID == 0) // Default int value if none provided
            {
                return false;
            }

            string query = $"DELETE FROM {PostsTable} WHERE (`PostID` = '{PID}')";

            return await _sqlFactory.Delete(query);
        }

        /// <summary>
        /// Validates primary Post fields (Title, Author)
        /// </summary>
        /// <returns>true - if valid, otherwise - false</returns>
        private static bool PostIsValid(in Post post) // 'in' [.net8+] is similar to 'ref', but it allows read-only access
        {
            return !string.IsNullOrWhiteSpace(post.Title) && !string.IsNullOrWhiteSpace(post.Author) && post.Title.Length > MinPostTitleLen;
        }

        /// <summary>
        /// Formats post fields if applicable
        /// </summary>
        private static void FormatPost(ref Post post) // 'ref' is a pointer to "post" variable with read-write access
        {
            if (post.Title.Length > MaxPostTitleLen)
            {
                post.Title = post.Title[0..MaxPostTitleLen];
            }

            if (!string.IsNullOrWhiteSpace(post.Text) && post.Text.Length > MaxPostTextLen)
            {
                post.Text = post.Text[0..MaxPostTextLen];
            }
        }
    }
}
