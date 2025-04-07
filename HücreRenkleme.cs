using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    public static class HücreRenkleme
    {
        public static void DurumRenklendir(DataGridView grid)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                DurumRenklendir(grid, row.Index);
            }
        }

        public static void DurumRenklendir(DataGridView grid, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= grid.Rows.Count)
                return;

            DataGridViewRow row = grid.Rows[rowIndex];

            if (row.Cells["Durum"] != null && row.Cells["Durum"].Value != null)
            {
                string durum = row.Cells["Durum"].Value.ToString();
                Color backColor;
                Color foreColor;

                if (durum.Contains("Down"))
                {
                    backColor = Color.Red;
                    foreColor = Color.White;
                }
                else if (durum.Contains("UP"))
                {
                    backColor = Color.Green;
                    foreColor = Color.White;
                }
                else
                {
                    backColor = Color.Yellow;
                    foreColor = Color.Black;
                }

                // Tüm satırın hücrelerini döngüyle gezerek renklendirme yapıyoruz
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = backColor;
                    cell.Style.ForeColor = foreColor;
                }
            }
        }
    }
}
