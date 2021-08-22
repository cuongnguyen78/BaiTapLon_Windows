using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTLWin
{
    class DiemSV
    {
        public string MaSV { get; set; }
        public string MaMH { get; set; }
        public double DiemTX { get; set; }
        public double DiemKTHP { get; set; }
        public double DiemTB { get; set; }
        public string DiemChu { get; set; }
        public DiemSV()
        {

        }
        public DiemSV(string MaSV, string MaMH, double DiemTX, double DiemKTHP, double DiemTB, string DiemChu)
        {
            this.MaSV = MaSV;
            this.MaMH = MaMH;
            this.DiemTX = DiemTX;
            this.DiemKTHP = DiemKTHP;
            this.DiemTB = DiemTB;
            this.DiemChu = DiemChu;
        }
    }
}
