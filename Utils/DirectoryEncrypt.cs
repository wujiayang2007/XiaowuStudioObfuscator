using System.Diagnostics;
using System.IO;

namespace XiaowuStudioFileOperation.Utils
{
    /// <summary>
    /// 文件夹加密类
    /// </summary>
    public class DirectoryEncrypt
    {
        #region 加密文件夹及其子文件夹中的所有文件
        /// <summary>
        /// 加密文件夹及其子文件夹中的所有文件
        /// </summary>
        public static void EncryptDirectory(string dirPath, string pwd, RefreshDirProgress refreshDirProgress, RefreshFileProgress refreshFileProgress)
        {
            string[] filePaths = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                if (!File.Exists(filePaths[i])) continue;
                FileEncrypt.EncryptFile(filePaths[i], pwd, refreshFileProgress);
                refreshDirProgress(filePaths.Length, i + 1);
            }
        }
        #endregion

        #region 解密文件夹及其子文件夹中的所有文件
        /// <summary>
        /// 解密文件夹及其子文件夹中的所有文件
        /// </summary>
        public static void DecryptDirectory(string dirPath, string pwd, RefreshDirProgress refreshDirProgress, RefreshFileProgress refreshFileProgress)
        {
            string[] filePaths = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                if (!File.Exists(filePaths[i])) continue;
                FileEncrypt.DecryptFile(filePaths[i], pwd, refreshFileProgress);
                refreshDirProgress(filePaths.Length, i + 1);
            }
        }
        #endregion

        #region 加密当前文件夹
        /// <summary>
        /// 加密当前文件夹
        /// </summary>
        public static void EncryptCurrentDirectory(string dirPath, string pwd, RefreshDirProgress refreshDirProgress, RefreshFileProgress refreshFileProgress)
        {
            int delta = 0;
            string[] filePaths = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                if (!File.Exists(filePaths[i])) continue;
                if (IsSelf(filePaths[i]))
                {
                    delta = -1;
                    continue;
                }
                FileEncrypt.EncryptFile(filePaths[i], pwd, refreshFileProgress);
                refreshDirProgress(filePaths.Length - 1, i + 1 + delta);
            }
        }
        #endregion

        #region 解密当前文件夹
        /// <summary>
        /// 解密当前文件夹
        /// </summary>
        public static void DecryptCurrentDirectory(string dirPath, string pwd, RefreshDirProgress refreshDirProgress, RefreshFileProgress refreshFileProgress)
        {
            int delta = 0;
            string[] filePaths = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                if (!File.Exists(filePaths[i])) continue;
                if (IsSelf(filePaths[i]))
                {
                    delta = -1;
                    continue;
                }
                FileEncrypt.DecryptFile(filePaths[i], pwd, refreshFileProgress);
                refreshDirProgress(filePaths.Length - 1, i + 1 + delta);
            }
        }
        #endregion

        #region 判断是否是自己
        /// <summary>
        /// 判断是否是自己
        /// </summary>
        private static bool IsSelf(string path)
        {
            if (path.ToLower().IndexOf(".exe") + 4 == path.Length) //如果是exe文件
            {
                int pos = path.LastIndexOf(@"\");
                string exeName = path.Substring(pos + 1);
                if (Process.GetCurrentProcess().ProcessName == exeName.Substring(0, exeName.Length - 4))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

    }

    /// <summary>
    /// 更新文件夹加密进度
    /// </summary>
    public delegate void RefreshDirProgress(int max, int value);

}
