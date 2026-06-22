using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form2 : Form
    {
        // === 11. Tambahakan Code Berikut Kedalam Class Form Cetak Data ===
        static string connectionString = "Data Source=RADIVA-12\\RADIVA;Initial Catalog=DBAkademikADO;User ID=sa;Password=Radiva1211";
        SqlConnection conn = new SqlConnection(connectionString);
        SqlDataAdapter da;
        DataTable dtMahasiswa;

        CrystalReport1 listMahasiswa = new CrystalReport1();

        // Diperbaiki dari get; get; menjadi get; set; agar tidak eror
        string prodi { get; set; }
        DateTime tglmasuk { get; set; }

        // === 12. Ubah Constrektor Form ===
        public Form2(string Prodi, DateTime TglMasuk)
        {
            // === 13. Tambahkan Code Berikut Kedalam Constrektor ===
            InitializeComponent();

            prodi = Prodi;
            tglmasuk = TglMasuk;

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inProdi", prodi);
                cmd.Parameters.AddWithValue("@inTglMsuk", tglmasuk.Year);

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                conn.Close();

                listMahasiswa.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = listMahasiswa;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {

        }
    }
}