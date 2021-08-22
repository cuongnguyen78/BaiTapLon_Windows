using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTLWin
{
    class GiangVien
    {
        public string MaGV { get; set; }
        public string TenGV { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }

        public GiangVien()
        {

        }

        public GiangVien(string MaGV, string TenGV, DateTime NgaySinh, string GioiTinh, string DiaChi, string SDT)
        {
            this.MaGV = MaGV;
            this.TenGV = TenGV;
            this.NgaySinh = NgaySinh;
            this.GioiTinh = GioiTinh;
            this.DiaChi = DiaChi;
            this.SDT = SDT;
        }
    }
}
