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
    public partial class XemDiem_Form : Form
    {
        String username, password, id;
        public XemDiem_Form()
        {
            InitializeComponent();
        }
        public XemDiem_Form(String username, String password, String id)
        {
            this.username = username;
            this.password = password;
            this.id = id;
            InitializeComponent();

        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            try
            {
                if(textBox1.Text != "")
                {
                    btnHuyKQ.Visible = true;
                    dataGridView1.DataSource = new Database().SelectData("EXEC TimKiem_Diem '" + id + "', '" + textBox1.Text + "'");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnHuyKQ_Click(object sender, EventArgs e)
        {
            load();
            textBox1.Clear();
            btnHuyKQ.Visible = false;
        }

        public void load()
        {
            try
            {
                dataGridView1.DataSource = new Database().SelectData("EXEC TimKiem_Diem_TheoMaSV '" + id + "'");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        private void XemDiem_Form_Load(object sender, EventArgs e)
        {
            lblMaSV.Text = id;
            load();
        }
    }
}
