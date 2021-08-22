using Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTLWin
{
    public partial class TaiKhoan_Form : Form
    {
        string username, password, id;
        int accountType;
        bool save = true;
        public TaiKhoan_Form()
        {
            InitializeComponent();
        }

        public TaiKhoan_Form(string username, string password, string id, int accountType)
        {
            InitializeComponent();
            this.username = username;
            this.password = password;
            this.id = id;
            this.accountType = accountType;
        }

        //Set thuộc tính enable của các textBox, dateTimePicker, combo box = true hoặc false
        private void edit(bool e)
        {
            txtHoTen.Enabled = e;
            dateNgaySinh.Enabled = e;
            cbGioiTinh.Enabled = e;
            txtDiaChi.Enabled = e;
            txtDienThoai.Enabled = e;
            txtMatKhau.Enabled = e;
            btn_Luu.Visible = e;
            btn_Huy.Visible = e;
            btn_Update.Visible = !e;
        }

        private void LoadData()
        {
            try
            {
                if(accountType == 0)
                {
                    dataGridView1.DataSource = new Database().SelectData("EXEC TimKiemMonHoc_TheoMaGV '" + username + "'");
                    DataTable dt = new Database().SelectData("EXEC TimKiem_GIangVien '" + id + "'");
                    dateNgaySinh.MaxDate = DateTime.Now;
                    dateNgaySinh.Value = DateTime.Parse(dt.Rows[0][2].ToString());
                    txtMaGV.Text = dt.Rows[0][0].ToString();
                    txtHoTen.Text = dt.Rows[0][1].ToString();
                    txtDiaChi.Text = dt.Rows[0][4].ToString();
                    txtDienThoai.Text = dt.Rows[0][5].ToString();
                    txtDangNhap.Text = username;
                    cbGioiTinh.Text = dt.Rows[0][3].ToString();
                    DataTable dt2 = new Database().SelectData("SELECT * FROM TAIKHOANGV WHERE TaiKhoan = '" + username + "'");
                    txtMatKhau.Text = dt2.Rows[0][1].ToString();
                }
                else if(accountType == 1)
                {
                    DataTable dt = new Database().SelectData("EXEC TimKiem_SinhVien '" + id + "'");
                    dateNgaySinh.MaxDate = DateTime.Now;
                    dateNgaySinh.Value = DateTime.Parse(dt.Rows[0][2].ToString());
                    txtMaGV.Text = dt.Rows[0][0].ToString();
                    txtHoTen.Text = dt.Rows[0][1].ToString();
                    txtDiaChi.Text = dt.Rows[0][4].ToString();
                    txtDienThoai.Text = dt.Rows[0][5].ToString();
                    cbGioiTinh.Text = dt.Rows[0][3].ToString();
                    txtMaLop.Text = dt.Rows[0][6].ToString();
                    txtDangNhap.Text = username;
                    DataTable dt2 = new Database().SelectData("SELECT * FROM TAIKHOANSV WHERE TaiKhoan = '" + username + "'");
                    txtMatKhau.Text = dt2.Rows[0][1].ToString();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void btn_Update_Click(object sender, EventArgs e)
        {
            edit(true);
            save = false;
        }

        private void btn_Luu_Click(object sender, EventArgs e)
        {
            edit(false);
            save = true;
            luuThongTin();

        }

        private void luuThongTin()
        {
            if (txtHoTen.Text == "")
            {
                textValidating(txtHoTen, new CancelEventArgs(), errorProvider1);
            }
            else if (txtDiaChi.Text == "")
            {
                textValidating(txtDiaChi, new CancelEventArgs(), errorProvider2);
            }
            else if (txtDienThoai.Text == "")
            {
                textValidating(txtDienThoai, new CancelEventArgs(), errorProvider3);
            }
            else if (txtMatKhau.Text == "")
            {
                textValidating(txtMatKhau, new CancelEventArgs(), errorProvider4);
            }
            else
            {
                string date = dateNgaySinh.Value.ToString("yyyy/MM/dd");
                string gioitinh = cbGioiTinh.Text;
                if(accountType == 0)
                {
                    int rowEffected = new Database().ExecCmd("EXEC Update_GiangVien '" + txtMaGV.Text + "', N'" + txtHoTen.Text + "', '"
                    + date + "', N'" + gioitinh + "', N'" + txtDiaChi.Text + "', '" + txtDienThoai.Text + "', '" + txtMatKhau.Text + "'");

                    if (rowEffected != 0)
                    {
                        MessageBox.Show("Cập nhập thành công", "Thông báo");
                    }
                    else
                    {
                        MessageBox.Show("Cập nhập thất bại", "Thông báo");
                    }
                }
                else if(accountType == 1)
                {
                    int rowEffected = new Database().ExecCmd("EXEC Update_SinhVien '" + txtMaGV.Text + "', N'" + txtHoTen.Text + "', '"
                    + date + "', N'" + gioitinh + "', N'" + txtDiaChi.Text + "', '" + txtDienThoai.Text + "', '" + txtMatKhau.Text + "'");

                    if (rowEffected != 0)
                    {
                        MessageBox.Show("Cập nhập thành công", "Thông báo");
                    }
                    else
                    {
                        MessageBox.Show("Cập nhập thất bại", "Thông báo");
                    }
                }
            }
            LoadData();
        }

        private void btn_Huy_Click(object sender, EventArgs e)
        {
            edit(false);
            save = true;
            LoadData();
        }

        private void TaiKhoan_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!save)
            {
                DialogResult result = MessageBox.Show("Thông tin bạn vừa cập nhập chưa được lưu. \nBạn có muốn lưu chúng không ?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {//Cập nhập thông tin
                    luuThongTin();
                }
            }
        }

        private void TaiKhoan_Form_Load(object sender, EventArgs e)
        {
            //Mặc định các textBox, dateTimePicker, comboBox chỉ để xem
            dateNgaySinh.MaxDate = DateTime.Now;
            if(accountType== 1)
            {
                lblMaLop.Visible = true;
                txtMaLop.Visible = true;
                panMaLop.Visible = true;
                dataGridView1.Visible = false;
                lblMonHoc.Visible = false;
            }
            
            LoadData();
        }

        private void textValidating(object sender, CancelEventArgs e, ErrorProvider error)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "")
            {
                error.SetError(textBox, "Không được bỏ trống phần này");
                textBox.Focus();
            }
            else
            {
                error.SetError(textBox, "");
                e.Cancel = false;
            }
        }

        private void txtHoTen_Validating(object sender, CancelEventArgs e)
        {
            textValidating(sender, e, errorProvider1);
        }

        private void txtDiaChi_Validating(object sender, CancelEventArgs e)
        {
            textValidating(sender, e, errorProvider2);
        }

        private void txtDienThoai_Validating(object sender, CancelEventArgs e)
        {
            textValidating(sender, e, errorProvider3);
        }

        private void txtMatKhau_Validating(object sender, CancelEventArgs e)
        {
            textValidating(sender, e, errorProvider4);
        }

       
    }
}
