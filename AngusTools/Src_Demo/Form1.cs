using System.Data;

namespace Src_Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string path = @"..\..\..\Data\Config.config";
            string key = "ChangeType";
            AngusTools.FileHelper.ConfigHelper.SaveValueToCfg(path, "ChangeType", "10");
            int value = int.Parse(AngusTools.FileHelper.ConfigHelper.GetCfgValue(path, key));
        }
    }
}
