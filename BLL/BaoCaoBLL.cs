using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class BaoCaoBLL
    {
        private readonly BaoCaoDAL _dal = new();

        public BaoCaoModel GetTongQuan()
        {
            return _dal.GetTongQuan();
        }

        public List<BaoCaoDoanhThuThang> GetDoanhThuTheoThang(int nam)
        {
            return _dal.GetDoanhThuTheoThang(nam);
        }

        public List<BaoCaoAnPhamBanChay> GetAnPhamBanChay(int top = 10)
        {
            if (top <= 0) top = 10;
            return _dal.GetAnPhamBanChay(top);
        }
    }
}
