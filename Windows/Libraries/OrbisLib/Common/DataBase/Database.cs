using System.Data.SQLite;

namespace OrbisSuite.Common.DataBase
{
    public static class Database
    {
        /// <summary>
        /// Gets the specified Option from the Settings.
        /// </summary>
        /// <typeparam name="T">The result data type.</typeparam>
        /// <param name="Name">The string name of the setting we would like.</param>
        /// <returns>returns the specified data type result of the setting.</returns>
        public static dynamic GetSetting<T>(string Name)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    $@"
                        SELECT {Name}
                        FROM Settings
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (typeof(T) == typeof(int))
                                return reader.GetInt32(0);
                            else if (typeof(T) == typeof(bool))
                                return reader.GetBoolean(0);
                            else if (typeof(T) == typeof(string))
                                return reader.GetString(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Sets the Option of the Settings.
        /// </summary>
        /// <param name="Name">The name of the Setting we would like to set.</param>
        /// <param name="Value">The value of the setting we would like to set.</param>
        public static void SetSetting(string Name, dynamic Value)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={Config.DataBasePath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText =
                    $@"
                        UPDATE Settings SET {Name}=@Value
                    ";
                    command.Parameters.AddWithValue("@Value", Value);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error: {ex.Message}");
            }
        }

    }
}
