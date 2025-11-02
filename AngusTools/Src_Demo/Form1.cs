using Src_Demo.FileHelper;
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
            Test_CfgHelper tc=new Test_CfgHelper();
        }
    }
}
