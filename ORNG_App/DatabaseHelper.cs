using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORNG_App
{
    public static class DatabaseHelper
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["ORNG_DB"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = GetConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
            }

            return dataTable;
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = GetConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                rowsAffected = command.ExecuteNonQuery();
            }

            return rowsAffected;
        }

        public static byte[] ImageToByteArray(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            return File.ReadAllBytes(imagePath);
        }

        public static void SaveImageFromByteArray(byte[] imageData, string savePath)
        {
            if (imageData == null || imageData.Length == 0)
                return;

            File.WriteAllBytes(savePath, imageData);
        }
    }
}
