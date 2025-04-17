using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Cihaz_Takip_Uygulaması
{
    public partial class CihazEkle : Form
    {
        public CihazEkle()
        {
            InitializeComponent();
        }

        private void CihazEkle_Load(object sender, EventArgs e)
        {
            CihazGrupListele();
        }

        private void CihazGrupListele()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Kod FROM CihazGrup", conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    comboBox1.Items.Clear();

                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader["Kod"].ToString());
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veri çekme hatası: " + ex.Message);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string secilenKod = comboBox1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(secilenKod))
            {
                MessageBox.Show("Seçilen Grup Kodu: " + secilenKod);
            }
        }
    }
}
