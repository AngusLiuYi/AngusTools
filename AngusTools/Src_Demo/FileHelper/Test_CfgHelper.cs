using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Src_Demo.FileHelper
{
    internal class Test_CfgHelper
    {
        public Test_CfgHelper()
        {
            string path = @"..\..\..\Data\Config.config";
            string key = "ChangeType";
            AngusTools.FileHelper.ConfigHelper.SaveValueToCfg(path, "ChangeType", "10");
            int value = int.Parse(AngusTools.FileHelper.ConfigHelper.GetCfgValue(path, key));

        }
    }
}
