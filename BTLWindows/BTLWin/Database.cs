using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data
{
    class Database
    {
        // Chuỗi link tới CSDL
        private const string StringConnection = @"Data Source=DESKTOP-FIP7SN7\SQLEXPRESS;Initial Catalog=qldsv;Integrated Security=True";
        private SqlConnection conn;
        private SqlCommand cmd;
        private DataTable dt;
        
        public Database()
        {
            //Nếu không link tới được thì thông báo lỗi
            conn = new SqlConnection(StringConnection);
            if(conn == null)
            {
               MessageBox.Show("Không thể kết nối được với SQLServer", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            conn.Close();
        }

        //Thực hiện cây lệnh SelectData đầu vào là câu lệnh truy vấn, trả về 1 bảng dữ liệu
        public DataTable SelectData(string query)
        {
            try
            {
                conn.Open();
                cmd = new SqlCommand(query, conn);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi : " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                conn.Close();
            }
        }

        //Thực hiện câu lệnh truy vấn , không trả về giá trị
        public int ExecCmd(string query)
        {          
            try
            {
                int rowEffected;
                conn.Open();
                cmd = new SqlCommand(query, conn);
                rowEffected = cmd.ExecuteNonQuery();
                return rowEffected;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi : " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
            finally
            {
                conn.Close();
            }
        }

    }
}
