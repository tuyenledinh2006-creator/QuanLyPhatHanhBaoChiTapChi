using System;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class KetNoi
    {
        public static string ConnectionString { get; set; } = "Server=.\\SQLEXPRESS01;Database=QuanLyPhatHanhBaoChi;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
