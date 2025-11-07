using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngusTools.FileHelper
{
    public static class CsvHelper
    {
        #region List读写
        /// <summary>
        /// 读取csv文件，以List格式返回
        /// 默认无表头返回
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static List<string> CsvToList(string path) => CsvToList(path, false);

        /// <summary>
        /// 读取csv文件，以List格式返回
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="isHasTitle">是否含有表头</param>
        /// <returns></returns>
        public static List<string> CsvToList(string path, bool isHasTitle)
        {
            if (!File.Exists(path))
                return new List<string>();
            return File.ReadAllLines(path).ToList();
        }
        #endregion

        #region 读取到DataTable
        /// <summary>
        /// 读取csv文件，以Dt格式返回
        /// 默认有表头
        /// 默认编码方式GB2312
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DataTable CsvToDataTable(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return CsvToDataTable(path, true, Encoding.GetEncoding("gb2312"));
        }

        /// <summary>
        /// 读取csv文件，以Dt格式返回
        /// 默认编码方式GB2312
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <param name="isHasTitle">是否含有表头</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DataTable CsvToDataTable(string path, bool isHasTitle)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return CsvToDataTable(path, isHasTitle, Encoding.GetEncoding("gb2312"));
        }

        /// <summary>
        /// 读取csv文件，以Dt格式返回
        /// 默认有表头
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <param name="encoding">文件编码方式（记事本通常为UTF-8，表格通常为GB2312）</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DataTable CsvToDataTable(string path, Encoding encoding) => CsvToDataTable(path, true, encoding);

        /// <summary>
        /// 读取csv文件，以Dt格式返回
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <param name="isHasTitle">是否含有表头</param>
        /// <param name="encoding">文件编码方式（记事本通常为UTF-8，表格通常为GB2312）</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DataTable CsvToDataTable(string path, bool isHasTitle,Encoding encoding)
        {
            if (!File.Exists(path))
                return new DataTable();
            var lines = new List<string>();
            using (StreamReader sr = new(path))
            {
                lines = File.ReadAllLines(path, encoding).ToList();
            }
            bool isFirst = true;
            DataTable dt = new();
            foreach (var line in lines)
            {
                string[] str = line.Split(',');
                if (isFirst)
                {
                    for (int i = 0; i < str.Length; i++)
                    {
                        dt.Columns.Add();
                        if (isHasTitle)
                            dt.Columns[i].ColumnName = str[i];
                    }
                }
                if (str.Length == dt.Columns.Count)
                {
                    if (!isFirst)
                        dt.Rows.Add(str);
                }
                else throw new Exception("csv文件错误：表格行列数量不一致");
                isFirst = false;
            }
            return dt;
        }
        #endregion

        #region 从DataTable写入到csv文件
        /// <summary>
        /// 将DT的数据写入到csv文件中保存
        /// 默认有表头
        /// 默认覆盖原文件
        /// 默认编码GB2312
        /// </summary>
        /// <param name="dt">文件源</param>
        /// <param name="path">目标文件地址</param>
        /// <returns></returns>
        public static bool DataTableToCsv(DataTable dt, string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return DataTableToCsv(dt, path, true, false, Encoding.GetEncoding("gb2312"));
        }

        /// <summary>
        /// 将DT的数据写入到csv文件中保存
        /// 默认覆盖原文件
        /// 默认编码GB2312
        /// </summary>
        /// <param name="dt">文件源</param>
        /// <param name="path">目标文件地址</param>
        /// <param name="isHasTitle">是否写入表头</param>
        /// <returns></returns>
        public static bool DataTableToCsv(DataTable dt, string path, bool isHasTitle)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return DataTableToCsv(dt, path, isHasTitle, false, Encoding.GetEncoding("gb2312"));
        }

        /// <summary>
        /// 将DT的数据写入到csv文件中保存
        /// 默认编码GB2312
        /// </summary>
        /// <param name="dt">文件源</param>
        /// <param name="path">目标文件地址</param>
        /// <param name="isHasTitle">是否写入表头</param>
        /// <param name="isAppend">追加或覆盖原文件</param>
        /// <returns></returns>
        public static bool DataTableToCsv(DataTable dt, string path, bool isHasTitle, bool isAppend)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return DataTableToCsv(dt, path, isHasTitle, isAppend, Encoding.GetEncoding("gb2312"));
        }

        /// <summary>
        /// 将DT的数据写入到csv文件中保存
        /// 默认有表头
        /// 默认覆盖原文件
        /// </summary>
        /// <param name="dt">文件源</param>
        /// <param name="path">目标文件地址</param>
        /// <param name="encoding">编码方式</param>
        /// <returns></returns>
        public static bool DataTableToCsv(DataTable dt, string path, Encoding encoding) => DataTableToCsv(dt, path, true, false, encoding);

        /// <summary>
        /// 将DT的数据写入到csv文件中保存
        /// 默认覆盖原文件
        /// </summary>
        /// <param name="dt">文件源</param>
        /// <param name="path">目标文件地址</param>
        /// <param name="isHasTitle">是否写入表头</param>
        /// <param name="encoding">编码方式</param>
        /// <returns></returns>
        public static bool DataTableToCsv(DataTable dt, string path, bool isHasTitle, Encoding encoding) => DataTableToCsv(dt, path, isHasTitle, false, encoding);


        /// <summary>
        /// 将DT的数据写入到csv文件中保存
        /// </summary>
        /// <param name="dt">文件源</param>
        /// <param name="path">目标文件地址</param>
        /// <param name="isHasTitle">是否写入表头</param>
        /// <param name="isAppend">追加或覆盖原文件</param>
        /// <param name="encoding">编码方式</param>
        /// <returns></returns>
        public static bool DataTableToCsv(DataTable dt, string path, bool isHasTitle, bool isAppend, Encoding encoding)
        {
            if (!File.Exists(path))
            {
                using FileStream fs = new(path, FileMode.Create, FileAccess.Write);
            }
            using (StreamWriter sw = new(path, isAppend, encoding))
            {
                if (isHasTitle)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sw.Write(dt.Columns[i].ColumnName);
                        if (i != dt.Columns.Count - 1)
                            sw.Write(",");
                    }
                    sw.WriteLine();
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        sw.Write(dt.Rows[i][j].ToString());
                        if (j != dt.Columns.Count - 1)
                            sw.Write(",");
                    }
                    sw.WriteLine();
                }
            }
            return true;
        }

        #endregion
    }
}
