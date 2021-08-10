using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Utils.helper
{
    public class SQLHelper
    {

        /// <summary>
        /// 执行增、删、改（insert/update/delete）
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int ExecuteSQL(string sql, string type = "main")
        {
            SqlConnection conn;
            if (type == "main")
            {
                string ConnectionStr = ConfigurationManager.ConnectionStrings["ConnectionStr"].ToString();
                conn = new SqlConnection(ConnectionStr);
            }
            else
            {
                string ConnectionStr1 = ConfigurationManager.ConnectionStrings["ConnectionStr1"].ToString();
                conn = new SqlConnection(ConnectionStr1);
            }

            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 执查询数据集的操作
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>        
        public static DataTable ExecuteFindSQL(string sql, string type = "main")
        {
            SqlConnection conn;
            if (type == "main")
            {
                string ConnectionStr = ConfigurationManager.ConnectionStrings["ConnectionStr"].ToString();
                conn = new SqlConnection(ConnectionStr);
            }
            else
            {
                string ConnectionStr1 = ConfigurationManager.ConnectionStrings["ConnectionStr1"].ToString();
                conn = new SqlConnection(ConnectionStr1);
            }
            // 创建数据适配器对象
            SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
            // 创建一个内存数据集
            DataTable dt = new DataTable();
            try
            {
                conn.Open();
                // 使用数据适配器填充数据集
                adapter.Fill(dt);
                // 返回数据集
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
