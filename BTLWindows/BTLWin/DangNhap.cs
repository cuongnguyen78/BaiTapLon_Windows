using Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTLWin
{
    public partial class DangNhap : Form
    {
        /*
         * Biến mov lưu trạng thái của chuột
         * mov = true khi mousedown (ấn chuột xuống)
         * mov = false khi mouseup (thả chuột lên)
         * movX, movY dùng để lưu tọa độ chuột
         * Dùng cho việc di chuyển form đăng nhập
         */
        bool mov;
        int movX, movY;
        public DangNhap()
        {
            InitializeComponent();
        }

        //btnExit và btnMinimized dùng để thoát, thu xuống thanh taskbar
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimized_Click(object sender, EventArgs e)
        {
            //Ẩn form xuống thanh taskbar
            this.WindowState = FormWindowState.Minimized;
        }

        /*
         * Hàm panel1_MouseDown, Move, Up dùng để thực hiện việc di chuyển form đăng nhập
         * MouseDown thực hiện lưu tọa độ của con chuột
         * MouseMove thực hiện di chuyển form dựa theo tọa độ con chuột, thực hiện khi mousedown
         * MouseUp kết thúc quá trình, đưa mov = false
         */
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            //Khi ấn chuột xuống lấy tọa độ của chuột so với form
            movX = e.X;
            movY = e.Y;
            mov = true;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mov)// Nêu chuột còn đang được ấn xuống và di chuyển
            {
                this.SetDesktopLocation(MousePosition.X - movX, MousePosition.Y - movY);
                //MousePosition là vị trí của chuột so với màn hình 
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            //Nhả chuột ra
            mov = false;
        }

        /*
         * Hàm DangNhap_Load
         * Mặc định combobox là Giảng viên
         */
        private void DangNhap_Load(object sender, EventArgs e)
        {
            comboBox1.Text = "Giảng viên";
            if (chkHienThiMK.Checked)
            {
                txtMatKhau.UseSystemPasswordChar = false;
            }
            lblSaiTKorMK.Visible = false;//Ẩn thông báo sai mk hoặc tên đăng nhập
        }

        /*
         * Hàm txtDangNhap_Validating thực hiện khi đang edit text
         * Đảm bảo việc không được để trống
         */
        private void txtDangNhap_Validating(object sender, CancelEventArgs e)
        {
            if (lblSaiTKorMK.Visible == true)
            {
                lblSaiTKorMK.Visible = false;
            }
            if (txtDangNhap.Text == "")
            {
                errorProvider1.SetError(txtDangNhap, "Tên đăng nhập đang bị bỏ trống");
                txtDangNhap.Focus();
            }
            else
            {
                errorProvider1.SetError(txtDangNhap, "");
                e.Cancel = false;
            }
        }

        //Tương tự như txtDangNhap
        private void txtMatKhau_Validating(object sender, CancelEventArgs e)
        {
            if (lblSaiTKorMK.Visible == true)
            {
                lblSaiTKorMK.Visible = false;
            }
            if (txtMatKhau.Text == "")
            {
                errorProvider2.SetError(txtMatKhau, "Mật khẩu đang bị bỏ trống");
                txtMatKhau.Focus();
            }
            else
            {
                errorProvider2.SetError(txtMatKhau, "");
                e.Cancel = false;
            }
        }

        /*
         * chkHienThiMK dùng để thay đổi định dạng password cho txtMatKhau
         */
        private void chkHienThiMK_CheckedChanged(object sender, EventArgs e)
        {

            if (chkHienThiMK.Checked)
            {
                txtMatKhau.UseSystemPasswordChar = false;
                //txtMatKhau.PasswordChar = '\0';
            }
            else
            {
                txtMatKhau.UseSystemPasswordChar = true;
                //txtMatKhau.PasswordChar = '*';
            }
        }

        /*
         * btnDangNhap thực hiện đăng nhập
         * Kiểm tra xem tài khoản hợp lệ không
         * Nếu không hợp lệ thì báo
         * Nếu hợp lệ thì thực hiện đăng nhập
         */
        private void btnDangNhap_Click(object sender, EventArgs e)
        {

            try
            {
                /*
                * TK giảng viên có type = 0
                * TK sinh viên có type = 1
                * TK TK quản trị có type 
                */
                if (comboBox1.Text == "Giảng viên")
                {
                    DataTable table = new Database().SelectData("EXEC DangNhap_GV '" + txtDangNhap.Text + "', '" + txtMatKhau.Text + "'");
                    if (table.Rows.Count != 0)
                    {
                        this.Hide();
                        //Truyền tên đăng nhập, mật khẩu và mã giáo viên vào main form
                        new MainForm(txtDangNhap.Text, txtMatKhau.Text, table.Rows[0][2].ToString(), 0).ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        lblSaiTKorMK.Visible = true;
                    }
                }
                else if(comboBox1.Text == "Sinh viên")
                {
                    DataTable table = new Database().SelectData("EXEC DangNhap_SV '" + txtDangNhap.Text + "', '" + txtMatKhau.Text + "'");
                    if (table.Rows.Count != 0)
                    {
                        this.Hide();
                        //Truyền tên đăng nhập, mật khẩu và mã giáo viên vào main form
                        new MainForm(txtDangNhap.Text, txtMatKhau.Text, table.Rows[0][2].ToString(), 1).ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        lblSaiTKorMK.Visible = true;
                    }
                }
                else if(comboBox1.Text == "Quản trị viên")
                {
                    DataTable table = new Database().SelectData("EXEC DangNhap_QTV '" + txtDangNhap.Text + "', '" + txtMatKhau.Text + "'");
                    if (table.Rows.Count != 0)
                    {
                        this.Hide();
                        //Truyền tên đăng nhập, mật khẩu và mã giáo viên vào main form
                        new MainForm(txtDangNhap.Text, txtMatKhau.Text, "0", 2).ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        lblSaiTKorMK.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Hàm Click thực hiện xóa dòng Tên đăng nhập, Mật khẩu
        private void txtDangNhap_Click(object sender, EventArgs e)
        {
            if(txtDangNhap.Text == "Tên đăng nhập")
            {
                txtDangNhap.Clear();
            }
        }

        private void txtMatKhau_Click(object sender, EventArgs e)
        {
            if(txtMatKhau.Text == "Mật khẩu")
            {
                txtMatKhau.Clear();
            }
        }
    }
}
