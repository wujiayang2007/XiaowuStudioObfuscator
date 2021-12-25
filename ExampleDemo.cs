using System;
using System.Windows.Forms;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using ICSharpCode.TextEditor.Document;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ManifestResourceAttributes = Mono.Cecil.ManifestResourceAttributes;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

namespace XiaowuStudioObfuscator
{
    public partial class ExampleDemo : Form
    {
        public ExampleDemo()
        {
            InitializeComponent();
        }
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
        byte[] Obf(string OBFpath)
        {
            Level INFO = Level.INFO;
            var assembly = AssemblyDefinition.ReadAssembly(OBFpath);
            Invoke(new MethodInvoker(() =>
            {
                Result("程序集名称：" + assembly.MainModule.Name,INFO);
            }));

            EmbeddedResource erTemp = new EmbeddedResource("XiaowuStudioObfuscator\u00A0\u0020\u3000\u00A0\u0020\u3000", Mono.Cecil.ManifestResourceAttributes.Public, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 });
            assembly.MainModule.Resources.Add(erTemp);
            Invoke(new MethodInvoker(() =>
            {
                Result("已在资源嵌入XiaowuStudioObfuscator防伪标记",INFO);
            }));

            foreach (var module in assembly.Modules)
            {
                Invoke(new MethodInvoker(() =>
                {
                    Result("模块名：" + assembly.ToString(),INFO);
                }));

                foreach (var type in module.Types)
                {
                    Invoke(new MethodInvoker(() =>
                    {
                        Result("类型名：" + type.FullName,INFO);
                    }));

                    //注入方法定义
                    Random ran = new Random();
                    int c = ran.Next(1, 10);
                    var md = getxwsobfr(assembly, c);
                    type.Methods.Add(md);
                    Invoke(new MethodInvoker(() =>
                    {
                        Result("已注入方法定义！",INFO);
                    }));

                    //枚举命名空间
                    Invoke(new MethodInvoker(() =>
                    {
                        Result("命名空间：" + type.Namespace + " => ",INFO);
                    }));

                    string obfnamespace = "";

                    //枚举方法
                    foreach (var method in type.Methods)
                        {
                            Invoke(new MethodInvoker(() =>
                            {
                                Result("方法全名：" + method.FullName,INFO);
                            }));

                            //混淆form的Name属性
                            if (method.Name == "InitializeComponent" || method.Name == "xwsobfr" || method.Name == "__ENCAddToList")
                            {
                                var worker = method.Body.GetILProcessor();

                                var list = method.Body.Instructions.Where(i => i.Operand != null && i.Operand.ToString().Contains("System.Windows.Forms.Control::set_Name")).ToList();
                                list.ForEach(i =>
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
                                });
                            }
                            else
                            {
                                //系统方法、关键字、构造器不混淆
                                if (!method.IsConstructor && !method.IsRuntime && !method.IsRuntimeSpecialName && !method.IsSpecialName && !method.IsVirtual && !method.IsAbstract && method.Overrides.Count <= 0 && !method.Name.StartsWith("<"))
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        Result("方法：" + method.Name + " => ",INFO);
                                    }));

