using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XiaowuStudioObfuscator
{
    internal class SimpleConsole
    {
        public static void Code(string[] args)
        {
            Console.Clear();
            Console.WriteLine("XiaowuStudioModule-XiaowuStudioObfuscator [版本" + Application.ProductVersion.ToString() + "]");
            Console.WriteLine("(c) 吴加杨 版权所有");
            Console.WriteLine();
            if (args[0] == "-License")
            {
                if (args.Length > 1)
                {
                    if (args[1].ToString() == "-Clear")
                    {
                        Console.WriteLine(">正在执行清楚指令，请稍后......");
                        Thread.Sleep(1000);
                        try
                        {
                            Console.WriteLine();
                            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\xwsobf.lic");
                            Console.WriteLine("清除执行 - 成功");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine();
                            Console.WriteLine("清除执行 - 失败\r\n" + "可能是以下原因引起的：\r\n" + ex.Message + "\r\n" + ex.ToString());
                        }
                    }
                    else if (args[1].ToString() == "-Archive" && args.Length == 3)
                    {
                        if (args[2].Substring(0, 11) == "LicenseKey:")
                        {
                            string key = args[2].Remove(0, 11);
                            Console.WriteLine(">正在读取许可证号码并获取许可证信息中，请稍后......");
                            Thread.Sleep(1000);
                            string result;
                            try
                            {
                                key = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(key, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                for (int i = 9 - 1; i >= 0; i--)
                                {
                                    try
                                    {
                                        key = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(key, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                    }
                                    catch
                                    {
                                        result = "密钥无效！";
                                    }
                                }
                            }
                            catch
                            {
                                result = "密钥无效！";
                            }
                            string lastName, firstName, Email, Product, BeginTime, OverTime, LicenseID;
                            try
                            {
                                lastName = key.Split('&')[0];
                                firstName = key.Split('&')[1];
                                Email = key.Split('&')[2];
                                Product = key.Split('&')[3];
                                BeginTime = key.Split('&')[4];
                                OverTime = key.Split('&')[5];
                                LicenseID = key.Split('&')[6];
                                lastName = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(lastName, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                firstName = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(firstName, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                Email = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(Email, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                Product = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(Product, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                BeginTime = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(BeginTime, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                OverTime = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(OverTime, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                LicenseID = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(LicenseID, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                if (OverTime == "Null")
                                {
                                    OverTime = "无限";
                                }
                                string s = "姓：" + lastName + "      " + "名：" + firstName + "\r\n" + "电子邮件地址：" + Email + "\r\n" + "购买的项目：" + Product + "\r\n" + "开始时间：" + BeginTime + "      " + "截止时间：" + OverTime + "\r\n" + "许可证ID：" + LicenseID;
                                if (Product == "XiaowuStudio-Modules XiaowuStudio-.Net App-Obfuscator-Pro") result = s;
                                else if (Product == "XiaowuStudio-Modules XiaowuStudio-.Net App-Obfuscator-Ultra") result = s;
                                else if (Product == "XiaowuStudio-Modules XiaowuStudio-.Net App-Obfuscator-Free") result = s;
                                else
                                {
                                    result = "您的产品密钥无法激活该产品，可能是您的产品密钥不属于该产品所导致的！！！";
                                }
                            }
                            catch
                            {
                                result = "密钥无效！";
                            }
                            Console.WriteLine();
                            Console.WriteLine("信息卡 Information Card");
                            Console.WriteLine("--------------------------------------------------");
                            Console.WriteLine(result);
                            Console.WriteLine("--------------------------------------------------");
                            if (result != "密钥无效！" && result != "您的产品密钥无法激活该产品，可能是您的产品密钥不属于该产品所导致的！！！" && result.Trim() != null)
                            {
                                Console.WriteLine();
                                try
                                {
                                    Console.WriteLine();
                                    File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\xwsobf.lic", key);
                                    Console.WriteLine("激活状态 - 成功");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("激活状态 - 失败\r\n" + "可能是以下原因引起的：\r\n" + ex.Message + "\r\n" + ex.ToString());
                                }
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("激活状态 - 密钥无效，无法激活！！！");
                            }
                        }
                        else
                        {
                            Console.WriteLine("--------------------------------------------------");
                            Console.WriteLine("错误的参数！！！");
                            Console.WriteLine("用法：XiaowuStudioObfuscator.exe -License -Archive LicenseKey:你的许可证号码");
                            Console.WriteLine("--------------------------------------------------");
                        }
                    }
                    else if (args[1].ToString() == "-GetInfo")
                    {
                        Console.WriteLine(">正在读取许可证号码并获取许可证信息中，请稍后......");
                        Thread.Sleep(1000);
                        string result, key = null;
                        try
                        {
                            key = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\xwsobf.lic");
                            key = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(key, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                            for (int i = 9 - 1; i >= 0; i--)
                            {
                                try
                                {
                                    key = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(key, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                                }
                                catch
                                {
                                    result = "密钥无效！";
                                }
                            }
                        }
                        catch
                        {
                            result = "密钥无效！";
                        }
                        string lastName, firstName, Email, Product, BeginTime, OverTime, LicenseID;
                        try
                        {
                            lastName = key.Split('&')[0];
                            firstName = key.Split('&')[1];
                            Email = key.Split('&')[2];
                            Product = key.Split('&')[3];
                            BeginTime = key.Split('&')[4];
                            OverTime = key.Split('&')[5];
                            LicenseID = key.Split('&')[6];
                            lastName = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(lastName, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                            firstName = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(firstName, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                            Email = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(Email, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                            Product = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(Product, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                            BeginTime = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(BeginTime, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                            OverTime = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(OverTime, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                            LicenseID = XiaowuStudioFileOperation.EncryptionCode.AES.AESDecrypt(LicenseID, XiaowuStudioFileOperation.Key.wujiayang2007KeyB64Encode());
                            if (OverTime == "Null")
                            {
                                OverTime = "无限";
                            }
                            string s = "姓：" + lastName + "      " + "名：" + firstName + "\r\n" + "电子邮件地址：" + Email + "\r\n" + "购买的项目：" + Product + "\r\n" + "开始时间：" + BeginTime + "      " + "截止时间：" + OverTime + "\r\n" + "许可证ID：" + LicenseID;
                            if (Product == "XiaowuStudio-Modules XiaowuStudio-.Net App-Obfuscator-Pro") result = s;
                            else if (Product == "XiaowuStudio-Modules XiaowuStudio-.Net App-Obfuscator-Ultra") result = s;
                            else if (Product == "XiaowuStudio-Modules XiaowuStudio-.Net App-Obfuscator-Free") result = s;
                            else
                            {
                                result = "您的产品密钥无法激活该产品，可能是您的产品密钥不属于该产品所导致的！！！";
                            }
                        }
                        catch
                        {
                            result = "密钥无效！";
                        }
                        Console.WriteLine();
                        Console.WriteLine("信息卡 Information Card");
                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine(result);
                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine("LicenseKey:");
                        Console.WriteLine(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\xwsobf.lic"));
                        Console.WriteLine("--------------------------------------------------");
                    }
                    else if (args[1].ToString() == "-help")
                    {
                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine("命令“-License -Archive”用于激活许可证");
                        Console.WriteLine("用法：XiaowuStudioObfuscator.exe -License -Archive LicenseKey:你的许可证号码");
                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine("命令“-License -Clear”用于清除许可证");
                        Console.WriteLine("用法：XiaowuStudioObfuscator.exe -License -Clear");
                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine("命令“-License -GetInfo”用于查询许可证");
                        Console.WriteLine("用法：XiaowuStudioObfuscator.exe -License -GetInfo");
                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine("命令“-License -help”用于获取关于License命令的详细帮助");
                        Console.WriteLine("用法：XiaowuStudioObfuscator.exe -License -help");
                        Console.WriteLine("--------------------------------------------------");
                    }
                    else
                    {
                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine("无效命令！！");
                        Console.WriteLine("请输入 -License -help 获取关于-License的用法");
                        Console.WriteLine("--------------------------------------------------");
                    }
                }
                else
                {
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("无效命令！！");
                    Console.WriteLine("命令“-License”不含空的命令");
                    Console.WriteLine("--------------------------------------------------");
                }
                Console.WriteLine();
                Console.WriteLine("按任意键退出...");
                Console.ReadKey(true);
            }
            else if (args[0] == "-Obfuscator")
            {
                Console.WriteLine();
                Console.WriteLine("按任意键退出...");
                Console.ReadKey(true);
            }
            else if (args[0] == "-help")
            {
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("命令“-License”用于激活许可证");
                Console.WriteLine("用法：XiaowuStudioObfuscator.exe -License");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("命令“-Obfuscator”用于混淆.Net软件");
                Console.WriteLine("用法：XiaowuStudioObfuscator.exe -Obfuscator Path:");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("命令“-help”用于查看帮助");
                Console.WriteLine("用法：XiaowuStudioObfuscator.exe -help");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("详细命令可在需详知的命令后“-help”用于查看帮助");
                Console.WriteLine("如：XiaowuStudioObfuscator.exe -License -help");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine("按任意键退出...");
                Console.ReadKey(true);
            }
            else
            {
                Console.WriteLine("无该指令！！！");
                Console.WriteLine("请输入“-help”命令查看操作列表");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey(true);
            }
        }
    }
}
