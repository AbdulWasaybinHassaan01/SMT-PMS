using System.Data;
using Microsoft.Data.SqlClient;

namespace TechWorldPMS.Data
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
        Task<SqlConnection> CreateOpenConnectionAsync();
        string ConnectionString { get; }
    }
}

