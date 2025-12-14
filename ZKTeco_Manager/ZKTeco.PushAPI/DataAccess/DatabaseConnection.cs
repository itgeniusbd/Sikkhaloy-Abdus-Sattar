using System;
using System.Configuration;
using System.Data.SqlClient;

namespace ZKTeco.PushAPI.DataAccess
{
    /// <summary>
    /// Database connection manager
    /// </summary>
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"]?.ConnectionString 
                ?? ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string not found in Web.config");
            }
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
