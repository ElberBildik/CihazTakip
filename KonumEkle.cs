using System;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    public partial class KonumEkle : Form
    {
        public float x;
        public float y;

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
            Form frm = new Harita();
            frm.Close();
            // Koordinatları CihazEklemeEkrani'ye gönder
            CihazEklemeEkrani cihazEkleForm = new CihazEklemeEkrani(x, y);
            cihazEkleForm.Show();
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}