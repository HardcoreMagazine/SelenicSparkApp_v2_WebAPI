using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace SelenicSparkApp_v2_WebAPI.Utilities
{
    /// <summary>
    /// Allows to shorten SELECT, UPDATE, DELETE database requests by using generics
    /// </summary>
    public class SqlFactory
    {
        public readonly ILogger<ControllerBase> Logger;
        public readonly MySqlConnection Connection;

        public SqlFactory(MySqlConnection connection, ILogger<ControllerBase> logger)
        {
            Connection = connection;
            Logger = logger;
        }

        /// <summary>Selects the specified query.</summary>
        /// <typeparam name="T">Model</typeparam>
        /// <param name="query">SQL 'SELECT' query string</param>
        /// <param name="mapFunction">Mapping function for Model</param>
        /// <returns>List of Model elements<br /></returns>
        /// <example>
        ///   <code title="Example usage">
        ///     string sqlQuery = "SELECT * FROM my_table;";
        ///     Func&lt;MySqlDataReader, MyModel&gt; mapFunction = reader =&gt;
        ///     {
        ///         return new MyModel { /* map properties based on data */ };
        ///     {
        ///     var result = await Select(sqlQuery, mapFunction);
        ///   </code>
        /// </example>
        public async Task<List<T>?> Select<T>(string query, Func<MySqlDataReader, T> mapFunction)
        {
            if (!query.Contains("SELECT")) // Dummy failsafe
            {
                return null;
            }
            try
            {
                await Connection.OpenAsync();
                var command = new MySqlCommand(query, Connection);
                var reader = await command.ExecuteReaderAsync();
                
                var results = new List<T>();
                
                while (await reader.ReadAsync())
                {
                    var result = mapFunction(reader);
                    results.Add(result);
                }

                return results;
            }
            catch
            {
                Logger.LogInformation("");
                return null;
            }
            finally
            { 
                await Connection.CloseAsync();
            }
        }

        public async Task<bool> Update(string query)
        {
            if (!query.Contains("UPDATE"))
            {
                return false;
            }
            try
            {
                await Connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            { 
                await Connection.CloseAsync();
            }
        }

        public async Task<bool> Delete(string query)
        {
            if (!query.Contains("DELETE")) // Dummy failsafe
            {
                return false;
            }
            try
            {
                await Connection.OpenAsync();
                // TODO
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                await Connection.CloseAsync();
            }
        }
    }
}
