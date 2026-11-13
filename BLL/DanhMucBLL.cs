using System;
using System.Collections.Generic;
using DAL;
using Models;

namespace BLL
{
    public class DanhMucBLL
    {
        private readonly DanhMucDAL _dal = new();

        public List<DanhMucModel> GetAll()
        {
            return _dal.GetAll();
        }

        public DanhMucModel? GetById(int id)
        {
            if (id <= 0) return null;
            return _dal.GetById(id);
        }

        public int Insert(DanhMucModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TenDanhMuc))
                throw new ArgumentException("Tên danh mục không được để trống.");

            return _dal.Insert(model);
        }

        public bool Update(DanhMucModel model)
        {
            if (model.MaDanhMuc <= 0)
                throw new ArgumentException("Mã danh mục không hợp lệ.");
            if (string.IsNullOrWhiteSpace(model.TenDanhMuc))
                throw new ArgumentException("Tên danh mục không được để trống.");

            return _dal.Update(model);
        }

        public bool Delete(int id)
        {
            if (id <= 0) return false;
            return _dal.Delete(id);
        }
    }
}
