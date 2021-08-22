using Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDataReader;

namespace BTLWin
{    
    public partial class QuanLyDiem : Form
    {
        
        private String username, password, id;

       /*
        * isSaved lưu trạng thái dữ liệu
        * true khi dữ liệu khi dữ liệu đã được lưu
        * false khi dữ liệu khi chưa đc lưu
        */
        bool isSaved;

        /*
         * MaMH lưu mã môn học được chọn
         */
        String MaMH;
        
        public QuanLyDiem()
        {
            InitializeComponent();
        }

        public QuanLyDiem(string username, String password, String id)
        {
            this.username = username;
            this.password = password;
            this.id = id;
            InitializeComponent();
            /*
            this.MaGV = username;
            isSaved = true;
            luuExcel = false;
            xoaDiemCSDL = new List<xoaDiem>();
            */
        }

        //Load lại thông tin form
        //Thực hiện đẩy thông tin điểm vào datagridview2 dựa trên môn đc chọn bên datagridview1, cập nhật số lg sinh viên
        public void load()
        {
            try
            {
                dataGridView2.ReadOnly = false;
                //Lấy ra thông tin môn học được chọn ở datagridview1 (currentCell là ô đang được chọn)
                if (dataGridView1.CurrentCell != null)
                {
                    int row, col;
                    row = dataGridView1.CurrentCell.RowIndex;
                    col = dataGridView1.CurrentCell.ColumnIndex;
                    MaMH = dataGridView1.Rows[row].Cells[0].Value.ToString();
                    txtTimKiem.Clear();
                    //Nếu ô hợp lệ thì lấy MaMH rồi đẩy điểm của môn học đấy vào datagridview2, tính số sinh viên
                    if (row >= 0 && col >= 0)
                    {
                        DataTable dt = new Database().SelectData("EXEC TimKiem_Diem_TheoMaMH '" + MaMH + "'");
                        dataGridView2.DataSource = dt;
                        lblTongSV.Text = dt.Rows.Count + " sinh viên";
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //isSaved= true: Nghĩa là không có thay đổi dữ liệu trên datagridview2, dữ liệu đã được lưu
                isSaved = true;
                //không cho phép sửa trên datagridview2
                dataGridView2.ReadOnly = true;
            }
        }

        //Kiểm tra dữ liệu khi nhập vào từ bàn phím hoặc từ exel
        public int KiemTraDuLieu()
        {
            try
            {
                //Kiểm tra xem bảng dữ liệu hợp lệ chưa
                //Dùng cho việc nếu file exel nhập vào sai thì kết thúc luôn
                if (dataGridView2.ColumnCount != 6) return 0;
                if (
                dataGridView2.Columns[0].HeaderText != "Mã sinh viên" ||
                dataGridView2.Columns[1].HeaderText != "Mã môn học" ||
                dataGridView2.Columns[2].HeaderText != "Điểm thường xuyên" ||
                dataGridView2.Columns[3].HeaderText != "Điểm thi kết thúc học phần" ||
                dataGridView2.Columns[4].HeaderText != "Điểm trung bình" ||
                dataGridView2.Columns[5].HeaderText != "Điểm chữ"
                )
                {
                    return 0;
                }

                //Chuỗi thông báo lỗi
                String error = "";
                //List các dòng dữ liệu bị lỗi
                List<int> e = new List<int>();
                try
                {
                    //Duyệt bảng dữ liệu
                    for (int i = 0; i < dataGridView2.RowCount - 1; i++)
                    {
                        //Kiểm tra hợp lệ của các đầu điểm
                        //Nếu các điểm TX và KTHP đc điền, thì thực hiện tính toán điểm Tb và điểm chữ
                        double diemtx, diemkthp, diemtb;
                        if (double.TryParse(dataGridView2.Rows[i].Cells[2].Value.ToString(), out diemtx)
                           && double.TryParse(dataGridView2.Rows[i].Cells[3].Value.ToString(), out diemkthp)
                           && diemtx >= 0 && diemtx <= 10 && diemkthp >= 0
                           && diemkthp <= 10)
                        {
                            diemtx = Math.Round(diemtx, 1);
                            diemkthp = Math.Round(diemkthp, 1);
                            diemtb = Math.Round((diemtx + diemkthp * 2) / 3, 1);
                            if (diemtb >= 8.5) dataGridView2.Rows[i].Cells[5].Value = "A";
                            else if (diemtb >= 7.7) dataGridView2.Rows[i].Cells[5].Value = "B+";
                            else if (diemtb >= 7) dataGridView2.Rows[i].Cells[5].Value = "B";
                            else if (diemtb >= 6.2) dataGridView2.Rows[i].Cells[5].Value = "C+";
                            else if (diemtb >= 5.5) dataGridView2.Rows[i].Cells[5].Value = "C";
                            else if (diemtb >= 4.7) dataGridView2.Rows[i].Cells[5].Value = "D+";
                            else if (diemtb >= 4.0) dataGridView2.Rows[i].Cells[5].Value = "D";
                            else if (diemtb < 4) dataGridView2.Rows[i].Cells[5].Value = "F";
                            dataGridView2.Rows[i].Cells[4].Value = diemtb + "";
                        }
                        else
                        {
                            error = error + (i + 1) + " ";
                            //Thêm cột lỗi vào danh sách dòng lỗi e
                            e.Add(i);
                        }
                        DataTable dt = new DataTable();
                        //Biến check dùng để lưu trạng thái dữ liệu
                        //check= 1 dữ liệu hợp lệ(default)
                        //check= 0 dữ liệu không hơp lệ
                        int check = 1;
                        //Độ dài chuỗi của các cột
                        int length_cell0 = dataGridView2.Rows[i].Cells[0].Value.ToString().Length;
                        int length_cell1 = dataGridView2.Rows[i].Cells[1].Value.ToString().Length;
         
                        //Kiểm tra xem có cột nào trống không
                        //Nếu phát hiện trống thì thêm vào bảng dữ liệu lỗi e
                        //Nếu không trống thực hiện tiếp câu lệnh if
                        if (length_cell0 != 0 && length_cell1 != 0 )
                        {
                            //Mã sinh viên
                            String masv = dataGridView2.Rows[i].Cells[0].Value.ToString();
                            //Mã môn học
                            String mamh = dataGridView2.Rows[i].Cells[1].Value.ToString();
                            //Kiểm tra tính hợp lệ của MaSV
                            try
                            {
                                //Kiểm tra xem mã sinh viên có tồn tại không?
                                //Câu lệnh cmd trả về 1 nếu sinh viên tồn tại, 0 nếu không tồn tại
                                dt = new Database().SelectData("SELECT dbo.Fn_KiemTra_SinhVien('" + masv + "')");
                                check = int.Parse(dt.Rows[0][0].ToString());
                                dt = new DataTable();
                                //Kiểm tra xem mã lớp hợp lệ hay không
                                if (mamh != MaMH)
                                {
                                    check = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                //Quá trình kiểm tra trả về 0 nếu xảy ra lỗi
                                //Thông báo lỗi, kết thúc cập nhật
                                MessageBox.Show(ex.Message);
                                return 0;
                            }
                            //Nếu check=0, MaSV không hợp lệ
                            if (check == 0)
                            {
                                error = error + (i + 1) + " ";
                                //Thêm cột lỗi vào danh sách dòng lỗi e
                                e.Add(i);
                            }
                        }
                        else
                        {
                            //Nếu dòng dữ liệu này chưa có trong bảng lỗi
                            if(e.IndexOf(i) == -1)
                            {
                                //Nếu có 1 cột bất kỳ trống, dữ liệu không hợp lệ
                                error = error + (i + 1) + " ";
                                //Thêm cột lỗi vào danh sách dòng lỗi e
                                e.Add(i);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return 0;
                }
                // Kiểm tra xem có dòng dữ liệu lỗi không
                //Nếu không có return 1 để tiếp tục cập nhật
                //Nếu có thì thực hiện câu lệnh if
                try
                {
                    if (e.Count != 0)
                    {
                        //Thông báo các dòng lỗi
                        //Hỏi người dùng muốn tiếp tục cập nhật không
                        //Nếu tiếp tục thì sẽ xóa toàn bộ các dòng lỗi, return 1 để tiếp tục quá trình
                        //Nếu không return 0, kết thúc cập nhật
                        DialogResult rsl = MessageBox.Show("Lỗi dữ liệu tại các dòng " + error + "\nNếu tiếp tục sẽ bỏ qua những dữ liệu không hợp lệ", "Cảnh bảo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (rsl == DialogResult.Yes)
                        {
                            for(int i = e.Count-1;i>= 0; i--)
                            {
                                dataGridView2.Rows.RemoveAt(e[i]);
                            }
                            e.Clear();
                            return 1;
                        }
                        else return 0;
                    }
                    else return 1;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return 1;
        }

        //Hàm xóa dòng dữ liệu trong datagridview
        public void Xoa(String masv, String mamh)
        {
            //Kiểm tra xem có chuỗi nào rỗng không, nếu có thì dữ liệu không hợp lệ
            try
            {
                if (masv.Length != 0 && mamh.Length != 0)
                {
                    /*
                     * Câu lệnh cmd sẽ kiểm tra xem đầu điểm này tồn tại không
                     * Nếu tồn tại thì xóa, nếu không thì bỏ qua
                     * -- Vì khi thực hiện xóa, có thể dòng dữ liệu vừa xóa có thể vẫn chưa cập nhật vào CSDL mà chỉ có trên datagridview2
                     */

                    new Database().ExecCmd("EXEC Delete_Diem '" + masv + "', '" + mamh + "'");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Hàm import file exel, trả về là một bảng dữ liệu
        public DataTable Import()
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Workbook|*xlsx", ValidateNames = true })
            {
                DataTable dt = new DataTable();
                if (ofd.ShowDialog() == DialogResult.OK)
                {

                    using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        IExcelDataReader reader;
                        if (ofd.FilterIndex == 2)
                        {
                            reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        }
                        else
                        {
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        }
                        DataSet ds = new DataSet();
                        ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });
                        foreach (DataTable item in ds.Tables)
                        {
                            dt = item;
                        }
                        reader.Close();

                    }
                    return dt;
                }
            }
            return null;
        }

        private void QuanLyDiem_Load(object sender, EventArgs e)
        {
            //Đưa ra bảng danh sách môn học vào datagridview1, rồi thực hiện hàm load
            try
            {
                dataGridView1.DataSource = new Database().SelectData("EXEC TimKiemMonHoc_TheoMaGV '"+this.username+"'");
                load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            /*
            dataGridView2.DataSource = new Database().SelectData("EXEC TimKiem_Diem_TheoMaMH '" + dataGridView1.Rows[0].Cells[0].Value + "'");
            MaMH = dataGridView1.Rows[0].Cells[0].Value.ToString();
            MaMHCu = MaMH;
            lblTongSV.Text = dataGridView2.RowCount + " sinh viên";
            */
        }
        
        //Khi click vào một môn học trong datagridview1, điểm của môn học đó sẽ đẩy vào datagridview2
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //Kiểm tra Cell có hợp lệ không
                if (e.RowIndex > -1)
                {
                    /*Kiểm tra xem dữ liệu được lưu chưa
                     * isSaved: true thì dữ liệu đã được lưu
                     * isSaved: false thì dữ liệu chưa được lưu, thực hiện hỏi người dùng muốn lưu không
                     */
                    if (!isSaved)
                    {
                        DialogResult result = MessageBox.Show("Thông tin bạn vừa cập nhập chưa được lưu. \nBạn có muốn lưu chúng không ?", "Thông báo",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        //Nếu người dùng muốn lưu thì thực hiện lưu, không thì hủy những thay đổi
                        if (result == DialogResult.Yes)
                        {
                            btnLuu_Click(sender, e);
                        }
                    }
                }
                load();
            }
            catch (Exception ex)
            {
            }
        }

        //Hàm được thực hiện khi bắt đầu edit dữ liệu trong datagridview, tự động cập nhật mã sinh viên
        private void dataGridView2_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                int row = e.RowIndex;
                int column = e.ColumnIndex;
                dataGridView2.Rows[row].Cells[1].Value = MaMH;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Hàm được thực hiện khi kết thức edit, kiểm tra dữ liệu có là kiểu số không, nếu là kiểu số thì tự động tính điểm TB và điểm chữ
        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //Kiểm tra hợp lệ của các đầu điểm
                //Nếu các điểm TX và KTHP đc điền, thì thực hiện tính toán điểm Tb và điểm chữ
                double diemtx, diemkthp, diemtb;
                if (double.TryParse(dataGridView2.Rows[e.RowIndex].Cells[2].Value.ToString(), out diemtx)
                   && double.TryParse(dataGridView2.Rows[e.RowIndex].Cells[3].Value.ToString(), out diemkthp)
                   && diemtx >= 0 && diemtx <= 10 && diemkthp >= 0
                   && diemkthp <= 10)
                {
                    diemtb = Math.Round((diemtx + diemkthp * 2) / 3, 1);
                    if (diemtb >= 8.5) dataGridView2.Rows[e.RowIndex].Cells[5].Value = "A";
                    else if (diemtb >= 7.7) dataGridView2.Rows[e.RowIndex].Cells[5].Value = "B+";
                    else if (diemtb >= 7) dataGridView2.Rows[e.RowIndex].Cells[5].Value = "B";
                    else if (diemtb >= 6.2) dataGridView2.Rows[e.RowIndex].Cells[5].Value = "C+";
                    else if (diemtb >= 5.5) dataGridView2.Rows[e.RowIndex].Cells[5].Value = "C";
                    else if (diemtb >= 4.7) dataGridView2.Rows[e.RowIndex].Cells[5].Value = "D+";
                    else if (diemtb >= 4.0) dataGridView2.Rows[e.RowIndex].Cells[5].Value = "D";
                    else if (diemtb < 4) dataGridView2.Rows[e.RowIndex].Cells[5].Value = "F";
                    dataGridView2.Rows[e.RowIndex].Cells[4].Value = diemtb + "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Hàm được thực hiện khi đang edit, kiểm tra dữ liệu có hợp lệ không
        private void dataGridView2_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //Nếu đang nhập ô diemTX và diemKTHP
            //Kiểm tra tính hợp lệ của dữ liệu
            //Dữ liệu phải là kiểu số dương từ 0 đến 10
            try
            {
                int column = e.ColumnIndex;
                double diem;
                if (column == 2 || column == 3)
                {
                    if (dataGridView2.Rows[e.RowIndex].IsNewRow) { return; }
                    if (!double.TryParse(e.FormattedValue.ToString(), out diem) && diem >= 0 && diem <= 10)
                    {
                        e.Cancel = true;
                        dataGridView2.Rows[e.RowIndex].ErrorText = "Dữ liệu phải kiểu số, có giá trị từ 0 đến 10";
                    }
                    else
                    {
                        dataGridView2.Rows[e.RowIndex].ErrorText = null;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }


        //Hàm nhập exel, thực hiện đẩy file exel vào datagridview2
        private void btnNhapExcel_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = Import();
                if (dt != null)
                {
                    dataGridView2.DataSource = dt;
                    isSaved = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Hàm xuất exel
        private void btnXuatFile_Click(object sender, EventArgs e)
        {

            try
            {
                if (!isSaved)
                {
                    MessageBox.Show("Phải lưu thay đổi trước khi xuất file exel");
                }
                else
                {
                    //Khoi tao Excel
                    Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();

                    //Khoi tao WorkBook
                    Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);

                    //Khoi tao WorkSheet

                    Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
                    worksheet = workbook.Sheets["Sheet1"];
                    worksheet = workbook.ActiveSheet;
                    app.Visible = true; //hien thi excel len
                                        //Do du lieu vao Sheet

                    worksheet.Cells[1, 1] = "BẢNG ĐIỂM SINH VIÊN THEO MÔN HỌC ";
                    worksheet.Cells[3, 2] = "Mã môn:  " + MaMH;
                    worksheet.Cells[4, 2] = "Tên Môn:  " + dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[1].Value.ToString();
                    worksheet.Cells[5, 2] = "Số lượng sinh viên:  " + lblTongSV.Text;

                    worksheet.Cells[9, 1] = "STT";
                    worksheet.Cells[9, 2] = "Mã sinh viên";
                    worksheet.Cells[9, 3] = "Mã môn học";
                    worksheet.Cells[9, 4] = "Điểm thường xuyên";
                    worksheet.Cells[9, 5] = "Điểm thi kết thúc học phần";
                    worksheet.Cells[9, 6] = "Điểm trung bình";
                    worksheet.Cells[9, 7] = "Điểm chữ";
                    for (int i = 0; i < dataGridView2.RowCount -1; i++)
                    {
                        worksheet.Cells[i + 10, 1] = i + 1;
                        for (int j = 0; j < 6; j++)
                        {
                            worksheet.Cells[i + 10, j + 2] = dataGridView2.Rows[i].Cells[j].Value.ToString();

                        }

                    }
                    
                    int mon = dataGridView2.RowCount;
                    //Dinh dang trang
                    worksheet.PageSetup.Orientation = Microsoft.Office.Interop.Excel.XlPageOrientation.xlPortrait;
                    worksheet.PageSetup.PaperSize = Microsoft.Office.Interop.Excel.XlPaperSize.xlPaperA3;
                    worksheet.PageSetup.LeftMargin = 0;
                    worksheet.PageSetup.RightMargin = 0;
                    worksheet.PageSetup.TopMargin = 0;
                    worksheet.PageSetup.BottomMargin = 0;
                    //Dinh dang cot
                    worksheet.Range["A1"].ColumnWidth = 5;
                    worksheet.Range["B1"].ColumnWidth = 15;
                    worksheet.Range["C1"].ColumnWidth = 10;
                    worksheet.Range["D1"].ColumnWidth = 30;
                    worksheet.Range["E1"].ColumnWidth = 30;
                    worksheet.Range["F1"].ColumnWidth = 30;
                    worksheet.Range["G1"].ColumnWidth = 15;

                    //Dinh dang font chu
                    worksheet.Range["A1", "F100"].Font.Name = "Times New Roman";
                    worksheet.Range["A1", "G1"].MergeCells = true;
                    worksheet.Range["A1", "G1"].Font.Bold = true;
                    //Ke bang diem
                    worksheet.Range["A9", "G" + (mon + 9)].Borders.LineStyle = 1;
                   // Ke cac dong text
                    worksheet.Range["A1", "G1"].HorizontalAlignment = 3;
                    worksheet.Range["A9", "G9"].HorizontalAlignment = 3;
                    worksheet.Range["A10", "A" + (mon + 9)].HorizontalAlignment = 3;
                    worksheet.Range["B10", "B" + (mon + 9)].HorizontalAlignment = 3;
                    worksheet.Range["C10", "C" + (mon + 9)].HorizontalAlignment = 3;
                    worksheet.Range["D10", "D" + (mon + 9)].HorizontalAlignment = 3;
                    worksheet.Range["E10", "E" + (mon + 9)].HorizontalAlignment = 3;
                    worksheet.Range["F10", "F" + (mon + 9)].HorizontalAlignment = 3;
                    worksheet.Range["G10", "G" + (mon + 9)].HorizontalAlignment = 3;
                }
            }
            catch (Exception ex)
            {
            }
        }

        //Hàm reset, thực hiện xóa toàn bộ nhưng thay đổi chưa được lưu và load lại dữ liệu
        private void btnReset_Click(object sender, EventArgs e)
        {
            load();
        }

        //Hàm tìm kiếm theo mã sinh viên
        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            try
            {
                //Nếu ô text không trống, thực hiện tìm sinh viên theo mã sinh viên
                if (txtTimKiem.Text != "")
                {
                    DataTable dt = new Database().SelectData("EXEC TimKiem_Diem '" + txtTimKiem.Text + "', '" + MaMH + "'");
                    dataGridView2.DataSource = dt;
                    btnHuyKQ.Visible = true;
                    lblTongSV.Text = dt.Rows.Count + " sinh viên";
                    dataGridView2.Columns[0].HeaderText = "Mã sinh viên";
                    dataGridView2.Columns[1].HeaderText = "Mã môn học";
                    dataGridView2.Columns[2].HeaderText = "Điểm thường xuyên";
                    dataGridView2.Columns[3].HeaderText = "Điểm thi kết thúc học phần";
                    dataGridView2.Columns[4].HeaderText = "Điểm trung bình";
                    dataGridView2.Columns[5].HeaderText = "Điểm chữ";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Hàm hủy kết quả tìm kiếm
        private void btnHuyKQ_Click(object sender, EventArgs e)
        {
            load();
        }
        
        //Hàm lưu sự thay đổi
        private void btnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                /*
                 * Kiểm tra xem dữ liệu được lưu chưa
                 * Nếu chưa được lưu thì thực hiện lưu dữ liệu
                 */
                if (!isSaved)
                {
                    //Nếu KiemTraDuLieu trả về 1 thì thực hiện lưu, lếu trả về 0 thì thông báo lưu thất bại
                    int check;
                    check = KiemTraDuLieu();
                    if (check == 1)
                    {
                        //Thực hiện cập nhật từng dòng dữ liệu
                        //Nếu đầu điểm đã tồn tại: thực hiện cập nhật điểm
                        //Nếu đầu điểm không tồn tại: thực hiện thêm mới đầu điểm
                        try
                        {
                            /*
                             * Nguyên tắc hoạt động của procedure Update_Diem:
                             * Nếu đầu điểm này đã tồn tại trong CSDL thì thực hiện update
                             * Nếu đàu điểm này chưa tồn tại thì thực hiện insert
                             * Vì các lỗi đã được kiểm tra tại KiemTraDuLieu nên dữ liệu đầu vào là hoàn toàn chính xác
                             */
                            for (int i = 0; i < dataGridView2.RowCount - 1; i++)
                            {
                                DiemSV diem = new DiemSV();
                                diem.MaSV = dataGridView2.Rows[i].Cells[0].Value.ToString();
                                diem.MaMH = dataGridView2.Rows[i].Cells[1].Value.ToString();
                                diem.DiemChu = dataGridView2.Rows[i].Cells[5].Value.ToString();
                                diem.DiemTX = double.Parse(dataGridView2.Rows[i].Cells[2].Value.ToString());
                                diem.DiemKTHP = double.Parse(dataGridView2.Rows[i].Cells[3].Value.ToString());
                                diem.DiemTB = double.Parse(dataGridView2.Rows[i].Cells[4].Value.ToString());
                                new Database().ExecCmd("EXEC Update_Diem '" + diem.MaSV + "', '" + diem.MaMH + "', " + diem.DiemTX + ", " + diem.DiemKTHP + ", " + diem.DiemTB + ", '" + diem.DiemChu + "'");
                            }
                            MessageBox.Show("Cập nhật thành công");
                            load();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    }
                    else
                    {
                        MessageBox.Show("Cập nhật thất bại");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                load();
                MessageBox.Show(ex.Message);
            }
        }

        //Hàm chỉnh sửa cho phép chỉnh sửa trên datagridview2, đưa isaved= false (Nghĩa là dữ liệu chưa đc lưu)
        private void btnChinhSua_Click(object sender, EventArgs e)
        {
            dataGridView2.ReadOnly = false;
            dataGridView2.AllowUserToDeleteRows = true;
            dataGridView2.Columns[1].ReadOnly = true;
            dataGridView2.Columns[4].ReadOnly = true;
            dataGridView2.Columns[5].ReadOnly = true;
            isSaved = false;
        }

        //Hàm xóa thực hiện nhiệm vụ xóa
        private void btnXoa_Click(object sender, EventArgs e)
        {
            try
            {
                //Lấy MaSV và MaMH điểm cần xóa
                int rowindex = dataGridView2.CurrentCell.RowIndex;
                DiemSV diemxoa = new DiemSV();
                diemxoa.MaSV = dataGridView2.Rows[rowindex].Cells[0].Value.ToString();
                diemxoa.MaMH = dataGridView2.Rows[rowindex].Cells[1].Value.ToString();
                //Hỏi trước khi xóa
                DialogResult a = MessageBox.Show("Bạn muốn xóa đầu điểm này ra khỏi danh sách?", "Hỏi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (a == DialogResult.Yes)
                {
                    /*
                     * Thực hiện xóa trong csdl
                     */
                    Xoa(diemxoa.MaSV, diemxoa.MaMH);
                    //Xóa trên datagridview
                    dataGridView2.Rows.RemoveAt(rowindex);
                }
                //Ngược lại thì hủy
                else
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
