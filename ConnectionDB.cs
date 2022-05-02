using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace MedHelper
{
    class ConnectionDB
    {
        static string startupPath = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName;
        SQLiteConnection con = new SQLiteConnection($"Data Source={startupPath}\\MedHelperSQLite.db; Version=3;");

        public void SqliteReader(string command, DataTable dt)
        {
            try
            {
                dt.Clear();
                con.Open();

                using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command, con))
                {
                    dataAdapter.Fill(dt);
                }
                con.Close();
            }
            catch
            {
                MessageBox.Show("Не удалось задействовать SQLite.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SqliteModification(string sql)
        {
            try
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, con);
                cmd.ExecuteReader();
                con.Close();
            }
            catch
            {
                MessageBox.Show("Не удалось задействовать SQLite.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
