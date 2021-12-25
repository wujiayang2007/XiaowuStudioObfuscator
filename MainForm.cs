using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Mono.Cecil.Cil;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace XiaowuStudioObfuscator
{
    public partial class MainForm : Form
    {
        private StartPage startPage1;
        public Panel panel6;
        byte[] xws_obf = null;
        static List<string> FileName = new List<string>();
        Thread thread;
        public MainForm()
        {
            InitializeComponent();
        }

        /** 佛祖保佑 永无BUG
         *                             _ooOoo_
         *                            o8888888o
         *                            88" . "88
         *                            (| -_- |)
         *                            O\  =  /O
         *                         ____/`---'\____
         *                       .'  \\|     |//  `.
         *                      /  \\|||  :  |||//  \
         *                     /  _||||| -:- |||||-  \
         *                     |   | \\\  -  /// |   |
         *                     | \_|  ''\---/''  |   |
         *                     \  .-\__  `-`  ___/-. /
         *                   ___`. .'  /--.--\  `. . __
         *                ."" '<  `.___\_<|>_/___.'  >'"".
         *               | | :  `- \`.;`\ _ /`;.`/ - ` : | |
         *               \  \ `-.   \_ __\ /__ _/   .-` /  /
         *          ======`-.____`-.___\_____/___.-`____.-'======
         *                             `=---='
         *          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
         *                     佛祖保佑        永无BUG
         *            佛曰:
         *                   写字楼里写字间，写字间里程序员；
         *                   程序人员写程序，又拿程序换酒钱。
         *                   酒醒只在网上坐，酒醉还来网下眠；
         *                   酒醉酒醒日复日，网上网下年复年。
         *                   但愿老死电脑间，不愿鞠躬老板前；
         *                   奔驰宝马贵者趣，公交自行程序员。
         *                   别人笑我忒疯癫，我笑自己命太贱；
         *                   不见满街漂亮妹，哪个归得程序员？
*/

        #region 选择或去除要加密的文件
        void selectFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "选择要混淆的.Net App";
            ofd.Filter = "所支持的文件|*.dll;*.exe|dll文件(*.dll)|*.dll|exe文件(*.exe)|*.exe";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                bool add = true;
                try
                {
                    for (int i = 0; i < FileName.Count(); i++)
                    {
                        if (FileName[i].ToString() == ofd.FileName)
                        {
                            add = false;
                        }
                    }
                }
                catch { add = true; }
                if (add == true)
                {
                    try
                    {
                        var assembly = AssemblyDefinition.ReadAssembly(ofd.FileName);
                        var pe = Runtime.GetPEFileKinds(ofd.FileName);
                        ListViewItem listViewItem = new ListViewItem();
                        listViewItem.Text = ofd.FileName;
                        listViewItem.SubItems.Add(assembly.MainModule.Name);
                        listViewItem.SubItems.Add(pe.ToString());
                        foreach (var module in assembly.Modules)
                        {
                            foreach (var Res in module.Resources)
                            {
                                if (Res.Name == "XiaowuStudioObfuscator\u00A0\u0020\u3000\u00A0\u0020\u3000")
                                {
                                    add = false;
                                    MessageBox.Show("无法混淆已经混淆过的.Net项目！", "XiaowuStudioObfuscator提醒", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        if(add == true)
                        {
                            treeView1.Nodes.Add(assembly.MainModule.Name);
                            treeView2.Nodes.Add(assembly.MainModule.Name);
                            treeView3.Nodes.Add(assembly.MainModule.Name);
                            treeView4.Nodes.Add(assembly.MainModule.Name);
                            foreach (var module in assembly.Modules)
                            {
                                foreach (var type in module.Types)
                                {
                                    if (pe != System.Reflection.Emit.PEFileKinds.Dll)
                                    {
                                        treeView1.Nodes[treeView1.Nodes.Count - 1].Nodes.Add(type.Name);
                                    }
                                    foreach (var property in type.Properties)
                                    {
                                        treeView2.Nodes[treeView2.Nodes.Count - 1].Nodes.Add(property.Name);
                                    }
                                    foreach (var filed in type.Fields)
                                    {
                                        treeView3.Nodes[treeView3.Nodes.Count - 1].Nodes.Add(filed.Name);
                                    }
                                    foreach (var method in type.Methods)
                                    {
                                        treeView4.Nodes[treeView4.Nodes.Count - 1].Nodes.Add(method.Name);
                                    }
                                }
                            }
                            FileInfo file = new FileInfo(ofd.FileName);
                            listViewItem.SubItems.Add(file.LastWriteTime.ToString("yyyy.MM.dd HH:mm:ss (zzz)"));
                            listView1.Items.Add(listViewItem);
                            FileName.Add(ofd.FileName);
                        }
                    }
                    catch { }
                }
            }
        }
        private void toolStripButton9_Click(object sender, EventArgs e) => selectFile();
        private void toolStripButton1_Click(object sender, EventArgs e) => selectFile();
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            if (listView1.FocusedItem != null)
            {
                FileName.Remove(listView1.FocusedItem.Text);
                listView1.FocusedItem.Remove();
            }
        }
        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;//调用DragDrop事件
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filePath.Count() == 1)
            {
                bool add = true;
                try
                {
                    for (int i = 0; i < FileName.Count(); i++)
                    {
                        if (FileName[i].ToString() == filePath[0])
                        {
                            add = false;
                            MessageBox.Show("您已添加该项目！！！");
                        }
                    }
                }
                catch { add = true; }
                if (add == true)
                {
                    try
                    {
                        var assembly = AssemblyDefinition.ReadAssembly(filePath[0]);
                        var pe = Runtime.GetPEFileKinds(filePath[0]);
                        ListViewItem listViewItem = new ListViewItem();
                        listViewItem.Text = filePath[0];
                        listViewItem.SubItems.Add(assembly.MainModule.Name);
                        listViewItem.SubItems.Add(pe.ToString());
                        foreach (var module in assembly.Modules)
                        {
                            foreach (var Res in module.Resources)
                            {
                                if (Res.Name == "XiaowuStudioObfuscator\u00A0\u0020\u3000\u00A0\u0020\u3000")
                                {
                                    add = false;
                                    MessageBox.Show("无法混淆已经混淆过的.Net项目！", "XiaowuStudioObfuscator提醒", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        if (add == true)
                        {
                            treeView1.Nodes.Add(assembly.MainModule.Name);
                            treeView2.Nodes.Add(assembly.MainModule.Name);
                            treeView3.Nodes.Add(assembly.MainModule.Name);
                            treeView4.Nodes.Add(assembly.MainModule.Name);
                            foreach (var module in assembly.Modules)
                            {
                                foreach (var type in module.Types)
                                {
                                    if (pe != System.Reflection.Emit.PEFileKinds.Dll)
                                    {
                                        treeView1.Nodes[treeView1.Nodes.Count - 1].Nodes.Add(type.Name);
                                    }
                                    foreach (var property in type.Properties)
                                    {
                                        treeView2.Nodes[treeView2.Nodes.Count - 1].Nodes.Add(property.Name);
                                    }
                                    foreach (var filed in type.Fields)
                                    {
                                        treeView3.Nodes[treeView3.Nodes.Count - 1].Nodes.Add(filed.Name);
                                    }
                                    foreach (var method in type.Methods)
                                    {
                                        treeView4.Nodes[treeView4.Nodes.Count - 1].Nodes.Add(method.Name);
                                    }
                                }
                            }
                            FileInfo file = new FileInfo(filePath[0]);
                            listViewItem.SubItems.Add(file.LastWriteTime.ToString("yyyy.MM.dd HH:mm:ss (zzz)"));
                            listView1.Items.Add(listViewItem);
                            FileName.Add(filePath[0]);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("请添加正确的.Net项目！！！");
                    }
                }
            }
            else
            {
                for (int a = 0; a < filePath.Count(); a++)
                {
                    bool add = true;
                    try
                    {
                        for (int i = 0; i < FileName.Count(); i++)
                        {
                            if (FileName[i].ToString() == filePath[a])
                            {
                                add = false;
                            }
                        }
                    }
                    catch { add = true; }
                Begin:
                    if (add == true)
                    {
                        try
                        {
                            var assembly = AssemblyDefinition.ReadAssembly(filePath[a]);
                            var pe = Runtime.GetPEFileKinds(filePath[a]);
                            ListViewItem listViewItem = new ListViewItem();
                            listViewItem.Text = filePath[a];
                            listViewItem.SubItems.Add(assembly.MainModule.Name);
                            listViewItem.SubItems.Add(pe.ToString());
                            foreach (var module in assembly.Modules)
                            {
                                foreach (var Res in module.Resources)
                                {
                                    if (Res.Name == "XiaowuStudioObfuscator\u00A0\u0020\u3000\u00A0\u0020\u3000")
                                    {
                                        add = false;
                                        goto Begin;
                                    }
                                }
                            }
                            FileInfo file = new FileInfo(filePath[a]);
                            listViewItem.SubItems.Add(file.LastWriteTime.ToString("yyyy.MM.dd HH:mm:ss (zzz)"));
                            listView1.Items.Add(listViewItem);
                            FileName.Add(filePath[a]);
                        }
                        catch { }
                    }
                }
            }
        }
        #endregion

        #region 生成按钮与打破生成按钮
        void Build()
        {
            Invoke(new MethodInvoker(() =>
            {
                for (int i = 0; i < tabPage1.Controls.Count; i++)
                {
                    tabControl2.Controls[i].Enabled = false;
                }
                for (int i = 0; i < tabPage2.Controls.Count; i++)
                {
                    tabPage2.Controls[i].Enabled = false;
                }
                tabControl1.SelectTab(tabPage3);
            }));
            for (int i = 0; i < FileName.Count; i++)
            {
                Invoke(new MethodInvoker(() =>
                {
                    richTextBoxEx1.AppendText("-------------------- 第" + (i + 1) + "/" + FileName.Count + "个程序正在混淆处理 --------------------" + "\r\n");
                }));
                thread = new Thread(new ParameterizedThreadStart(Obf));
                thread.Start(FileName[i].ToString());
                Invoke(new MethodInvoker(() =>
                {
                    richTextBoxEx1.AppendText("-------------------- 第" + (i + 1) + "/" + FileName.Count + "个程序混淆处理完毕 --------------------" + "\r\n");
                }));
            }
            Invoke(new MethodInvoker(() =>
            {
                for (int i = 0; i < tabPage1.Controls.Count; i++)
                {
                    tabControl2.Controls[i].Enabled = true;
                }
                for (int i = 0; i < tabPage2.Controls.Count; i++)
                {
                    tabPage2.Controls[i].Enabled = true;
                }
                listView1.Items.Clear();
                FileName.Clear();
            }));
        }
        private void 生成开始混淆ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(Build);
            thread.Start();
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(Build);
            thread.Start();
        }
        private void 打破生成中断混淆ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (thread != null)
            {
                thread.Abort();
                richTextBoxEx1.AppendText("已打断混淆！！");
            }
        }
        #endregion

        #region Obf核心
        enum ObfMode
        {
            Hexadecimal,
            Unicode,
            Space,
            Number,
            UUID,
            RandomString,
            Alphabets,
            EncryptionString
        }
        static string ObfString(ObfMode obfMode, int length, string Data)
        {
            string RandomResult = "";
            if (obfMode == ObfMode.RandomString)
            {
                string Ranstr = @"1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ~@#$%^&*()_+`-={}:|<>?[];',/";
                Random r = new Random();
                for (int i = 0; i < length; i++)
                {
                    int m = r.Next(0, 90);
                    string s = Ranstr.Substring(m, 1);
                    RandomResult += s;
                }
            }
            else if (obfMode == ObfMode.Alphabets)
            {
                string Ranstr = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                Random r = new Random();
                for (int i = 0; i < length; i++)
                {
                    int m = r.Next(0, 52);
                    string s = Ranstr.Substring(m, 1);
                    RandomResult += s;
                }
            }
            else if (obfMode == ObfMode.Number)
            {
                string Ranstr = "1234567890";
                Random r = new Random();
                for (int i = 0; i < length; i++)
                {
                    int m = r.Next(0, 10);
                    string s = Ranstr.Substring(m, 1);
                    RandomResult += s;
                }
            }
            else if (obfMode == ObfMode.Space)
            {
                string Ranstr = "\u00A0\u0020\u3000";
                Random r = new Random();
                for (int i = 0; i < length; i++)
                {
                    int m = r.Next(0, 3);
                    string s = Ranstr.Substring(m, 1);
                    RandomResult += s;
                }
            }
            else if (obfMode == ObfMode.Hexadecimal)
            {
                Random random = new Random();
                int digits = length;
                byte[] buffer = new byte[digits / 2];
                random.NextBytes(buffer);
                string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
                RandomResult = result + random.Next(16).ToString("X");
                var bytes = new byte[RandomResult.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(RandomResult.Substring(i * 2, 2), 16);
                }

                RandomResult = Encoding.Unicode.GetString(bytes);
            }
            else if (obfMode == ObfMode.UUID)
            {
                RandomResult = Guid.NewGuid().ToString();
            }
            else if (obfMode == ObfMode.EncryptionString)
            {
                string Key = "Vm0xd1EyRXlUWGxTYTJoVVYwaENhRlZyVm1GV01WSlZVbXR3YkZKdFVubFhhMmhQWVRKS1IxSnFWbFpOYWtJMFdWUkdhMUpyTlZsU2JHaFhZbGRvVlZkclkzaFVNa3B6VVd4V1RsSkVRVGs9";
                MemoryStream mStream = new MemoryStream();
                RijndaelManaged aes = new RijndaelManaged();

                byte[] plainBytes = Encoding.UTF8.GetBytes(Data);
                byte[] bKey = new byte[32];
                Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);

                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                //aes.Key = _key;  
                aes.Key = bKey;
                //aes.IV = _iV;  
                CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                try
                {
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    RandomResult = Convert.ToBase64String(mStream.ToArray());
                }
                finally
                {
                    cryptoStream.Close();
                    mStream.Close();
                    aes.Clear();
                }
            }
            else if (obfMode == ObfMode.Unicode)
            {
                Random random = new Random();
                int digits = length;
                byte[] buffer = new byte[digits / 2];
                random.NextBytes(buffer);
                string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
                RandomResult = result + random.Next(16).ToString("X4");
                var bytes = new byte[RandomResult.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(RandomResult.Substring(i * 2, 2), 16);
                }

                RandomResult = Encoding.Unicode.GetString(bytes);
            }
            return RandomResult;
        }
        static byte[] encode(string s, byte c)
        {
            byte[] b = System.Text.Encoding.UTF8.GetBytes(s);
            for (int i = 0; i < b.Length; i++)
                b[i] += c;
            return b;
        }
        private static MethodDefinition getxwsobfr(AssemblyDefinition assembly, int c)
        {
            MethodDefinition md = new MethodDefinition("xwsobfr", MethodAttributes.Static, assembly.MainModule.TypeSystem.String);
            md.Parameters.Add(new ParameterDefinition(assembly.MainModule.TypeSystem.String));

            md.Body.Variables.Add(new VariableDefinition("assembly", assembly.MainModule.Import(typeof(System.Reflection.Assembly))));
            md.Body.Variables.Add(new VariableDefinition("stream", assembly.MainModule.Import(typeof(System.IO.Stream))));
            md.Body.Variables.Add(new VariableDefinition("buffer", assembly.MainModule.Import(typeof(byte[]))));
            md.Body.Variables.Add(new VariableDefinition("num", assembly.MainModule.TypeSystem.Int32));
            VariableDefinition varstr = new VariableDefinition("str", assembly.MainModule.TypeSystem.String);
            md.Body.Variables.Add(varstr);
            VariableDefinition varflag = new VariableDefinition("flag", assembly.MainModule.TypeSystem.Boolean);
            md.Body.Variables.Add(varflag);
            md.Body.Variables.Add(new VariableDefinition("a", assembly.MainModule.Import(typeof(System.Reflection.Assembly))));
            md.Body.Variables.Add(new VariableDefinition("b", assembly.MainModule.Import(typeof(System.IO.Stream))));
            md.Body.Variables.Add(new VariableDefinition("c", assembly.MainModule.Import(typeof(byte[]))));
            md.Body.Variables.Add(new VariableDefinition("d", assembly.MainModule.TypeSystem.Int32));

            var mworker = md.Body.GetILProcessor();
            md.Body.Instructions.Add(mworker.Create(OpCodes.Nop));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Call, assembly.MainModule.Import(typeof(System.Reflection.Assembly).GetMethod("GetExecutingAssembly"))));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Stloc_0));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_0));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldarg_0));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Callvirt, assembly.MainModule.Import(typeof(System.Reflection.Assembly).GetMethod("GetManifestResourceStream", new Type[] { typeof(string) }))));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Stloc_1));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Nop));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_1));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Callvirt, assembly.MainModule.Import(typeof(System.IO.Stream).GetMethod("get_Length"))));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Conv_Ovf_I));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Newarr, assembly.MainModule.TypeSystem.Byte));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Stloc_2));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_1));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_2));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldc_I4_0));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_2));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldlen));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Conv_I4));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Callvirt, assembly.MainModule.Import(typeof(System.IO.Stream).GetMethod("Read", new Type[] { typeof(byte[]), typeof(int), typeof(int) }))));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Pop));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldc_I4_0));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Stloc_3));
            var L0046 = mworker.Create(OpCodes.Ldloc_3);
            md.Body.Instructions.Add(mworker.Create(OpCodes.Br_S, L0046));
            var L002d = mworker.Create(OpCodes.Ldloc_2);
            md.Body.Instructions.Add(L002d);
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_3));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldelema, assembly.MainModule.TypeSystem.Byte));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Dup));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldobj, assembly.MainModule.TypeSystem.Byte));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldc_I4, c));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Sub));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Conv_U1));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Stobj, assembly.MainModule.TypeSystem.Byte));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_3));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldc_I4_1));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Add));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Stloc_3));
            md.Body.Instructions.Add(L0046);
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_2));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldlen));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Conv_I4));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Clt));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Stloc_S, varflag));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_S, varflag));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Brtrue_S, L002d));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Call, assembly.MainModule.Import(typeof(System.Text.Encoding).GetMethod("get_UTF8"))));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_2));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Callvirt, assembly.MainModule.Import(typeof(System.Text.Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }))));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Stloc_S, varstr));
            var L0073 = mworker.Create(OpCodes.Nop);
            md.Body.Instructions.Add(mworker.Create(OpCodes.Leave_S, L0073));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_1));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldnull));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ceq));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Stloc_S, varflag));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_S, varflag));
            var L0072 = mworker.Create(OpCodes.Endfinally);
            md.Body.Instructions.Add(mworker.Create(OpCodes.Brtrue_S, L0072));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_1));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Callvirt, assembly.MainModule.Import(typeof(System.IDisposable).GetMethod("Dispose"))));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Nop));
            md.Body.Instructions.Add(L0072);
            md.Body.Instructions.Add(L0073);
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ldloc_S, varstr));
            md.Body.Instructions.Add(mworker.Create(OpCodes.Ret));
            return md;
        }
        void Obf(object DotNetFilePath)
        {
            var assembly = AssemblyDefinition.ReadAssembly(DotNetFilePath.ToString());
            var pe = Runtime.GetPEFileKinds(DotNetFilePath.ToString());
            Invoke(new MethodInvoker(() =>
            {
                richTextBoxEx1.AppendText("文件路径：" + DotNetFilePath + "\r\n");
                richTextBoxEx1.AppendText("程序集名称：" + assembly.MainModule.Name + "\r\n");
                richTextBoxEx1.AppendText("PE头：" + pe.ToString() + "\r\n");
            }));

            EmbeddedResource erTemp = new EmbeddedResource("XiaowuStudioObfuscator\u00A0\u0020\u3000\u00A0\u0020\u3000", ManifestResourceAttributes.Public, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 });
            assembly.MainModule.Resources.Add(erTemp);
            Invoke(new MethodInvoker(() =>
            {
                richTextBoxEx1.AppendText("已在资源嵌入XiaowuStudioObfuscator防伪标记" + "\r\n");
            }));

            foreach (var module in assembly.Modules)
            {
                Invoke(new MethodInvoker(() =>
                {
                    richTextBoxEx1.AppendText("模块名：" + assembly.ToString() + "\r\n");
                }));

                foreach (var type in module.Types)
                {
                    Invoke(new MethodInvoker(() =>
                    {
                        richTextBoxEx1.AppendText("类型名：" + type.FullName + "\r\n");
                    }));

                    //注入方法定义
                    Random ran = new Random();
                    int c = ran.Next(1, 10);
                    var md = getxwsobfr(assembly, c);
                    type.Methods.Add(md);
                    Invoke(new MethodInvoker(() =>
                    {
                        richTextBoxEx1.AppendText("已注入方法定义！" + "\r\n");
                    }));

                    //枚举命名空间
                    if (checkBox1.Checked == true)
                    {
                        Invoke(new MethodInvoker(() =>
                        {
                            richTextBoxEx1.AppendText("命名空间：" + type.Namespace + " => ");
                        }));

                        string obfnamespace = "";
                        if (radioButton1.Checked == true) obfnamespace = ObfString(ObfMode.Hexadecimal, int.Parse(textBox1.Text), type.Namespace);
                        else if (radioButton2.Checked == true) obfnamespace = ObfString(ObfMode.Unicode, int.Parse(textBox1.Text), type.Namespace);
                        else if (radioButton3.Checked == true) obfnamespace = ObfString(ObfMode.Space, int.Parse(textBox1.Text), type.Namespace);
                        else if (radioButton4.Checked == true) obfnamespace = ObfString(ObfMode.Number, int.Parse(textBox1.Text), type.Namespace);
                        else if (radioButton5.Checked == true) obfnamespace = ObfString(ObfMode.UUID, int.Parse(textBox1.Text), type.Namespace);
                        else if (radioButton6.Checked == true) obfnamespace = ObfString(ObfMode.RandomString, int.Parse(textBox1.Text), type.Namespace);
                        else if (radioButton7.Checked == true) obfnamespace = ObfString(ObfMode.Alphabets, int.Parse(textBox1.Text), type.Namespace);
                        else if (radioButton8.Checked == true) obfnamespace = ObfString(ObfMode.EncryptionString, int.Parse(textBox1.Text), type.Namespace);
                        List<string> ResName = new List<string>();
                        bool cando = true;
                        foreach (var Res in module.Resources)
                        {
                            ResName.Add(Res.Name);
                        }
                        for (int i = 0; i < ResName.Count; i++)
                        {
                            if (ResName[i].ToString().Contains(type.Name))
                            {
                                cando = false;
                            }
                        }
                        if (cando == true)
                        {
                            type.Namespace = obfnamespace;
                        }
                        Invoke(new MethodInvoker(() =>
                        {
                            richTextBoxEx1.AppendText(obfnamespace + "\r\n");
                        }));

                    }

                    //枚举方法
                    if (checkBox2.Checked == true)
                    {
                        foreach (var method in type.Methods)
                        {
                            Invoke(new MethodInvoker(() =>
                            {
                                richTextBoxEx1.AppendText("方法全名：" + method.FullName + "\r\n");
                            }));

                            //混淆form的Name属性
                            if (method.Name == "InitializeComponent" || method.Name == "xwsobfr" || method.Name == "__ENCAddToList")
                            {
                                var worker = method.Body.GetILProcessor();

                                var list = method.Body.Instructions.Where(i => i.Operand != null && i.Operand.ToString().Contains("System.Windows.Forms.Control::set_Name")).ToList();
                                list.ForEach(i =>
                                {
                                    if (checkBox3.Checked)
                                    {
                                        List<Instruction> ilist = new List<Instruction>();
                                        //查找前导符
                                        while (i.OpCode.Name != "ldarg.0")
                                        {
                                            ilist.Add(i);
                                            i = i.Previous;
                                        }
                                        ilist.Add(i);

                                        foreach (var it in ilist)
                                        {
                                            worker.Remove(it);
                                        }
                                    }
                                    else
                                    {
                                        i.Previous.Operand = "xwsobf";
                                    }
                                });
                            }
                            else
                            {
                                //系统方法、关键字、构造器不混淆
                                if (!method.IsConstructor && !method.IsRuntime && !method.IsRuntimeSpecialName && !method.IsSpecialName && !method.IsVirtual && !method.IsAbstract && method.Overrides.Count <= 0 && !method.Name.StartsWith("<") && !method.IsPublic)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("方法：" + method.Name + " => ");
                                    }));

                                    string obfmethod = "";
                                    if (radioButton1.Checked == true) obfmethod = ObfString(ObfMode.Hexadecimal, int.Parse(textBox2.Text), method.Name);
                                    else if (radioButton2.Checked == true) obfmethod = ObfString(ObfMode.Unicode, int.Parse(textBox2.Text), method.Name);
                                    else if (radioButton3.Checked == true) obfmethod = ObfString(ObfMode.Space, int.Parse(textBox2.Text), method.Name);
                                    else if (radioButton4.Checked == true) obfmethod = ObfString(ObfMode.Number, int.Parse(textBox2.Text), method.Name);
                                    else if (radioButton5.Checked == true) obfmethod = ObfString(ObfMode.UUID, int.Parse(textBox2.Text), method.Name);
                                    else if (radioButton6.Checked == true) obfmethod = ObfString(ObfMode.RandomString, int.Parse(textBox2.Text), method.Name);
                                    else if (radioButton7.Checked == true) obfmethod = ObfString(ObfMode.Alphabets, int.Parse(textBox2.Text), method.Name);
                                    else if (radioButton8.Checked == true) obfmethod = ObfString(ObfMode.EncryptionString, int.Parse(textBox2.Text), method.Name);
                                    method.Name = obfmethod;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obfmethod + "\r\n");
                                    }));
                                }
                                if (checkBox4.Checked == true)
                                {
                                    if (method.IsPublic)
                                    {
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText("方法：" + method.Name + " => ");
                                        }));

                                        string obfmethod = "";
                                        if (radioButton1.Checked == true) obfmethod = ObfString(ObfMode.Hexadecimal, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton2.Checked == true) obfmethod = ObfString(ObfMode.Unicode, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton3.Checked == true) obfmethod = ObfString(ObfMode.Space, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton4.Checked == true) obfmethod = ObfString(ObfMode.Number, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton5.Checked == true) obfmethod = ObfString(ObfMode.UUID, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton6.Checked == true) obfmethod = ObfString(ObfMode.RandomString, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton7.Checked == true) obfmethod = ObfString(ObfMode.Alphabets, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton8.Checked == true) obfmethod = ObfString(ObfMode.EncryptionString, int.Parse(textBox2.Text), method.Name);
                                        method.Name = obfmethod;
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText(obfmethod + "\r\n");
                                        }));
                                    }
                                }
                                if (checkBox20.Checked == true)
                                {
                                    if (method.IsRuntime)
                                    {
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText("方法：" + method.Name + " => ");
                                        }));

                                        string obfmethod = "";
                                        if (radioButton1.Checked == true) obfmethod = ObfString(ObfMode.Hexadecimal, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton2.Checked == true) obfmethod = ObfString(ObfMode.Unicode, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton3.Checked == true) obfmethod = ObfString(ObfMode.Space, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton4.Checked == true) obfmethod = ObfString(ObfMode.Number, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton5.Checked == true) obfmethod = ObfString(ObfMode.UUID, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton6.Checked == true) obfmethod = ObfString(ObfMode.RandomString, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton7.Checked == true) obfmethod = ObfString(ObfMode.Alphabets, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton8.Checked == true) obfmethod = ObfString(ObfMode.EncryptionString, int.Parse(textBox2.Text), method.Name);
                                        method.Name = obfmethod;
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText(obfmethod + "\r\n");
                                        }));
                                    }
                                }
                                if (checkBox21.Checked == true)
                                {
                                    if (method.IsRuntimeSpecialName)
                                    {
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText("方法：" + method.Name + " => ");
                                        }));

                                        string obfmethod = "";
                                        if (radioButton1.Checked == true) obfmethod = ObfString(ObfMode.Hexadecimal, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton2.Checked == true) obfmethod = ObfString(ObfMode.Unicode, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton3.Checked == true) obfmethod = ObfString(ObfMode.Space, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton4.Checked == true) obfmethod = ObfString(ObfMode.Number, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton5.Checked == true) obfmethod = ObfString(ObfMode.UUID, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton6.Checked == true) obfmethod = ObfString(ObfMode.RandomString, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton7.Checked == true) obfmethod = ObfString(ObfMode.Alphabets, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton8.Checked == true) obfmethod = ObfString(ObfMode.EncryptionString, int.Parse(textBox2.Text), method.Name);
                                        method.Name = obfmethod;
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText(obfmethod + "\r\n");
                                        }));
                                    }
                                }
                                if (checkBox22.Checked == true)
                                {
                                    if (method.IsSpecialName)
                                    {
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText("方法：" + method.Name + " => ");
                                        }));

                                        string obfmethod = "";
                                        if (radioButton1.Checked == true) obfmethod = ObfString(ObfMode.Hexadecimal, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton2.Checked == true) obfmethod = ObfString(ObfMode.Unicode, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton3.Checked == true) obfmethod = ObfString(ObfMode.Space, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton4.Checked == true) obfmethod = ObfString(ObfMode.Number, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton5.Checked == true) obfmethod = ObfString(ObfMode.UUID, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton6.Checked == true) obfmethod = ObfString(ObfMode.RandomString, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton7.Checked == true) obfmethod = ObfString(ObfMode.Alphabets, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton8.Checked == true) obfmethod = ObfString(ObfMode.EncryptionString, int.Parse(textBox2.Text), method.Name);
                                        method.Name = obfmethod;
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText(obfmethod + "\r\n");
                                        }));
                                    }
                                }
                                if (checkBox23.Checked == true)
                                {
                                    if (method.IsVirtual)
                                    {
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText("方法：" + method.Name + " => ");
                                        }));

                                        string obfmethod = "";
                                        if (radioButton1.Checked == true) obfmethod = ObfString(ObfMode.Hexadecimal, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton2.Checked == true) obfmethod = ObfString(ObfMode.Unicode, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton3.Checked == true) obfmethod = ObfString(ObfMode.Space, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton4.Checked == true) obfmethod = ObfString(ObfMode.Number, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton5.Checked == true) obfmethod = ObfString(ObfMode.UUID, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton6.Checked == true) obfmethod = ObfString(ObfMode.RandomString, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton7.Checked == true) obfmethod = ObfString(ObfMode.Alphabets, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton8.Checked == true) obfmethod = ObfString(ObfMode.EncryptionString, int.Parse(textBox2.Text), method.Name);
                                        method.Name = obfmethod;
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText(obfmethod + "\r\n");
                                        }));
                                    }
                                }
                                if (checkBox24.Checked == true)
                                {
                                    if (method.IsAbstract)
                                    {
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText("方法：" + method.Name + " => ");
                                        }));

                                        string obfmethod = "";
                                        if (radioButton1.Checked == true) obfmethod = ObfString(ObfMode.Hexadecimal, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton2.Checked == true) obfmethod = ObfString(ObfMode.Unicode, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton3.Checked == true) obfmethod = ObfString(ObfMode.Space, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton4.Checked == true) obfmethod = ObfString(ObfMode.Number, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton5.Checked == true) obfmethod = ObfString(ObfMode.UUID, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton6.Checked == true) obfmethod = ObfString(ObfMode.RandomString, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton7.Checked == true) obfmethod = ObfString(ObfMode.Alphabets, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton8.Checked == true) obfmethod = ObfString(ObfMode.EncryptionString, int.Parse(textBox2.Text), method.Name);
                                        method.Name = obfmethod;
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText(obfmethod + "\r\n");
                                        }));
                                    }
                                }
                                if (checkBox25.Checked == true)
                                {
                                    if (method.Name.StartsWith("<"))
                                    {
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText("方法：" + method.Name + " => ");
                                        }));

                                        string obfmethod = "";
                                        if (radioButton1.Checked == true) obfmethod = ObfString(ObfMode.Hexadecimal, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton2.Checked == true) obfmethod = ObfString(ObfMode.Unicode, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton3.Checked == true) obfmethod = ObfString(ObfMode.Space, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton4.Checked == true) obfmethod = ObfString(ObfMode.Number, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton5.Checked == true) obfmethod = ObfString(ObfMode.UUID, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton6.Checked == true) obfmethod = ObfString(ObfMode.RandomString, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton7.Checked == true) obfmethod = ObfString(ObfMode.Alphabets, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton8.Checked == true) obfmethod = ObfString(ObfMode.EncryptionString, int.Parse(textBox2.Text), method.Name);
                                        method.Name = obfmethod;
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText(obfmethod + "\r\n");
                                        }));
                                    }
                                }
                                if (checkBox26.Checked == true)
                                {
                                    if (method.IsConstructor)
                                    {
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText("方法：" + method.Name + " => ");
                                        }));

                                        string obfmethod = "";
                                        if (radioButton1.Checked == true) obfmethod = ObfString(ObfMode.Hexadecimal, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton2.Checked == true) obfmethod = ObfString(ObfMode.Unicode, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton3.Checked == true) obfmethod = ObfString(ObfMode.Space, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton4.Checked == true) obfmethod = ObfString(ObfMode.Number, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton5.Checked == true) obfmethod = ObfString(ObfMode.UUID, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton6.Checked == true) obfmethod = ObfString(ObfMode.RandomString, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton7.Checked == true) obfmethod = ObfString(ObfMode.Alphabets, int.Parse(textBox2.Text), method.Name);
                                        else if (radioButton8.Checked == true) obfmethod = ObfString(ObfMode.EncryptionString, int.Parse(textBox2.Text), method.Name);
                                        method.Name = obfmethod;
                                        Invoke(new MethodInvoker(() =>
                                        {
                                            richTextBoxEx1.AppendText(obfmethod + "\r\n");
                                        }));
                                    }
                                }
                            }
                            if (method.Body != null)
                            {
                                var strilist = method.Body.Instructions.Where(i => i.OpCode.Name == "ldstr").ToList();
                                strilist.ForEach(i =>
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("字符串：" + i.Operand.ToString() + "\r\n");
                                    }));
                                    string obfoperand = "";
                                    if (radioButton1.Checked == true) obfoperand = ObfString(ObfMode.Hexadecimal, int.Parse(textBox3.Text), i.Operand.ToString());
                                    else if (radioButton2.Checked == true) obfoperand = ObfString(ObfMode.Unicode, int.Parse(textBox3.Text), i.Operand.ToString());
                                    else if (radioButton3.Checked == true) obfoperand = ObfString(ObfMode.Space, int.Parse(textBox3.Text), i.Operand.ToString());
                                    else if (radioButton4.Checked == true) obfoperand = ObfString(ObfMode.Number, int.Parse(textBox3.Text), i.Operand.ToString());
                                    else if (radioButton5.Checked == true) obfoperand = ObfString(ObfMode.UUID, int.Parse(textBox3.Text), i.Operand.ToString());
                                    else if (radioButton6.Checked == true) obfoperand = ObfString(ObfMode.RandomString, int.Parse(textBox3.Text), i.Operand.ToString());
                                    else if (radioButton7.Checked == true) obfoperand = ObfString(ObfMode.Alphabets, int.Parse(textBox3.Text), i.Operand.ToString());
                                    else if (radioButton8.Checked == true) obfoperand = ObfString(ObfMode.EncryptionString, int.Parse(textBox3.Text), i.Operand.ToString());
                                    EmbeddedResource erTmp = new EmbeddedResource(obfoperand, ManifestResourceAttributes.Private, encode(i.Operand.ToString(), (byte)c));
                                    assembly.MainModule.Resources.Add(erTmp);
                                    i.Operand = obfoperand;
                                    var worker = method.Body.GetILProcessor();
                                    worker.InsertAfter(i, worker.Create(OpCodes.Call, md));
                                });
                            }
                        }
                    }

                    //枚举字段
                    if (checkBox5.Checked == true)
                    {
                        foreach (var field in type.Fields)
                        {
                            if (checkBox8.Checked == true)
                            {
                                Invoke(new MethodInvoker(() =>
                                {
                                    richTextBoxEx1.AppendText("字段：" + field.Name + " => ");
                                }));
                                string obffield = "";
                                if (radioButton1.Checked == true) obffield = ObfString(ObfMode.Hexadecimal, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton2.Checked == true) obffield = ObfString(ObfMode.Unicode, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton3.Checked == true) obffield = ObfString(ObfMode.Space, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton4.Checked == true) obffield = ObfString(ObfMode.Number, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton5.Checked == true) obffield = ObfString(ObfMode.UUID, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton6.Checked == true) obffield = ObfString(ObfMode.RandomString, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton7.Checked == true) obffield = ObfString(ObfMode.Alphabets, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton8.Checked == true) obffield = ObfString(ObfMode.EncryptionString, int.Parse(textBox4.Text), field.Name);
                                field.Name = obffield;
                                Invoke(new MethodInvoker(() =>
                                {
                                    richTextBoxEx1.AppendText(obffield + "\r\n");
                                }));

                            }
                            else if (!field.IsPublic)
                            {
                                Invoke(new MethodInvoker(() =>
                                {
                                    richTextBoxEx1.AppendText("字段：" + field.Name + " => ");
                                }));

                                string obffield = "";
                                if (radioButton1.Checked == true) obffield = ObfString(ObfMode.Hexadecimal, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton2.Checked == true) obffield = ObfString(ObfMode.Unicode, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton3.Checked == true) obffield = ObfString(ObfMode.Space, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton4.Checked == true) obffield = ObfString(ObfMode.Number, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton5.Checked == true) obffield = ObfString(ObfMode.UUID, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton6.Checked == true) obffield = ObfString(ObfMode.RandomString, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton7.Checked == true) obffield = ObfString(ObfMode.Alphabets, int.Parse(textBox4.Text), field.Name);
                                else if (radioButton8.Checked == true) obffield = ObfString(ObfMode.EncryptionString, int.Parse(textBox4.Text), field.Name);
                                field.Name = obffield;
                                Invoke(new MethodInvoker(() =>
                                {
                                    richTextBoxEx1.AppendText(obffield + "\r\n");
                                }));
                            }
                        }
                    }

                    //枚举属性
                    if (checkBox6.Checked == true)
                    {
                        foreach (var property in type.Properties)
                        {
                            if (property.GetMethod != null && property.SetMethod != null && !property.GetMethod.IsPublic && !property.SetMethod.IsPublic)
                            {
                                Invoke(new MethodInvoker(() =>
                                {
                                    richTextBoxEx1.AppendText("属性：" + property.Name + " => ");
                                }));

                                string obfproperty = "";
                                if (radioButton1.Checked == true) obfproperty = ObfString(ObfMode.Hexadecimal, int.Parse(textBox4.Text), property.Name);
                                else if (radioButton2.Checked == true) obfproperty = ObfString(ObfMode.Unicode, int.Parse(textBox4.Text), property.Name);
                                else if (radioButton3.Checked == true) obfproperty = ObfString(ObfMode.Space, int.Parse(textBox4.Text), property.Name);
                                else if (radioButton4.Checked == true) obfproperty = ObfString(ObfMode.Number, int.Parse(textBox4.Text), property.Name);
                                else if (radioButton5.Checked == true) obfproperty = ObfString(ObfMode.UUID, int.Parse(textBox4.Text), property.Name);
                                else if (radioButton6.Checked == true) obfproperty = ObfString(ObfMode.RandomString, int.Parse(textBox4.Text), property.Name);
                                else if (radioButton7.Checked == true) obfproperty = ObfString(ObfMode.Alphabets, int.Parse(textBox4.Text), property.Name);
                                else if (radioButton8.Checked == true) obfproperty = ObfString(ObfMode.EncryptionString, int.Parse(textBox4.Text), property.Name);
                                property.Name = obfproperty;
                                Invoke(new MethodInvoker(() =>
                                {
                                    richTextBoxEx1.AppendText(obfproperty + "\r\n");
                                }));

                            }
                            if (checkBox9.Checked == true)
                            {
                                if (property.GetMethod == null)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("属性：" + property.Name + " => ");
                                    }));
                                    string obfproperty = "";
                                    if (radioButton1.Checked == true) obfproperty = ObfString(ObfMode.Hexadecimal, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton2.Checked == true) obfproperty = ObfString(ObfMode.Unicode, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton3.Checked == true) obfproperty = ObfString(ObfMode.Space, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton4.Checked == true) obfproperty = ObfString(ObfMode.Number, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton5.Checked == true) obfproperty = ObfString(ObfMode.UUID, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton6.Checked == true) obfproperty = ObfString(ObfMode.RandomString, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton7.Checked == true) obfproperty = ObfString(ObfMode.Alphabets, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton8.Checked == true) obfproperty = ObfString(ObfMode.EncryptionString, int.Parse(textBox4.Text), property.Name);
                                    property.Name = obfproperty;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obfproperty + "\r\n");
                                    }));
                                }
                            }
                            if (checkBox17.Checked == true)
                            {
                                if (property.SetMethod == null)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("属性：" + property.Name + " => ");
                                    }));
                                    string obfproperty = "";
                                    if (radioButton1.Checked == true) obfproperty = ObfString(ObfMode.Hexadecimal, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton2.Checked == true) obfproperty = ObfString(ObfMode.Unicode, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton3.Checked == true) obfproperty = ObfString(ObfMode.Space, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton4.Checked == true) obfproperty = ObfString(ObfMode.Number, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton5.Checked == true) obfproperty = ObfString(ObfMode.UUID, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton6.Checked == true) obfproperty = ObfString(ObfMode.RandomString, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton7.Checked == true) obfproperty = ObfString(ObfMode.Alphabets, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton8.Checked == true) obfproperty = ObfString(ObfMode.EncryptionString, int.Parse(textBox4.Text), property.Name);
                                    property.Name = obfproperty;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obfproperty + "\r\n");
                                    }));
                                }
                            }
                            if (checkBox18.Checked == true)
                            {
                                if (property.GetMethod.IsPublic)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("属性：" + property.Name + " => ");
                                    }));
                                    string obfproperty = "";
                                    if (radioButton1.Checked == true) obfproperty = ObfString(ObfMode.Hexadecimal, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton2.Checked == true) obfproperty = ObfString(ObfMode.Unicode, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton3.Checked == true) obfproperty = ObfString(ObfMode.Space, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton4.Checked == true) obfproperty = ObfString(ObfMode.Number, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton5.Checked == true) obfproperty = ObfString(ObfMode.UUID, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton6.Checked == true) obfproperty = ObfString(ObfMode.RandomString, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton7.Checked == true) obfproperty = ObfString(ObfMode.Alphabets, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton8.Checked == true) obfproperty = ObfString(ObfMode.EncryptionString, int.Parse(textBox4.Text), property.Name);
                                    property.Name = obfproperty;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obfproperty + "\r\n");
                                    }));
                                }
                            }
                            if (checkBox19.Checked == true)
                            {
                                if (property.SetMethod.IsPublic)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("属性：" + property.Name + " => ");
                                    }));
                                    string obfproperty = "";
                                    if (radioButton1.Checked == true) obfproperty = ObfString(ObfMode.Hexadecimal, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton2.Checked == true) obfproperty = ObfString(ObfMode.Unicode, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton3.Checked == true) obfproperty = ObfString(ObfMode.Space, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton4.Checked == true) obfproperty = ObfString(ObfMode.Number, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton5.Checked == true) obfproperty = ObfString(ObfMode.UUID, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton6.Checked == true) obfproperty = ObfString(ObfMode.RandomString, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton7.Checked == true) obfproperty = ObfString(ObfMode.Alphabets, int.Parse(textBox4.Text), property.Name);
                                    else if (radioButton8.Checked == true) obfproperty = ObfString(ObfMode.EncryptionString, int.Parse(textBox4.Text), property.Name);
                                    property.Name = obfproperty;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obfproperty + "\r\n");
                                    }));
                                }
                            }
                        }
                    }

                    //混淆类名
                    if (checkBox7.Checked == true)
                    {
                        if (pe != System.Reflection.Emit.PEFileKinds.Dll)
                        {
                            if (!type.IsPublic && (((type.Name != "<Module>") && !type.IsRuntimeSpecialName) && (!type.IsSpecialName && !type.Name.Contains("Resources"))) && ((!type.Name.StartsWith("<") && !type.Name.Contains("__"))) && !type.IsEnum)
                            {
                                Invoke(new MethodInvoker(() =>
                                {
                                    richTextBoxEx1.AppendText("类：" + type.Name + " => ");
                                }));

                                string obftype = "";
                                if (radioButton1.Checked == true) obftype = ObfString(ObfMode.Hexadecimal, int.Parse(textBox5.Text), type.Name);
                                else if (radioButton2.Checked == true) obftype = ObfString(ObfMode.Unicode, int.Parse(textBox5.Text), type.Name);
                                else if (radioButton3.Checked == true) obftype = ObfString(ObfMode.Space, int.Parse(textBox5.Text), type.Name);
                                else if (radioButton4.Checked == true) obftype = ObfString(ObfMode.Number, int.Parse(textBox5.Text), type.Name);
                                else if (radioButton5.Checked == true) obftype = ObfString(ObfMode.UUID, int.Parse(textBox5.Text), type.Name);
                                else if (radioButton6.Checked == true) obftype = ObfString(ObfMode.RandomString, int.Parse(textBox5.Text), type.Name);
                                else if (radioButton7.Checked == true) obftype = ObfString(ObfMode.Alphabets, int.Parse(textBox5.Text), type.Name);
                                else if (radioButton8.Checked == true) obftype = ObfString(ObfMode.EncryptionString, int.Parse(textBox5.Text), type.Name);
                                type.Name = obftype;
                                Invoke(new MethodInvoker(() =>
                                {
                                    richTextBoxEx1.AppendText(obftype + "\r\n");
                                }));
                            }
                            if(checkBox10.Checked == true)
                            {
                                if (type.IsPublic)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("类：" + type.Name + " => ");
                                    }));

                                    string obftype = "";
                                    if (radioButton1.Checked == true) obftype = ObfString(ObfMode.Hexadecimal, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton2.Checked == true) obftype = ObfString(ObfMode.Unicode, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton3.Checked == true) obftype = ObfString(ObfMode.Space, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton4.Checked == true) obftype = ObfString(ObfMode.Number, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton5.Checked == true) obftype = ObfString(ObfMode.UUID, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton6.Checked == true) obftype = ObfString(ObfMode.RandomString, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton7.Checked == true) obftype = ObfString(ObfMode.Alphabets, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton8.Checked == true) obftype = ObfString(ObfMode.EncryptionString, int.Parse(textBox5.Text), type.Name);
                                    type.Name = obftype;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obftype + "\r\n");
                                    }));
                                }
                            }
                            if(checkBox11.Checked == true)
                            {
                                if (type.Name == "<Module>")
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("类：" + type.Name + " => ");
                                    }));

                                    string obftype = "";
                                    if (radioButton1.Checked == true) obftype = ObfString(ObfMode.Hexadecimal, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton2.Checked == true) obftype = ObfString(ObfMode.Unicode, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton3.Checked == true) obftype = ObfString(ObfMode.Space, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton4.Checked == true) obftype = ObfString(ObfMode.Number, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton5.Checked == true) obftype = ObfString(ObfMode.UUID, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton6.Checked == true) obftype = ObfString(ObfMode.RandomString, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton7.Checked == true) obftype = ObfString(ObfMode.Alphabets, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton8.Checked == true) obftype = ObfString(ObfMode.EncryptionString, int.Parse(textBox5.Text), type.Name);
                                    type.Name = obftype;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obftype + "\r\n");
                                    }));
                                }
                            }
                            if(checkBox12.Checked == true)
                            {
                                if (type.IsRuntimeSpecialName)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("类：" + type.Name + " => ");
                                    }));

                                    string obftype = "";
                                    if (radioButton1.Checked == true) obftype = ObfString(ObfMode.Hexadecimal, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton2.Checked == true) obftype = ObfString(ObfMode.Unicode, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton3.Checked == true) obftype = ObfString(ObfMode.Space, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton4.Checked == true) obftype = ObfString(ObfMode.Number, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton5.Checked == true) obftype = ObfString(ObfMode.UUID, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton6.Checked == true) obftype = ObfString(ObfMode.RandomString, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton7.Checked == true) obftype = ObfString(ObfMode.Alphabets, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton8.Checked == true) obftype = ObfString(ObfMode.EncryptionString, int.Parse(textBox5.Text), type.Name);
                                    type.Name = obftype;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obftype + "\r\n");
                                    }));
                                }
                            }
                            if(checkBox13.Checked == true)
                            {
                                if (type.IsSpecialName)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("类：" + type.Name + " => ");
                                    }));

                                    string obftype = "";
                                    if (radioButton1.Checked == true) obftype = ObfString(ObfMode.Hexadecimal, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton2.Checked == true) obftype = ObfString(ObfMode.Unicode, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton3.Checked == true) obftype = ObfString(ObfMode.Space, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton4.Checked == true) obftype = ObfString(ObfMode.Number, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton5.Checked == true) obftype = ObfString(ObfMode.UUID, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton6.Checked == true) obftype = ObfString(ObfMode.RandomString, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton7.Checked == true) obftype = ObfString(ObfMode.Alphabets, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton8.Checked == true) obftype = ObfString(ObfMode.EncryptionString, int.Parse(textBox5.Text), type.Name);
                                    type.Name = obftype;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obftype + "\r\n");
                                    }));
                                }
                            }
                            if(checkBox14.Checked == true)
                            {
                                if (type.Name.Contains("Resources"))
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("类：" + type.Name + " => ");
                                    }));

                                    string obftype = "";
                                    if (radioButton1.Checked == true) obftype = ObfString(ObfMode.Hexadecimal, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton2.Checked == true) obftype = ObfString(ObfMode.Unicode, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton3.Checked == true) obftype = ObfString(ObfMode.Space, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton4.Checked == true) obftype = ObfString(ObfMode.Number, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton5.Checked == true) obftype = ObfString(ObfMode.UUID, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton6.Checked == true) obftype = ObfString(ObfMode.RandomString, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton7.Checked == true) obftype = ObfString(ObfMode.Alphabets, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton8.Checked == true) obftype = ObfString(ObfMode.EncryptionString, int.Parse(textBox5.Text), type.Name);
                                    type.Name = obftype;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obftype + "\r\n");
                                    }));
                                }
                            }
                            if(checkBox15.Checked == true)
                            {
                                if (type.Name.StartsWith("<"))
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("类：" + type.Name + " => ");
                                    }));

                                    string obftype = "";
                                    if (radioButton1.Checked == true) obftype = ObfString(ObfMode.Hexadecimal, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton2.Checked == true) obftype = ObfString(ObfMode.Unicode, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton3.Checked == true) obftype = ObfString(ObfMode.Space, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton4.Checked == true) obftype = ObfString(ObfMode.Number, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton5.Checked == true) obftype = ObfString(ObfMode.UUID, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton6.Checked == true) obftype = ObfString(ObfMode.RandomString, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton7.Checked == true) obftype = ObfString(ObfMode.Alphabets, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton8.Checked == true) obftype = ObfString(ObfMode.EncryptionString, int.Parse(textBox5.Text), type.Name);
                                    type.Name = obftype;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obftype + "\r\n");
                                    }));
                                }
                            }
                            if(checkBox16.Checked == true)
                            {
                                if (type.Name.Contains("__"))
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("类：" + type.Name + " => ");
                                    }));

                                    string obftype = "";
                                    if (radioButton1.Checked == true) obftype = ObfString(ObfMode.Hexadecimal, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton2.Checked == true) obftype = ObfString(ObfMode.Unicode, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton3.Checked == true) obftype = ObfString(ObfMode.Space, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton4.Checked == true) obftype = ObfString(ObfMode.Number, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton5.Checked == true) obftype = ObfString(ObfMode.UUID, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton6.Checked == true) obftype = ObfString(ObfMode.RandomString, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton7.Checked == true) obftype = ObfString(ObfMode.Alphabets, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton8.Checked == true) obftype = ObfString(ObfMode.EncryptionString, int.Parse(textBox5.Text), type.Name);
                                    type.Name = obftype;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obftype + "\r\n");
                                    }));
                                }
                            }
                            if(checkBox27.Checked == true)
                            {
                                if (type.IsEnum)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText("类：" + type.Name + " => ");
                                    }));

                                    string obftype = "";
                                    if (radioButton1.Checked == true) obftype = ObfString(ObfMode.Hexadecimal, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton2.Checked == true) obftype = ObfString(ObfMode.Unicode, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton3.Checked == true) obftype = ObfString(ObfMode.Space, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton4.Checked == true) obftype = ObfString(ObfMode.Number, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton5.Checked == true) obftype = ObfString(ObfMode.UUID, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton6.Checked == true) obftype = ObfString(ObfMode.RandomString, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton7.Checked == true) obftype = ObfString(ObfMode.Alphabets, int.Parse(textBox5.Text), type.Name);
                                    else if (radioButton8.Checked == true) obftype = ObfString(ObfMode.EncryptionString, int.Parse(textBox5.Text), type.Name);
                                    type.Name = obftype;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        richTextBoxEx1.AppendText(obftype + "\r\n");
                                    }));
                                }
                            }
                        }
                    }
                }
            }
            //保存
            if (File.Exists(Path.GetFileNameWithoutExtension(DotNetFilePath.ToString()) + ".XiaowuStudioObfuscatorProtected.exe"))
                File.Delete(Path.GetFileNameWithoutExtension(DotNetFilePath.ToString()) + ".XiaowuStudioObfuscatorProtected.exe");
            try
            {
                assembly.Write(Path.GetFileNameWithoutExtension(DotNetFilePath.ToString()) + ".XiaowuStudioObfuscatorProtected.exe");
                Invoke(new MethodInvoker(() =>
                {
                    richTextBoxEx1.AppendText("保存成功！保存位置（路径）：" + Path.GetFileNameWithoutExtension(DotNetFilePath.ToString()) + ".XiaowuStudioObfuscatorProtected.exe" + "\r\n");
                    ListViewItem listViewItem = new ListViewItem();
                    listViewItem.Text = assembly.MainModule.Name;
                    listViewItem.SubItems.Add(pe.ToString());
                    listViewItem.SubItems.Add("成功");
                    listView2.Items.Add(listViewItem);
                }));
            }
            catch(Exception ex)
            {
                Invoke(new MethodInvoker(() =>
                {
                    richTextBoxEx1.AppendText("发生错误：" + ex.ToString() + ex.Message + "\r\n");
                    ListViewItem listViewItem = new ListViewItem();
                    listViewItem.Text = assembly.MainModule.Name;
                    listViewItem.SubItems.Add(pe.ToString());
                    listViewItem.SubItems.Add("失败");
                    listView2.Items.Add(listViewItem);
                }));
            }
        }
        #endregion

        #region 打开或新建或保存混淆规则
        private void 打开OToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XiaowuStudioObfuscator混淆规则文件(*.xws-obf) | *.xws-obf";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                xws_obf = File.ReadAllBytes(openFileDialog.FileName);
            }
        }
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XiaowuStudioObfuscator混淆规则文件(*.xws-obf) | *.xws-obf";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog.FileName, xws_obf);
            }
        }
        private void 保存SToolStripButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XiaowuStudioObfuscator混淆规则文件(*.xws-obf) | *.xws-obf";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog.FileName, xws_obf);
            }
        }
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XiaowuStudioObfuscator混淆规则文件(*.xws-obf) | *.xws-obf";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                xws_obf = File.ReadAllBytes(openFileDialog.FileName);
            }
        }
        #endregion

        #region 模式配置方案
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            checkBox7.Checked = false;
            checkBox8.Checked = false;
            checkBox9.Checked = false;
            checkBox10.Checked = false;
            checkBox11.Checked = false;
            checkBox12.Checked = false;
            checkBox13.Checked = false;
            checkBox14.Checked = false;
            checkBox15.Checked = false;
            checkBox16.Checked = false;
            checkBox17.Checked = false;
            checkBox18.Checked = false;
            checkBox19.Checked = false;
            checkBox20.Checked = false;
            checkBox21.Checked = false;
            checkBox22.Checked = false;
            checkBox23.Checked = false;
            checkBox24.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;
            checkBox27.Checked = false;
            radioButton8.Checked = true;
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            checkBox3.Checked = true;
            checkBox5.Checked = true;
            checkBox6.Checked = true;
            checkBox7.Checked = true;
            textBox1.Text = "20";
            textBox2.Text = "20";
            textBox3.Text = "20";
            textBox4.Text = "20";
            textBox5.Text = "20";
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            checkBox7.Checked = false;
            checkBox8.Checked = false;
            checkBox9.Checked = false;
            checkBox10.Checked = false;
            checkBox11.Checked = false;
            checkBox12.Checked = false;
            checkBox13.Checked = false;
            checkBox14.Checked = false;
            checkBox15.Checked = false;
            checkBox16.Checked = false;
            checkBox17.Checked = false;
            checkBox18.Checked = false;
            checkBox19.Checked = false;
            checkBox20.Checked = false;
            checkBox21.Checked = false;
            checkBox22.Checked = false;
            checkBox23.Checked = false;
            checkBox24.Checked = false;
            checkBox25.Checked = false;
            checkBox26.Checked = false;
            checkBox27.Checked = false;
            radioButton5.Checked = true;
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            checkBox5.Checked = true;
            checkBox6.Checked = true;
            checkBox7.Checked = true;
            textBox1.Text = "10";
            textBox2.Text = "10";
            textBox3.Text = "10";
            textBox4.Text = "10";
            textBox5.Text = "10";
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = true;
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            checkBox3.Checked = true;
            checkBox4.Checked = true;
            checkBox5.Checked = true;
            checkBox6.Checked = true;
            checkBox7.Checked = true;
            checkBox8.Checked = true;
            checkBox9.Checked = true;
            checkBox10.Checked = true;
            checkBox11.Checked = true;
            checkBox12.Checked = true;
            checkBox13.Checked = true;
            checkBox14.Checked = true;
            checkBox15.Checked = true;
            checkBox16.Checked = true;
            checkBox17.Checked = true;
            checkBox18.Checked = true;
            checkBox19.Checked = true;
            checkBox20.Checked = true;
            checkBox21.Checked = true;
            checkBox22.Checked = true;
            checkBox23.Checked = true;
            checkBox24.Checked = true;
            checkBox25.Checked = true;
            checkBox26.Checked = true;
            checkBox27.Checked = true;
            textBox1.Text = "30";
            textBox2.Text = "30";
            textBox3.Text = "30";
            textBox4.Text = "30";
            textBox5.Text = "30";
        }
        #endregion

        #region 输出与保存
        private void 保存SToolStripButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "txt文件(*.txt) | *.txt";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, richTextBoxEx1.Text);
            }
        }
        private void 保存SToolStripButton2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "rtf文件(*.rtf) | *.rtf";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                richTextBoxEx1.SaveFile(saveFileDialog.FileName);
            }
        }
        private void richTextBoxEx1_TextChanged(object sender, EventArgs e)
        {
            richTextBoxEx1.SelectionStart = richTextBoxEx1.TextLength;
            richTextBoxEx1.ScrollToCaret();
        }
        private void 新建NToolStripButton1_Click(object sender, EventArgs e)
        {
            richTextBoxEx1.Clear();
        }
        private void 新建NToolStripButton2_Click(object sender, EventArgs e)
        {
            listView2.Items.Clear();
        }
        #endregion

        #region MainForm相关处理
        private void MainForm_FormClosing(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void MainForm_FormClosed(object sender, EventArgs e)
        {
            Dispose();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
#if TRACE
            startPage1 = new StartPage();
            startPage1.BackColor = System.Drawing.Color.Black;
            startPage1.Dock = DockStyle.Fill;
            startPage1.Location = new System.Drawing.Point(0, 0);
            startPage1.Name = "startPage1";
            Controls.Remove(panel2);
            Controls.Add(startPage1);
            timer1.Enabled = true;
            timer1.Start();
            tabControl1.SelectTab(tabPage1);
#endif
            tabControl2.TabPages.Remove(tabPage7);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Controls.Remove(startPage1);
            Controls.Add(panel2);
            timer1.Stop();
            timer1.Enabled = false;
        }
        #endregion

        #region 激活页面与购买页面
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            new LicensePage().ShowDialog();
        }
        #endregion

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            ExampleDemo demo = new ExampleDemo();
            demo.ShowDialog();
        }

        private void 下载示例样本ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExampleDemo demo = new ExampleDemo();
            demo.ShowDialog();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {

        }
    }
}
