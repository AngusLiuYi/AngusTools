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
            DataSet dt = new DataSet();
            string path = @"..\..\..\Data\Config.config";
            if (File.Exists(path))
                dt.ReadXml(path);
            string? str = dt.Tables[0].Rows[0]["name"].ToString();


        }
    }
}
