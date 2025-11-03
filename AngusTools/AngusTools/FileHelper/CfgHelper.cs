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
        public static string? GetCfgValue(string path, string key)
        {
            DataSet ds=CfgToDataTable(path);
            string? str = string.Empty;
            if (ds != null )
                str = ds.Tables[0].Rows[0][key].ToString();
            return str;
        }

        /// <summary>
        /// 将Config文件按照XML架构读取并储存到Dataset表格中
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <returns>读取到的文件表格</returns>
        public static DataSet CfgToDataTable(string path)
        {
            DataSet ds = new DataSet();
            if (File.Exists(path))
                ds.ReadXml(path);
            return ds;
        } 

        /// <summary>
        /// 将指定的键值对写入XML文件中保存
        /// </summary>
        /// <param name="path">保存文件地址</param>
        /// <param name="key">对象键（名称）</param>
        /// <param name="value">对象值</param>
        public static void SaveValueToCfg(string path,string key,string value)
        {
            DataSet ds = CfgToDataTable(path);
            if(!ds.Tables[0].Columns.Contains(key))
                ds.Tables[0].Columns.Add(key);
            ds.Tables[0].Rows[0][key] = value;
            DataTableToCfg(path, ds);
        }

        /// <summary>
        /// 将DataSet表格内容按XML架构写入Config文件中
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <param name="ds">源数据</param>
        public static void DataTableToCfg(string path,DataSet ds)
        {
            if (!File.Exists(path))
                File.Create(path);
            ds.WriteXml(path);
        }
    }
}
