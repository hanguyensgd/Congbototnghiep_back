using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http.Cors;
using tictac.Models.DTO;
using tictac.Helper;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace tictac.Controllers
{
    [RoutePrefix("api")]

    public class TicTacController : ApiController
    {
        private static readonly HttpClient client = new HttpClient();

        [Route("getdata")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public string GetData(string id)
        {
            return "Hello " + id;
        }

        [Route("postdata")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public string PostData(ClassModel class1)
        {
            return "received : " + class1.data1 + ", " + class1.data2;
        }


        [Route("GetNoiSinh")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult GetNoiSinh()
        {
            SQL_function sql_function = new SQL_function();
            String strQuery = @"SELECT [TinhID],[TenTinh],[Moet_TinhID]
            FROM [DuLieuTotNghiep].[dbo].[T_DM_Tinh] ";
            SqlCommand cmd = new SqlCommand(strQuery);
            String json = sql_function.GetData_Json(cmd, "sqlconnString");
            return Json(json);
        }


        [Route("GetDanToc")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult GetDanToc()
        {
            SQL_function sql_function = new SQL_function();
            String strQuery = @"SELECT [DanTocID],[TenDanToc],[Moet_DanTocID]
  FROM [DuLieuTotNghiep].[dbo].[T_DM_DanToc]";
            SqlCommand cmd = new SqlCommand(strQuery);
            String json = sql_function.GetData_Json(cmd, "sqlconnString");
            return Json(json);
        }

        [Route("GetTruong")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult GetTruong()
        {
            SQL_function sql_function = new SQL_function();
            String strQuery = @"SELECT [SchoolID] ,[TenTruong]
  FROM [DuLieuTotNghiep].[dbo].[T_DM_Truong]";
            SqlCommand cmd = new SqlCommand(strQuery);
            String json = sql_function.GetData_Json(cmd, "sqlconnString");
            return Json(json);
        }

        [Route("PostResponse")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult PostResponse(DataTotNghiepModel Model)
        {

            return Ok(Model);
        }

        [Authorize(Roles ="Guest,admin")]
        [Route("PostTraCuu")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult PostTraCuu(DataTotNghiepModel Model)
        {
            SQL_function sql_function = new SQL_function();
            String strQuery = @"SELECT  dbo.DataTotNghiep.ID, dbo.DataTotNghiep.HoTen, dbo.DataTotNghiep.NgaySinh, dbo.DataTotNghiep.NoiSinh, dbo.DataTotNghiep.GioiTinh, dbo.T_DM_DanToc.TenDanToc, dbo.DataTotNghiep.Truong, 
                         dbo.DataTotNghiep.KhoaThi, dbo.DataTotNghiep.HoiDong, dbo.DataTotNghiep.SoHieu, dbo.DataTotNghiep.Rot, dbo.DataTotNghiep.DauSauPhucKhao
FROM            dbo.DataTotNghiep INNER JOIN
                         dbo.T_DM_DanToc ON dbo.DataTotNghiep.DanToc = dbo.T_DM_DanToc.DanTocID 
						 WHERE (HoTen like @HoTen AND NgaySinh = @NgaySinh) OR (SoHieu like @SoHieu)";

            SqlCommand cmd = new SqlCommand(strQuery);
            cmd.Parameters.Add("@HoTen", SqlDbType.NVarChar).Value = Model.HoTen;
            cmd.Parameters.Add("@NgaySinh", SqlDbType.Date).Value = Model.NgaySinh;
            cmd.Parameters.Add("@SoHieu", SqlDbType.NVarChar).Value = Model.SoHieu;
            String json = sql_function.GetData_Json(cmd, "sqlconnString");
            return Json(json);

        }



        //[Authorize(Roles = "Administrator")]
        [Route("ReCaptchaVerify")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> ReCaptchaVerify(VerifyModel Model)
        {
            String secret_key = "6LdSD84UAAAAANYhNhpJUef1_ydrERXTzmVXdH75";
            var values = new Dictionary<string, string>
               {
                { "secret", secret_key },
                { "response", Model.ResponseKey }
               };
            var content = new FormUrlEncodedContent(values);
            HttpResponseMessage response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            if (response.IsSuccessStatusCode)
            {
                var domain = Request.Headers.Referrer? .GetLeftPart(UriPartial.Authority) ?? Request.Headers.UserAgent.ToString();
                SQL_function sql_function = new SQL_function();
                String strQuery = @"INSERT INTO [dbo].[Session] ([ID] ,[UserName] ,[TimeStampt] ,[IP], [isActive]) VALUES (@ID ,@UserName  ,Getdate() ,@IP, 1)";
                SqlCommand cmd = new SqlCommand(strQuery);
                cmd.Parameters.Add("@ID", SqlDbType.NVarChar).Value = Model.ResponseKey.Substring(0, 50);
                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = "Guest";
                cmd.Parameters.Add("@IP", SqlDbType.NVarChar).Value = domain;
                sql_function.InsertUpdateData(cmd, "sqlconnString");
            }
            var responseString = await response.Content.ReadAsStringAsync();
            return Ok(responseString);
        }

        [Authorize(Roles = "Guest")]
        [Route("AdminLogin")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult AdminLogin(LogInModel Model)
        {
            LogInResult resModel = new LogInResult();
            String newID = Shuffle(Model.ID);
            resModel.ID = newID;
            resModel.Result = false;

            SQL_function sql_function = new SQL_function();
            String strQuery_auth = @"SELECT COUNT(*) FROM [DuLieuTotNghiep].[dbo].[Account]
  WHERE [UserName] = @UserName and [Password] =@Password ";
            SqlCommand cmd_auth = new SqlCommand(strQuery_auth);
            cmd_auth.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = Model.UserName;
            cmd_auth.Parameters.Add("@Password", SqlDbType.NVarChar).Value = Model.Password;
            String[,] array_auth = sql_function.xml_deserialize(sql_function.GetData(cmd_auth, "sqlconnString"));

            if (array_auth[0,0] == "1")
            {
                String strQuery = @"UPDATE [dbo].[Session] SET ID =@newID ,[UserName] = @UserName WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(strQuery);
                cmd.Parameters.Add("@newID", SqlDbType.NVarChar).Value = newID;
                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = "admin";
                cmd.Parameters.Add("@ID", SqlDbType.NVarChar).Value = Model.ID;
                String insertRes = sql_function.InsertUpdateData(cmd, "sqlconnString");
                if(insertRes == "1")
                {
                    resModel.Result = true;
                }
            }
         
            return Json(resModel);
        }




        [Authorize(Roles = "admin")]
        [Route("AdminLogout")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public void AdminLogout(LogInModel Model)
        {
            SQL_function sql_function = new SQL_function();
            String strQuery = @"UPDATE [dbo].[Session] SET isActive = 0 WHERE ID = @ID";
            SqlCommand cmd = new SqlCommand(strQuery);
            cmd.Parameters.Add("@ID", SqlDbType.NVarChar).Value = Model.ID;
            String insertRes = sql_function.InsertUpdateData(cmd, "sqlconnString");          
        }


        public string Shuffle( string str)
        {
            char[] array = str.ToCharArray();
            Random rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
            return new string(array);
        }


        //        [Route("LogIn")]
        //        [HttpPost]
        //        [EnableCors(origins: "*", headers: "*", methods: "*")]
        //        public async Task<IHttpActionResult> LogIn(LogInModel Model)
        //        {
        //            String secret_key = "6LdSD84UAAAAANYhNhpJUef1_ydrERXTzmVXdH75";
        //            var values = new Dictionary<string, string>
        //{
        //{ "secret", secret_key },
        //{ "response", Model.ResponseKey }
        //};
        //            var content = new FormUrlEncodedContent(values);
        //            HttpResponseMessage response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                SQL_function sql_function = new SQL_function();
        //                String strQuery = @"INSERT INTO [dbo].[Session]
        //           ([ID]
        //           ,[UserName]
        //           ,[TimeStampt]
        //           ,[IP])
        //     VALUES
        //           (@ID
        //           ,@UserName
        //           ,Getdate()
        //           ,@IP)";
        //                SqlCommand cmd = new SqlCommand(strQuery);
        //                cmd.Parameters.Add("@ID", SqlDbType.NVarChar).Value = Model.ResponseKey.Substring(0, 50);
        //                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = "Guest";
        //                cmd.Parameters.Add("@IP", SqlDbType.NVarChar).Value = "IP";
        //                sql_function.InsertUpdateData(cmd, "sqlconnString");

        //            }
        //            var responseString = await response.Content.ReadAsStringAsync();
        //            return Ok(responseString);
        //        } }



        public String[] NoiSinh()
        {
            
            SQL_function sql_function = new SQL_function();
            String strQuery = @"SELECT [TinhID]
            FROM [DuLieuTotNghiep].[dbo].[T_DM_Tinh] ";
            SqlCommand cmd = new SqlCommand(strQuery);
            String[,] array = sql_function.xml_deserialize(sql_function.GetData(cmd, "sqlconnString"));
            string[] res = new string[array.GetLength(0)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                res[i] = array[i, 0];
            }

            return res;
        }

        public String[] DanToc()
        {

            SQL_function sql_function = new SQL_function();
            String strQuery = @"SELECT[DanTocID],[TenDanToc],[Moet_DanTocID]
        FROM[DuLieuTotNghiep].[dbo].[T_DM_DanToc]";
            SqlCommand cmd = new SqlCommand(strQuery);
            String[,] array = sql_function.xml_deserialize(sql_function.GetData(cmd, "sqlconnString"));
            string[] res = new string[array.GetLength(0)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                res[i] = array[i, 0];
            }

            return res;
        }

        public String[] Truong()
        {

            SQL_function sql_function = new SQL_function();
            String strQuery = @"SELECT [SchoolID] ,[TenTruong]
  FROM [DuLieuTotNghiep].[dbo].[T_DM_Truong]";
            SqlCommand cmd = new SqlCommand(strQuery);
            String[,] array = sql_function.xml_deserialize(sql_function.GetData(cmd, "sqlconnString"));
            string[] res = new string[array.GetLength(0)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                res[i] = array[i, 0];
            }

            return res;
        }

        public String[] SoHieu()
        {

            SQL_function sql_function = new SQL_function();
            String strQuery = @"SELECT DISTINCT [SoHieu] FROM [DuLieuTotNghiep].[dbo].[DataTotNghiep]";
            SqlCommand cmd = new SqlCommand(strQuery);
            String[,] array = sql_function.xml_deserialize(sql_function.GetData(cmd, "sqlconnString"));
            string[] res = new string[array.GetLength(0)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                res[i] = array[i, 0];
            }

            return res;
        }


        public String insertJson(String json)
        {
            String res = "";

            SQL_function sql_function = new SQL_function();
            String strQuery = @"INSERT INTO [dbo].[DataTotNghiep]
           ([HoTen]
           ,[NgaySinh]
           ,[NoiSinh]
           ,[GioiTinh]
           ,[DanToc]
           ,[Truong]
           ,[KhoaThi]
           ,[HoiDong]
           ,[SoHieu]
           ,[Rot]
           ,[DauSauPhucKhao])
SELECT *
FROM OPENJSON(@json)
     WITH (HoTen nvarchar(50), NgaySinh date, NoiSinh nvarchar(50),
           GioiTinh bit, DanToc int,
		    Truong nvarchar(50), KhoaThi date,
			 HoiDong nvarchar(50), SoHieu nvarchar(50),
			 Rot bit, DauSauPhucKhao bit	   
		   )";
            SqlCommand cmd = new SqlCommand(strQuery);
            cmd.Parameters.Add("@json", SqlDbType.NVarChar).Value = json;
            res = sql_function.InsertUpdateData(cmd, "sqlconnString");
            return res;
        }



        [Route("PostDataSet")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public IHttpActionResult PostDataSet(DataTotNghiepModel[] Model)
        {
            DataTotNghiepModel[] res = Model;
            int Erro = 0;
            String HoTen_invalid = "";
            String NgaySinh_invalid = "";
            String NoiSinh_invalid = "";
            String GioiTinh_invalid = "";
            String DanToc_invalid = "";
            String Truong_invalid = "";
            String KhoaThi_invalid = "";
            String HoiDong_invalid = "";
            String SoHieu_invalid = "";
            String Rot_invalid = "";
            String DauSauPhucKhao_invalid = "";

            String[] POB = NoiSinh();
            String[] Ethnic = DanToc();
            String[] School = Truong();
            String[] ID = SoHieu();
            int row = 0;
            for (int i = 0; i < res.Length; i++)
            {
                row++;
                if (Model[i].HoTen.Length >= 50 || !Regex.IsMatch(Model[i].HoTen, @"^[A-Za-zÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯưẠ-ỹ ]+$"))
                {                  
                        HoTen_invalid = HoTen_invalid + i + " ";
                        Erro++;       
                }




                if (res[i].NgaySinh == Convert.ToDateTime("0001-01-01T00:00:00"))
                {
                    NgaySinh_invalid = NgaySinh_invalid + " " + i.ToString();
                    Erro++;
                }


                if (Model[i].NoiSinh.Length >= 50 || !Regex.IsMatch(Model[i].NoiSinh, @"^[A-Za-zÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯưẠ-ỹ ]+$"))
                {
                        NoiSinh_invalid = NoiSinh_invalid + i + " ";
                        Erro++;
                }

               

                if (res[i].GioiTinh != true && res[i].GioiTinh != false)
                {
                    GioiTinh_invalid = GioiTinh_invalid + " " + i.ToString();
                    Erro++;
                }

                int pos_ethnic = Array.IndexOf(Ethnic, Model[i].DanToc);
                if (pos_ethnic == -1)
                {
                    DanToc_invalid = DanToc_invalid + " " + i.ToString();
                    Erro++;
                }


                if (Model[i].Truong.Length >= 50 || !Regex.IsMatch(Model[i].Truong, @"^[A-Za-zÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯưẠ-ỹ ]+$"))
                {
                        Truong_invalid = Truong_invalid + i + " ";
                        Erro++;
                }

               

                if (res[i].KhoaThi == Convert.ToDateTime("0001-01-01T00:00:00"))
                {
                    KhoaThi_invalid = KhoaThi_invalid + " " + i.ToString();
                    Erro++;
                }

                if (string.IsNullOrEmpty(res[i].HoiDong))
                {
                    HoiDong_invalid = HoiDong_invalid + " " + i.ToString();
                    Erro++;
                }

                int pos_id = Array.IndexOf(ID, Model[i].SoHieu);
                if (pos_id > -1)
                {
                    SoHieu_invalid = SoHieu_invalid + " " + i.ToString();
                    Erro++;
                }

                if (res[i].Rot != true && res[i].Rot != false)
                {
                    Rot_invalid = Rot_invalid + " " + i.ToString();
                    Erro++;
                }

                if (res[i].DauSauPhucKhao != true && res[i].DauSauPhucKhao != false)
                {
                    DauSauPhucKhao_invalid = DauSauPhucKhao_invalid + " " + i.ToString();
                    Erro++;
                }
            }


            if(Erro == 0)
            {
                string input = JsonConvert.SerializeObject(res);
                insertJson(input);
            }



            ResultCheckFileModel resModel = new ResultCheckFileModel();

            resModel.Rows = row;
            resModel.Error = Erro;
            resModel.HoTen = HoTen_invalid;

            resModel.NgaySinh = NgaySinh_invalid;
            resModel.NoiSinh = NoiSinh_invalid;
            resModel.GioiTinh = GioiTinh_invalid;
            resModel.DanToc = GioiTinh_invalid;
            resModel.Truong = Truong_invalid;
            resModel.KhoaThi = KhoaThi_invalid;
            resModel.HoiDong = HoiDong_invalid;
            resModel.SoHieu = SoHieu_invalid;
            resModel.Rot = Rot_invalid;
            resModel.DauSauPhucKhao = DauSauPhucKhao_invalid;

            string json = JsonConvert.SerializeObject(new
            {
                results = new List<ResultCheckFileModel>()
                {
                  resModel
                }
            });
            return Json(json);
          

        }

    }
}
