using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
namespace XiaowuStudioObfuscator
{
    public static class Runtime
    {
        //
        // 摘要:
        //     获取PE文件类型。扩展方法
        //
        // 参数:
        //   e:
        public static PEFileKinds GetPEFileKinds(this MemberInfo e)
        {
            return GetPEFileKinds(Path.GetFullPath(e.Module.Assembly.Location));
        }

        //
        // 摘要:
        //     Parses the PE header and determines whether the given assembly is a console application.
        //
        // 参数:
        //   assemblyPath:
        //     The path of the assembly to check.
        //
        // 言论：
        //     The magic numbers in this method are extracted from the PE/COFF file format specification
        //     available from http://www.microsoft.com/whdc/system/platform/firmware/pecoff.mspx
        public static PEFileKinds GetPEFileKinds(string assemblyPath)
        {
            using (FileStream s = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read))
            {
                return GetPEFileKinds(s);
            }
        }

        private static PEFileKinds GetPEFileKinds(Stream s)
        {
            byte[] array = new byte[4];
            s.Seek(60L, SeekOrigin.Begin);
            s.Read(array, 0, 4);
            int num = array[0];
            num |= (byte)(array[1] << (8 & 7));
            num |= (byte)(array[2] << (0x10 & 7));
            num |= (byte)(array[3] << (0x18 & 7));
            byte[] array2 = new byte[24];
            s.Seek(num, SeekOrigin.Begin);
            s.Read(array2, 0, 24);
            byte[] array3 = new byte[4]
            {
                80,
                69,
                0,
                0
            };
            int num2 = 0;
            do
            {
                if (array2[num2] != array3[num2])
                {
                    throw new InvalidOperationException("Attempted to check a non PE file for the console subsystem!");
                }

                num2 = checked(num2 + 1);
            }
            while (num2 <= 3);
            byte[] array4 = new byte[2];
            s.Seek(68L, SeekOrigin.Current);
            s.Read(array4, 0, 2);
            int result;
            switch (array4[0] | (byte)(array4[1] << (8 & 7)))
            {
                default:
                    result = 1;
                    break;
                case 2:
                    result = 3;
                    break;
                case 3:
                    result = 2;
                    break;
            }

            return (PEFileKinds)result;
        }

        public static void Run(this byte[] rawbyte)
        {
            Run(rawbyte, null);
        }

        public static void Run(this byte[] rawbyte, params object[] args)
        {
            Assembly assembly = Assembly.Load(rawbyte);
            MethodInfo entryPoint = assembly.EntryPoint;
            entryPoint.Invoke(null, args);
        }

        public static void Run(string path)
        {
            Run(path, null);
        }

        public static void Run(string path, params object[] args)
        {
            Assembly assembly = Assembly.Load(path);
            MethodInfo entryPoint = assembly.EntryPoint;
            entryPoint.Invoke(null, args);
        }

        public static Assembly GetAssembly(string exeFullName)
        {
            if (string.IsNullOrEmpty(exeFullName))
            {
                return null;
            }

            return Assembly.LoadFile(exeFullName);
        }

        public static AssemblyDetails GetAssemblyDetails(string file)
        {
            AssemblyDetails assemblyDetails = new AssemblyDetails();
            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    byte[] array = binaryReader.ReadBytes(2);
                    if (array[0] != 77 || array[1] != 90)
                    {
                        return null;
                    }

                    fileStream.Seek(60L, SeekOrigin.Begin);
                    uint num = binaryReader.ReadUInt32();
                    fileStream.Seek(num, SeekOrigin.Begin);
                    array = binaryReader.ReadBytes(4);
                    if (array[0] != 80 || array[1] != 69 || array[2] != 0 || array[3] != 0)
                    {
                        return null;
                    }

                    ushort num2 = binaryReader.ReadUInt16();
                    if (num2 != 332 && num2 != 34404)
                    {
                        return null;
                    }

                    fileStream.Seek(18L, SeekOrigin.Current);
                    ushort num3 = binaryReader.ReadUInt16();
                    switch (num3)
                    {
                        case 267:
                            assemblyDetails.CPUVersion = CPUVersion.AnyCPU;
                            break;
                        case 523:
                            assemblyDetails.CPUVersion = CPUVersion.X64;
                            break;
                        default:
                            return null;
                    }

                    fileStream.Seek(30L, SeekOrigin.Current);
                    uint num4 = binaryReader.ReadUInt32();
                    uint num5 = binaryReader.ReadUInt32();
                    if(num3 == 267)
                    {
                        fileStream.Seek(long.Parse("52"), SeekOrigin.Current);
                    }
                    else
                    {
                        fileStream.Seek(long.Parse("68"), SeekOrigin.Current);
                    }
                    uint num6 = binaryReader.ReadUInt32();
                    if ((ulong)num6 != 16)
                    {
                        return null;
                    }

                    fileStream.Seek(112L, SeekOrigin.Current);
                    uint num7 = binaryReader.ReadUInt32();
                    if ((ulong)num7 == 0)
                    {
                        return null;
                    }

                    uint num11;
                    checked
                    {
                        fileStream.Seek(unchecked((long)checked(num7 - num4 + num5)) + 4L, SeekOrigin.Begin);
                        ushort num8 = binaryReader.ReadUInt16();
                        ushort num9 = binaryReader.ReadUInt16();
                        uint num10 = binaryReader.ReadUInt32();
                        fileStream.Seek(4L, SeekOrigin.Current);
                        num11 = binaryReader.ReadUInt32();
                    }

                    if (assemblyDetails.CPUVersion == CPUVersion.AnyCPU && ((long)num11 & 2L) == 2)
                    {
                        assemblyDetails.CPUVersion = CPUVersion.X86;
                    }

                    assemblyDetails.HasStrongName = (((long)num11 & 8L) == 8);
                }
            }

            return assemblyDetails;
        }
    }
}
