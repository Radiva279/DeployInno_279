using System;
using System.Data;
using System.Data.SqlClient;

namespace CRUDMahasiswaADO
{
    public class DAL
    {
        public static string GetLocalIPAddress()
        {
            string localIP = string.Empty;
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception)
            {
                localIP = "";
            }
            return localIP;
        }

        public static string GetConnectionString()
        {
            string ip = GetLocalIPAddress();

            if (!string.IsNullOrEmpty(ip))
            {
                try
                {
                    string testConnStr = $"Data Source={ip};Initial Catalog=DBAkademikADO;User ID=sa;Password=Radiva1211;Connect Timeout=2;";
                    using (SqlConnection conn = new SqlConnection(testConnStr))
                    {
                        conn.Open();
                        return testConnStr;
                    }
                }
                catch (Exception)
                {
                }
            }

            return "Data Source=RADIVA-12\\RADIVA;Initial Catalog=DBAkademikADO;User ID=sa;Password=Radiva1211;";
        }

        private static readonly string connectionString = GetConnectionString();

        public string GetConnectionStringInstance()
        {
            return connectionString;
        }

        public int CountMhs()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    cmd.ExecuteNonQuery();
                    return outputParam.Value != DBNull.Value ? Convert.ToInt32(outputParam.Value) : 0;
                }
            }
        }

        public DataTable GetMhs()
        {
            DataTable dtMahasiswa = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtMahasiswa);
                    }
                }
            }
            return dtMahasiswa;
        }

        public void InsertMhs(string nim, string nama, string alamat, string jeniskelamin, DateTime tanggallahir, string kodeProdi, byte[] foto)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("sp_InsertMahasiswa", conn, trans))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@PNIM", SqlDbType.Char, 11).Value = nim;
                        command.Parameters.Add("@pNama", SqlDbType.VarChar, 100).Value = nama;
                        command.Parameters.Add("@pAlamat", SqlDbType.VarChar, 200).Value = alamat;
                        command.Parameters.Add("@pJenisKelamin", SqlDbType.Char, 1).Value = jeniskelamin;
                        command.Parameters.Add("@pTanggalLahir", SqlDbType.DateTime).Value = tanggallahir;
                        command.Parameters.Add("@pKodeProdi", SqlDbType.Char, 4).Value = kodeProdi;

                        // DISELARASKAN: Menggunakan @pFoto kembali sesuai kebutuhan Stored Procedure database kamu
                        command.Parameters.Add("@pFoto", SqlDbType.VarBinary, -1).Value = (object)foto ?? DBNull.Value;

                        try
                        {
                            command.ExecuteNonQuery();
                            trans.Commit();
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        public void UpdateMhs(string nim, string nama, string alamat, string jeniskelamin, DateTime tanggallahir, string kodeProdi, byte[] foto)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("sp_UpdateMahasiswa", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@PNIM", SqlDbType.Char, 11).Value = nim;
                    command.Parameters.Add("@pNama", SqlDbType.VarChar, 100).Value = nama;
                    command.Parameters.Add("@pAlamat", SqlDbType.VarChar, 200).Value = alamat;
                    command.Parameters.Add("@pJenisKelamin", SqlDbType.Char, 1).Value = jeniskelamin;
                    command.Parameters.Add("@pTanggalLahir", SqlDbType.DateTime).Value = tanggallahir;
                    command.Parameters.Add("@pKodeProdi", SqlDbType.Char, 4).Value = kodeProdi;

                    // DISELARASKAN: Menggunakan @pFoto kembali agar sinkron dengan database saat menekan tombol Update
                    command.Parameters.Add("@pFoto", SqlDbType.VarBinary, -1).Value = (object)foto ?? DBNull.Value;

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteMhs(string nim)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NIM", SqlDbType.Char, 11).Value = nim;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void resetData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmdDelete = new SqlCommand("DELETE FROM Mahasiswa;", conn, trans))
                        {
                            cmdDelete.ExecuteNonQuery();
                        }

                        string insertQuery = @"
                            INSERT INTO Mahasiswa (NIM, Nama, JenisKelamin, TanggalLahir, Alamat, KodeProdi, TanggalDaftar)
                            SELECT NIM, Nama, JenisKelamin, TanggalLahir, Alamat, KodeProdi, TanggalDaftar FROM Mahasiswa_Backup;";

                        using (SqlCommand cmdInsert = new SqlCommand(insertQuery, conn, trans))
                        {
                            cmdInsert.ExecuteNonQuery();
                        }

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public void testInject(string nim)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Mahasiswa SET Nama = 'HACKED' WHERE NIM = '" + nim + "'";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetMhsByNIM(string nim)
        {
            DataTable dtMahasiswa = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswaByNIM", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@PNIM", SqlDbType.Char, 11).Value = nim;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtMahasiswa);
                    }
                }
            }
            return dtMahasiswa;
        }

        public void InsertLog(string message)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_LogMessage", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@psn", SqlDbType.VarChar, -1).Value = message;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable getProdi()
        {
            DataTable dtProdi = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT NamaProdi FROM ProgramStudi", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtProdi);
                    }
                }
            }
            return dtProdi;
        }

        public DataTable getDataRekap(string prodi, DateTime tanggalMasuk)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_Report", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@inProdi", SqlDbType.VarChar, 100).Value = prodi;
                    cmd.Parameters.Add("@inTglMsuk", SqlDbType.Char, 4).Value = tanggalMasuk.Year.ToString();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public DataTable getAllDataChart()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_DashBoard", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public DataTable getDataChartByTahun(DateTime thMasuk)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_DashBoardByTahun", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@inTglMsuk", SqlDbType.Char, 4).Value = thMasuk.Year.ToString();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
    }
}