                                    string obfmethod = "";
                                    obfnamespace = ObfString(ObfMode.EncryptionString, 0, type.Namespace);
                                    method.Name = obfmethod;
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        Result(obfmethod,INFO);
                                    }));
                                }
                            }
                            if (method.Body != null)
                            {
                                var strilist = method.Body.Instructions.Where(i => i.OpCode.Name == "ldstr").ToList();
                                strilist.ForEach(i =>
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        Result("字符串：" + i.Operand.ToString(),INFO);
                                    }));
                                    string obfoperand = "";
                                    obfnamespace = ObfString(ObfMode.EncryptionString, 0, type.Namespace);
                                    EmbeddedResource erTmp = new EmbeddedResource(obfoperand, ManifestResourceAttributes.Private, encode(i.Operand.ToString(), (byte)c));
                                    assembly.MainModule.Resources.Add(erTmp);
                                    i.Operand = obfoperand;
                                    var worker = method.Body.GetILProcessor();
                                    worker.InsertAfter(i, worker.Create(OpCodes.Call, md));
                                });
                            }
                        }

                    //枚举字段
                    foreach (var field in type.Fields)
                        {
                            Invoke(new MethodInvoker(() =>
                                {
                                    Result("字段：" + field.Name + " => ",INFO);
                                }));
                                string obffield = "";
                            obfnamespace = ObfString(ObfMode.EncryptionString, 0, type.Namespace);
                            field.Name = obffield;
                                Invoke(new MethodInvoker(() =>
                                {
                                    Result(obffield,INFO);
                                })); 
                        }

                    //枚举属性
                    foreach (var property in type.Properties)
                        {
                            if (property.GetMethod != null && property.SetMethod != null && !property.GetMethod.IsPublic && !property.SetMethod.IsPublic)
                            {
                                Invoke(new MethodInvoker(() =>
                                {
                                    Result("属性：" + property.Name + " => ",INFO);
                                }));

                                string obfproperty = "";
                                obfnamespace = ObfString(ObfMode.EncryptionString, 0, type.Namespace);
                                property.Name = obfproperty;
                                Invoke(new MethodInvoker(() =>
                                {
                                    Result(obfproperty,INFO);
                                }));

                            }
                        }
                }
            }
            
            MemoryStream ms1 = new MemoryStream();
            assembly.Write(ms1);
            return ms1.ToArray();
        }
        #endregion
        Assembly objAssembly = null;
        string path = null;
        private void button1_Click(object sender, EventArgs e)
        {
            //get the code to compile
            string strSourceCode = txtSource.Text;

            // 1.Create a new CSharpCodePrivoder instance
            CSharpCodeProvider objCSharpCodePrivoder =
            new CSharpCodeProvider();

            // 2.Sets the runtime compiling parameters by crating a new CompilerParameters instance
            CompilerParameters objCompilerParameters = new CompilerParameters();
            objCompilerParameters.ReferencedAssemblies.Add("System.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Data.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Deployment.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Drawing.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Net.Http.dll");
            objCompilerParameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Xml.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            objCompilerParameters.GenerateInMemory = true;
            objCompilerParameters.OutputAssembly = Application.StartupPath + "//cr.temp";
            // 3.CompilerResults: Complile the code snippet by calling a method from the provider
            CompilerResults cr = objCSharpCodePrivoder.CompileAssemblyFromSource(objCompilerParameters, strSourceCode);
            if (cr.Errors.HasErrors)
            {
                string strErrorMsg = cr.Errors.Count.ToString() + " 错误:";

                for (int x = 0; x < cr.Errors.Count; x++)
                {
                    strErrorMsg = strErrorMsg + "\r\nLine: " +
                    cr.Errors[x].Line.ToString() + " - " +
                    cr.Errors[x].ErrorText;
                }

                Result(strErrorMsg,Level.ERROR);
            }
            else
            {
                Result("编译通过。", Level.INFO);
                // 4. Invoke the method by using Reflection
                objAssembly = cr.CompiledAssembly;
                path = objCompilerParameters.OutputAssembly;
            }
        }

        private void ExampleDemo_Load(object sender, EventArgs e)
        {
            
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if(comboBox1.Text == "Hello World(C#Console)")
            {
                textBox1.Text = "Dynamicly";
                textBox2.Text = "HelloWorld";
                textBox3.Text = "GetTime";
                txtSource.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("C#");
                txtSource.Text = Properties.Resources.Hello_World_CSharpConsole;
            }
        }
        void Result(string str,Level level)
        {
            string NowDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff zzz");
            ListViewItem item = new ListViewItem();
            item.Text = NowDate;
            item.SubItems.Add(str);
            if (level == Level.WARNING) item.SubItems.Add("WARNING");
            else if (level == Level.ERROR) item.SubItems.Add("ERROR");
            else if (level == Level.INFO) item.SubItems.Add("INFO");
            listView1.Items.Add(item);
        }
        enum Level
        {
            WARNING,
            ERROR,
            INFO
        }

        private void button2_Click(object sender, EventArgs e)
        {
            object objClass;
            string strResult;
            try
            {
                objClass = objAssembly.CreateInstance(textBox1.Text + "." + textBox2.Text);

                if (objClass == null)
                {
                    Result("错误：" + "无法加载该类！！！", Level.ERROR);
                }
                else
                {
                    object[] objCodeParms = new object[1];
                    objCodeParms[0] = "Allan.";

                    strResult = (string)objClass.GetType().InvokeMember(textBox3.Text, BindingFlags.InvokeMethod, null, objClass, null);
                    richTextBoxEx1.Text += strResult;
                }
                
            }
            catch (Exception ex)
            {
                Result("错误：" + ex.Message, Level.ERROR);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] bytes = Obf(path);
                File.WriteAllBytes(Application.StartupPath + "//obf.temp", bytes);
                var decompiler = new CSharpDecompiler(Application.StartupPath + "//obf.temp", new DecompilerSettings());
                var name = new FullTypeName(textBox5.Text + "." + textBox4.Text);
                var code = decompiler.DecompileTypeAsString(name);
                textEditorControl1.SetHighlighting("C#");
                textEditorControl1.Text = code;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误",buttons:MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            }
        }
    }
}
