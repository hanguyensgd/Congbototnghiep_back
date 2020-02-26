using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Web;
using System.Web.UI;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace tictac.Helper
{
    public class SQL_function
    {
        //String strConnString = "Data Source=172.16.2.8;Database=tuvanduhoc;User Id=sa;Password=QuangTue85@gmail.com;";
        //String strConnString = "Data Source=HANGUYEN\AVANTGARDE;Database= tuvanduhoc ;User Id=sa;Password=Meocon0710@;";


        public String InsertUpdateData(SqlCommand cmd, String db_connection)
        {
            string strConnString = WebConfigurationManager.ConnectionStrings[db_connection].ConnectionString;
            //String strConnString = "Data Source=HANGUYEN\AVANTGARDE;Database= tuvanduhoc ;User Id=sa;Password=Meocon0710@;";


            SqlConnection con = new SqlConnection(strConnString);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            try
            {
                con.Open();
                int row_affected = cmd.ExecuteNonQuery();
                return row_affected.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }

        }

        public String GetData_Json(SqlCommand cmd, String db_connection)
        {
            string strConnString = WebConfigurationManager.ConnectionStrings[db_connection].ConnectionString;
            //String strConnString = "Data Source=HANGUYEN\\AVANTGARDE;Initial Catalog=ThiDuaKhenThuong;User ID=sa;Password=Meocon0710@";

            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(strConnString);
            SqlDataAdapter sda = new SqlDataAdapter();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            try
            {
                con.Open();
                sda.SelectCommand = cmd;
                sda.Fill(dt);
                dt.TableName = "sql_table";

                String res = JsonConvert.SerializeObject(dt);
                return res;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                con.Close();
                sda.Dispose();
                con.Dispose();
            }
        }



        public String GetData(SqlCommand cmd, String db_connection)
        {
            string strConnString = WebConfigurationManager.ConnectionStrings[db_connection].ConnectionString;
            //String strConnString = "Data Source=HANGUYEN\\AVANTGARDE;Initial Catalog=ThiDuaKhenThuong;User ID=sa;Password=Meocon0710@";

            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(strConnString);
            SqlDataAdapter sda = new SqlDataAdapter();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            try
            {
                con.Open();
                sda.SelectCommand = cmd;
                sda.Fill(dt);
                dt.TableName = "sql_table";
                String xml = ToStringAsXml(GetNullFilledDataTableForXML(dt));
                return xml;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                con.Close();
                sda.Dispose();
                con.Dispose();
            }
        }
        private DataTable GetNullFilledDataTableForXML(DataTable dtSource)
        {
            // Create a target table with same structure as source and fields as strings
            // We can change the column datatype as long as there is no data loaded
            DataTable dtTarget = dtSource.Clone();
            foreach (DataColumn col in dtTarget.Columns)
                col.DataType = typeof(string);

            // Start importing the source into target by ItemArray copying which 
            // is found to be reasonably fast for nulk operations. VS 2015 is reporting
            // 500-525 milliseconds for loading 100,000 records x 10 columns 
            // after null conversion in every cell which may be usable in many
            // circumstances.
            // Machine config: i5 2nd Gen, 8 GB RAM, Windows 7 64bit, VS 2015 Update 1
            int colCountInTarget = dtTarget.Columns.Count;
            foreach (DataRow sourceRow in dtSource.Rows)
            {
                // Get a new row loaded with data from source row
                DataRow targetRow = dtTarget.NewRow();
                targetRow.ItemArray = sourceRow.ItemArray;

                // Update DBNull.Values to empty string in the new (target) row
                // We can safely assign empty string since the target table columns
                // are all of string type
                for (int ctr = 0; ctr < colCountInTarget; ctr++)
                    if (targetRow[ctr] == DBNull.Value)
                        targetRow[ctr] = String.Empty;

                // Now add the null filled row to target datatable
                dtTarget.Rows.Add(targetRow);
            }

            // Return the target datatable
            return dtTarget;
        }
        public static string ToStringAsXml(DataTable ds)
        {
            string result;
            using (StringWriter sw = new StringWriter())
            {
                ds.WriteXml(sw);
                result = sw.ToString();
            }

            return result;
        }


        public String[,] xml_deserialize(String xml)
        {
            XElement[] rows;
            try
            {
                XDocument doc = XDocument.Parse(xml);
                rows = (from xml2 in doc.Descendants("sql_table")
                        select xml2).ToArray();
            }

            catch (Exception ex)
            {
                string[,] arr = new String[0, 0];

                return arr;

            }




            if (rows.Length > 0)
            {
                string[,] value_matrix = new String[rows.Length, rows[0].Elements().Count()];

                for (int n = 0; n < rows.Length; n++)
                {
                    for (int i = 0; i < rows[n].Elements().Count(); i++)
                    {
                        value_matrix[n, i] = rows[n].Elements().ToArray()[i].Value.ToString();
                        // monitor.Text = monitor.Text + rows[n].Elements().ToArray()[i].Value.ToString();
                    }
                }
                return value_matrix;
            }

            else
            {
                //Response.Redirect(Request.RawUrl, false);
                string[,] arr = new String[0, 0];
                return arr;

            }


        }

    }
}