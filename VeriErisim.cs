using System;
using System.Data;
using System.Data.SqlClient;

namespace Cihaz_Takip_Uygulaması
{
    class VeriErisim
    {
        public static DataTable VerileriGetir()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
                {
                    conn.Open();
                    string query = "SELECT * FROM Cihaz";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veriler yüklenirken hata oluştu: " + ex.Message);
            }
            return dt;
        }
    }
}
