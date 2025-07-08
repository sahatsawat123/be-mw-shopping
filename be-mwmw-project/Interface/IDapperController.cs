public interface IDapperController
{
    IEnumerable<T> LoadData<T>(string sql);

    T LoadDataSingle<T>(string sql);

    bool ExecuteSQL(string sql);

    int ExecuteSQLRowCount(string sql);
}