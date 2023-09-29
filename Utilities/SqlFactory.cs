﻿using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data.Common;

namespace SelenicSparkApp_v2_WebAPI.Utilities
{
    /// <summary>Allows to shorten CRUD actions with database using generics and abstraction</summary>
    public class SqlFactory
    {
        public readonly ILogger<ControllerBase> Logger;
        public readonly MySqlConnection Connection;

        /// <summary>Allows to shorten CRUD actions with database using generics and abstraction</summary>
        /// <param name="connection">Database connection instance</param>
        /// <param name="logger">Logger, native to controller making database request</param>
        public SqlFactory(MySqlConnection connection, ILogger<ControllerBase> logger)
        {
            Connection = connection;
            Logger = logger;
        }

        public async Task<bool> Insert(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || !query.Contains("INSERT")) // Dummy failsafe
            {
                return false;
            }
            try
            {
                await Connection.OpenAsync();
                var command = new MySqlCommand(query, Connection);
                var reader = await command.ExecuteReaderAsync();
                if (reader.RecordsAffected > 0)
                {
                    Logger.LogInformation($"Created new database entry, q='{query}'");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (DbException exc)
            {
                Logger.LogError($"Database error has occured: '{exc.Message}'");
                return false;
            }
            catch (Exception exc)
            {
                Logger.LogWarning($"Unknown error has occured: '{exc.Message}'");
                return false;
            }
            finally
            {
                await Connection.CloseAsync();
            }
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
            if (string.IsNullOrWhiteSpace(query) || !query.Contains("SELECT")) // Dummy failsafe
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
            catch (DbException exc)
            {
                Logger.LogError($"Database error has occured: '{exc.Message}'");
                return null;
            }
            catch (Exception exc)
            {
                Logger.LogWarning($"Unknown error has occured: '{exc.Message}'");
                return null;
            }
            finally
            { 
                await Connection.CloseAsync();
            }
        }

        public async Task<bool> Update(string query) // TODO
        {
            if (string.IsNullOrWhiteSpace(query) || !query.Contains("UPDATE"))
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

        public async Task<bool> Delete(string query) // TODO
        {
            if (string.IsNullOrWhiteSpace(query) || !query.Contains("DELETE")) // Dummy failsafe
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
    }
}
