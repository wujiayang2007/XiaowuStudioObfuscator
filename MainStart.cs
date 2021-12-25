using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace XiaowuStudioObfuscator
{
    public partial class MainStart : Form
    {
        string strResult = "";
        public MainStart()
        {
            InitializeComponent();
        }
        void StartCheck()
        {
            timer2.Enabled = true;
            try
            {
                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                byte[] pageData = MyWebClient.DownloadData("http://www.xiaowustudio.top/XiaowuStudio/Obfuscator/MD5@1060.txt"); //从指定网站下载数据         
                string pageHtml = Encoding.UTF8.GetString(pageData); //如果获取网站页面采用的是UTF-8，则使用这句

                if (pageHtml == strResult)
                {
                    MainForm frm = new MainForm();
                    timer2.Enabled = false;
                    Hide();
                    frm.ShowDialog();
                }
                else
                {
                    timer2.Enabled = false;
                    Hide();
                    new PasswordToSkip(strResult).ShowDialog();
                }
            }
            catch
            {
                timer2.Enabled = false;
                Hide();
                new PasswordToSkip(strResult).ShowDialog();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            StartCheck();
        }

        private void MainStart_Load(object sender, EventArgs e)
        {
            byte[] arrbytHashValue;
            System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            try
            {
                FileStream oFileStream = new FileStream(Application.ExecutablePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);//计算指定Stream 对象的哈希值
                oFileStream.Close();
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                string strHashData = BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData;
            }
            catch { }
        }

        int i;
        private void timer2_Tick(object sender, EventArgs e)
        {
            if(i == 0)
            {
                label1.Text = "数据加载中";
            }
            else if(i == 1)
            {
                label1.Text = "数据加载中…";
            }
            else if (i == 2)
            {
                label1.Text = "数据加载中……";
            }
            i++;
            if(i == 3)
            {
                i = 0;
            }
        }
    }
}
