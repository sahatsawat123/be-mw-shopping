using System.Data;
using Dapper;
using Npgsql;

namespace Shopping.Interface
{
    class CategoryDapperController
    {
        private readonly IConfiguration _config;

        public CategoryDapperController(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }

        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
        }

        public bool ExecuteSQL(string sql)
        {
            IDbConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql) > 0;
        }
        public int ExecuteSQLRowCount(string sql)
        {
            IDbConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }
    }  
}