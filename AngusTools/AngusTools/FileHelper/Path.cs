using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngusTools.FileHelper
{
    internal class Path
    {
        public Path()
        {
            string FileAbsolutePath = @"D:\A个人项目\GitHub\AngusTools\bin\Debug\net8.0-windows\AngusTools.exe";

            string path = Application.ExecutablePath;//@"D:\A个人项目\GitHub\AngusTools\bin\Debug\net8.0-windows\AngusTools.exe"

            path = Application.StartupPath;//@"D:\A个人项目\GitHub\AngusTools\bin\Debug\net8.0-windows"

            path = @"..\AngusTools.exe";//@"D:\A个人项目\GitHub\AngusTools\bin\Debug\AngusTools.exe"

            path = @"..\..\AngusTools.exe";//@"D:\A个人项目\GitHub\AngusTools\bin\AngusTools.exe"

            path = @"..\..\..\AngusTools.exe";//@"D:\A个人项目\GitHub\AngusTools\AngusTools.exe"
        }
    }
}
