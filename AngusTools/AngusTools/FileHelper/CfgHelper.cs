using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngusTools.FileHelper
{
    //config文件可以使用DataSet.ReadXml/DataSetWriteXml指令直接进行读写
    //所以此类仅作为典型读写操作的示例，不需要专门开发
    //2025年11月2日12:23:56

    public static class CfgHelper
    {
        /// <summary>
        /// 获取指令路径下XML文件的值
        /// 默认该XML文件有且仅有一个主分支，一个对象
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <param name="key">对象索引</param>
        /// <returns>对象值</returns>
        public static string GetCfgValue(string path, string key)
        {
            DataTable dt=CfgToDataTable(path);
            string str = null;
            if (dt == null)
                return "未读到表单";
            if (dt.Columns.Contains(key))
                return "未包含Key";
            if (dt.Rows.Count < 0)
                return "空值";
            return dt.Rows[0][key].ToString() ?? "未获取Key的值";
        }

        /// <summary>
        /// 将Config文件按照XML架构读取并储存到Dataset表格中
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <returns>读取到的文件表格</returns>
        public static DataTable CfgToDataTable(string path)
        {
            DataSet ds = new();
            if (File.Exists(path))
                ds.ReadXml(path);
            else ds.Tables.Add();
            return ds.Tables[0];
        } 

        /// <summary>
        /// 将指定的键值对写入XML文件中保存
        /// </summary>
        /// <param name="path">保存文件地址</param>
        /// <param name="key">对象键（名称）</param>
        /// <param name="value">对象值</param>
        public static void SaveValueToCfg(string path,string key,string value)
        {
            DataTable dt = CfgToDataTable(path);
            if(!dt.Columns.Contains(key))
                dt.Columns.Add(key);
            dt.Rows[0][key] = value;
            DataTableToCfg(path, dt);
        }

        /// <summary>
        /// 将DataSet表格内容按XML架构写入Config文件中
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <param name="ds">源数据</param>
        public static void DataTableToCfg(string path,DataTable dt)
        {
            if (!File.Exists(path))
            {
                using FileStream fs = new(path, FileMode.Create, FileAccess.Write); ;
            }
            dt.TableName = "configuration";
            dt.WriteXml(path);
        }
    }
}
