using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    public partial class KonumEkle : Form
    {
        private float x;
        private float y;

        public KonumEkle(float x, float y)
        {
            InitializeComponent();
            this.x = x;
            this.y = y;
        }

        private void KonumEkle_Load(object sender, EventArgs e)
        {
            lblKoordinat.Text = $"Tıklanan Konum:\nX: {x}, Y: {y}\n\nCihaz eklemek için 'Cihaz Ekle' butonuna basın.";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            CihazEkle cihazEkleForm = new CihazEkle();
            cihazEkleForm.Show();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();

        }
    }

}
