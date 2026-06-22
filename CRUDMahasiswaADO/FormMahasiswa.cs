using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ExcelDataReader;

namespace CRUDMahasiswaADO
{
    public partial class FormMahasiswa : Form
    {
        DAL dbLogic = new DAL();
        BindingSource bindingSource1 = new BindingSource();

        public FormMahasiswa()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbJK.DataSource = new string[] { "L", "P" };

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            LoadData();
        }

        private void HitungTotal()
        {
            try
            {
                int total = dbLogic.CountMhs();
                lblTotal.Text = "Total Mahasiswa: " + total;
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Gagal load data total: " + ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                bindingSource1.DataSource = dbLogic.GetMhs();
                dataGridView1.DataSource = bindingSource1;

                // Memperbaiki visual kolom foto agar tanda X merah hilang
                if (dataGridView1.Columns.Contains("Foto") && dataGridView1.Columns["Foto"] is DataGridViewImageColumn fotoColumn)
                {
                    fotoColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                    fotoColumn.DefaultCellStyle.NullValue = new Bitmap(1, 1); // Mengganti X merah dengan gambar transparan kosong
                }

                HitungTotal();

                // Mengembalikan semua kondisi tombol ke keadaan normal database
                dataGridView1.Enabled = true;
                btnImporttD.Enabled = false;
                btnInsert.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnCari.Enabled = true;
                btnLoad.Enabled = true;
                btnResetData.Enabled = true;
                btnTestInjection.Enabled = true;
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            textNIM.Enabled = true;
            textNIM.Clear();
            textNama.Clear();
            cmbJK.SelectedIndex = -1;
            textAlamat.Clear();
            textKodeProdi.Clear();
            dptTanggalLahir.Value = DateTime.Now;
            pictureBox1.Image = null;
            pictureBox1.ImageLocation = null; // Reset path lokasi gambar
            textNIM.Focus();
        }

        public void simpanLog(string message)
        {
            dbLogic.InsertLog(message);
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] imgBytes = null;

                // 1. Cek dari ImageLocation (File fisik di laptop)
                if (!string.IsNullOrEmpty(pictureBox1.ImageLocation) && File.Exists(pictureBox1.ImageLocation))
                {
                    imgBytes = File.ReadAllBytes(pictureBox1.ImageLocation);
                }
                // 2. Cek dari Object Memory PictureBox (jika gambar hasil salinan/bukan openfile)
                else if (pictureBox1.Image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pictureBox1.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        imgBytes = ms.ToArray();
                    }
                }

                dbLogic.InsertMhs(textNIM.Text, textNama.Text, textAlamat.Text, cmbJK.Text, dptTanggalLahir.Value.Date, textKodeProdi.Text, imgBytes);
                MessageBox.Show("Data mahasiswa berhasil ditambahkan", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                simpanLog("Rollback Insert : " + ex.Message);
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog("General Error: " + ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] imgBytes = null;

                // 1. Cek dari ImageLocation (File fisik di laptop)
                if (!string.IsNullOrEmpty(pictureBox1.ImageLocation) && File.Exists(pictureBox1.ImageLocation))
                {
                    imgBytes = File.ReadAllBytes(pictureBox1.ImageLocation);
                }
                // 2. Cek dari Object Memory PictureBox (Gambar bawaan grid view saat cell klik)
                else if (pictureBox1.Image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pictureBox1.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        imgBytes = ms.ToArray();
                    }
                }

                // Mengeksekusi update data ke database beserta biner fotonya
                dbLogic.UpdateMhs(textNIM.Text, textNama.Text, textAlamat.Text, cmbJK.Text, dptTanggalLahir.Value.Date, textKodeProdi.Text, imgBytes);

                MessageBox.Show("Data mahasiswa berhasil diubah", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dg = MessageBox.Show(
                    "Yakin ingin menghapus data?",
                    "Konfirmasi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dg == DialogResult.Yes)
                {
                    dbLogic.DeleteMhs(textNIM.Text);
                    MessageBox.Show("Data mahasiswa berhasil dihapus", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadData();
                }
            }
            catch (SqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnUploadG_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.ImageLocation = ofd.FileName; // Mengisi path lokasi agar dibaca File.ReadAllBytes
                    pictureBox1.Image = Image.FromFile(ofd.FileName);
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            LoadData();
            MessageBox.Show("Koneksi ke Database Berhasil Dibuka!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnImportFE_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Excel Workbook|*.xlsx" })
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (x) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                            });

                            DataTable dt = result.Tables[0];
                            dataGridView1.DataSource = dt;

                            dataGridView1.Enabled = true;

                            // Mencegah tanda X merah muncul saat menayangkan preview baris Excel
                            if (dataGridView1.Columns.Contains("Foto") && dataGridView1.Columns["Foto"] is DataGridViewImageColumn fotoColumn)
                            {
                                fotoColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                                fotoColumn.DefaultCellStyle.NullValue = new Bitmap(1, 1);
                            }

                            btnImporttD.Enabled = true;
                            btnInsert.Enabled = false;
                            btnUpdate.Enabled = false;
                            btnDelete.Enabled = false;
                            btnCari.Enabled = false;
                            btnLoad.Enabled = false;
                            btnResetData.Enabled = false;
                            btnTestInjection.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengimpor file Excel: " + ex.Message);
            }
        }

        private void btnImporttD_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dataGridView1.DataSource;
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimport.");
                    return;
                }

                int sukses = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string nim = row["NIM"].ToString().Trim();
                    string nama = row["Nama"].ToString().Trim();
                    string jk = row["JenisKelamin"].ToString().Trim();
                    string alamat = row["Alamat"].ToString().Trim();
                    string namaProdi = row["NamaProdi"].ToString().Trim();

                    string kodeProdi = "TI01";
                    if (namaProdi.Contains("Sistem") || namaProdi.Contains("SI"))
                        kodeProdi = "SI01";
                    else if (namaProdi.Contains("Manajemen") || namaProdi.Contains("MI"))
                        kodeProdi = "MI01";

                    string fotoPath = row.Table.Columns.Contains("FotoPath") ? row["FotoPath"].ToString().Trim() : string.Empty;

                    if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama)) continue;

                    DateTime tglLahir;
                    if (!DateTime.TryParse(row["TanggalLahir"].ToString(), out tglLahir))
                    {
                        tglLahir = new DateTime(2003, 1, 1);
                    }

                    byte[] fotoBytes = null;
                    if (!string.IsNullOrWhiteSpace(fotoPath) && File.Exists(fotoPath))
                    {
                        fotoBytes = File.ReadAllBytes(fotoPath);
                    }

                    dbLogic.InsertMhs(nim, nama, alamat, jk, tglLahir.Date, kodeProdi, fotoBytes);
                    sukses++;
                }

                MessageBox.Show($"{sukses} Data mahasiswa berhasil ditambahkan dari Excel", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                simpanLog("Rollback Import : " + ex.Message);
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog("General Error: " + ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        private void btnRekap_Click(object sender, EventArgs e)
        {
            Form3 fm3 = new Form3();
            fm3.Show();
            this.Hide();
        }

        private void btnLoad_Click(object sender, EventArgs e) { LoadData(); }
        private void btnRefresh_Click(object sender, EventArgs e) { LoadData(); }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.resetData();
                MessageBox.Show("Data berhasil direset", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (SqlException ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.testInject(textNIM.Text);
                LoadData();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("safe"))
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("SQL Error : Unsafe UPDATE operation not allowed", "Security Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    simpanLog(ex.Message);
                    MessageBox.Show("SQL Error : " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                simpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

    
}