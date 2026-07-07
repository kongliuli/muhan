using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ModernBoxes.Infrastructure
{
    public class GetIcon
    {
        /// <summary>提取文件图标到指定目录</summary>
        [STAThread]
        public static void getFileIcon(String FilePath, String savePath, String FileName)
        {
            //ָ�����ͼ����ļ���
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

            //ѡ���ļ��е�ͼ������
            var iconTotalCount = 1;

            //���ڽ��ջ�ȡ����ͼ��ָ��
            IntPtr[] hIconsDefault = new IntPtr[iconTotalCount];
            //��Ӧ��ͼ��id
            int[] idsDefault = new int[iconTotalCount];
            //�ɹ���ȡ����ͼ�����
            var successCount = PrivateExtractIcons(FilePath, 0, 0, 0, hIconsDefault, idsDefault, iconTotalCount, 0);
            IntPtr[] hIconsLarge = new IntPtr[iconTotalCount];
            int[] idsLarge = new int[iconTotalCount];
            PrivateExtractIcons(FilePath, 0, 64, 64, hIconsLarge, idsLarge, iconTotalCount, 0);

            //����������ͼ��
            for (var i = 0; i < successCount; i++)
            {
                //ָ��Ϊ�գ�����
                if (hIconsDefault[i] == IntPtr.Zero) continue;

                IntPtr hicon, defaultSizeIcon = hIconsDefault[i], largeIcon = hIconsLarge[i];
                //���id��ͬ��˵���Ƕ�ȡͬһ��ͼ���ļ���ȡĬ�ϴ�С��
                if (idsDefault[i] == idsLarge[i])
                {
                    hicon = defaultSizeIcon;
                    DestroyIcon(largeIcon);
                }
                //���id��ͬ��˵�����ڴ�ͼ��ȡ��ͼ��
                else
                {
                    hicon = largeIcon;
                    DestroyIcon(defaultSizeIcon);
                }

                using (var ico = Icon.FromHandle(hicon))
                {
                    using (var myIcon = ico.ToBitmap())
                    {
                        myIcon.Save($"{savePath + FileName}", ImageFormat.Icon);
                    }
                }
                //�ڴ����
                DestroyIcon(hicon);
            }
        }


        /// <summary>
        /// ��ȡ��ݷ�ʽ��Ŀ���ַ
        /// </summary>
        /// <param name="kinkPath"></param>
        /// <returns></returns>
        public static String getLinkTarget(String kinkPath)
        {
            if (System.IO.File.Exists(kinkPath))
            {
                Type wshShellType = Type.GetTypeFromProgID("WScript.Shell")!;
                dynamic wshShell = Activator.CreateInstance(wshShellType)!;
                dynamic wshShortcut = wshShell.CreateShortcut(kinkPath);
                return wshShortcut.TargetPath;
            }
            else
                return String.Empty;
        }

        //details: https://msdn.microsoft.com/en-us/library/windows/desktop/ms648075(v=vs.85).aspx
        [DllImport("User32.dll")]
        public static extern int PrivateExtractIcons(string lpszFile, int nIconIndex, int cxIcon, int cyIcon, IntPtr[] phicon, int[] piconid, int nIcons, int flags);

        //details:https://msdn.microsoft.com/en-us/library/windows/desktop/ms648063(v=vs.85).aspx
        [DllImport("User32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);
    }
}
