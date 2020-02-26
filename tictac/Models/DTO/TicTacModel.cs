using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace tictac.Models.DTO
{

    public class ClassModel
    {
        public string data1 { get; set; }
        public string data2 { get; set; }
    
    }
    public class DataTotNghiepModel
    {
        public int Rows { get; set; }
        public int Error { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string NoiSinh { get; set; }
        public Boolean GioiTinh { get; set; }
        public string DanToc { get; set; }
        public string Truong { get; set; }
        public DateTime KhoaThi { get; set; }
        public string HoiDong { get; set; }
        public string SoHieu { get; set; }
        public Boolean Rot { get; set; }
        public Boolean DauSauPhucKhao { get; set; }
    }

    public class ResultCheckFileModel
    {
        public int Rows { get; set; }
        public int Error { get; set; }
        public string HoTen { get; set; }
        public string NgaySinh { get; set; }
        public string NoiSinh { get; set; }
        public string GioiTinh { get; set; }
        public string DanToc { get; set; }
        public string Truong { get; set; }
        public string KhoaThi { get; set; }
        public string HoiDong { get; set; }
        public string SoHieu { get; set; }
        public string Rot { get; set; }
        public string DauSauPhucKhao { get; set; }
    }
    public class VerifyModel
    {
        public string ResponseKey { get; set; }

        public string SecretKey { get; set; }
       
    }

    public class LogInModel
    {
        public string ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }

    public class LogInResult
    {
        public string ID { get; set; }
        public Boolean Result { get; set; }
      
    }
}