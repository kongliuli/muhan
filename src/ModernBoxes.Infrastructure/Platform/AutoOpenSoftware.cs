using Microsoft.Win32;
using ModernBoxes.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using ModernBoxes.Core.Enums;

namespace ModernBoxes.Infrastructure
{
    public class AutoOpenSoftware
    {
        private readonly IUserNotifier? _notifier;

        public AutoOpenSoftware(IUserNotifier? notifier = null)
        {
            _notifier = notifier;
        }
        /// <summary>自动创建桌面快捷方式</summary>
        private const string quickName = "ModernBoxes";

        /// <summary>自动读取系统自启动目录</summary>
        private string systemStartPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            }
        }

        /// <summary>
        /// �Զ���ȡ��������·��
        /// </summary>
        private string appAllPath
        {
            get
            {
                return Process.GetCurrentProcess().MainModule.FileName;
            }
        }

        /// <summary>
        /// �Զ���ȡ����Ŀ¼
        /// </summary>
        private string desktopPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
        }

        /// <summary>
        /// ���ÿ���������
        /// </summary>
        /// <param name="onOff">��������</param>
        public void SetAutoStart(bool onOff = true)
        {
            if (onOff) //��������
            {
                //��ȡ����·��Ӧ�ó����ݷ�ʽ��·������
                List<string> shortcutPaths = GetQuickFromFolder(systemStartPath, appAllPath);
                //����2���Կ�ݷ�ʽ����һ����ݷ�ʽ�������ظ�
                if (shortcutPaths.Count >= 2)
                {
                    for (int i = 1; i < shortcutPaths.Count; i++)
                    {
                        DeleteFile(shortcutPaths[i]);
                    }
                }
                else if (shortcutPaths.Count < 1)//�������򴴽���ݷ�ʽ
                {
                    CreateShortcut(systemStartPath, quickName, appAllPath, "Client");
                }
            }
            else //����������
            {
                //��ȡ����·��Ӧ�ó����ݷ�ʽ��·������
                List<string> shortcutPaths = GetQuickFromFolder(systemStartPath, appAllPath);
                //���ڿ�ݷ�ʽ�����ȫ��ɾ��
                if (shortcutPaths.Count > 0)
                {
                    for (int i = 0; i < shortcutPaths.Count; i++)
                    {
                        DeleteFile(shortcutPaths[i]);
                    }
                }
            }


        }

        /// <summary>
        ///  ��Ŀ��·������ָ���ļ��Ŀ�ݷ�ʽ
        /// </summary>
        /// <param name="directory">Ŀ��Ŀ¼</param>
        /// <param name="shortcutName">��ݷ�ʽ����</param>
        /// <param name="targetPath">�ļ���ȫ·��</param>
        /// <param name="description">����</param>
        /// <param name="iconLocation">ͼ���ַ</param>
        /// <returns></returns>
        private bool CreateShortcut(string directory, string shortcutName, string targetPath, string description, string? iconLocation = null)
        {
            try
            {
                //Ŀ¼�������򴴽�
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", shortcutName));          //�ϳ�·��
                Type wshShellType = Type.GetTypeFromProgID("WScript.Shell")!;
                dynamic shell = Activator.CreateInstance(wshShellType)!;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);    //������ݷ�ʽ����
                shortcut.TargetPath = targetPath;                                                               //ָ��Ŀ��·��
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);                                  //������ʼλ��
                shortcut.WindowStyle = 1;                                                                       //�������з�ʽ��Ĭ��Ϊ���洰��
                shortcut.Description = description;                                                             //���ñ�ע
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;    //����ͼ��·��
                shortcut.Save();                                                                                //�����ݷ�ʽ

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }

        /// <summary>
        /// ��ȡָ���ļ�����ָ��Ӧ�ó���Ŀ�ݷ�ʽ·������
        /// </summary>
        /// <param name="directory">�ļ���</param>
        /// <param name="targetPath">Ŀ��Ӧ�ó���·��</param>
        /// <returns>Ŀ��Ӧ�ó���Ŀ�ݷ�ʽ</returns>
        private List<string> GetQuickFromFolder(string directory, string targetPath)
        {
            List<string> tempStrs = new List<string>();
            tempStrs.Clear();
            string tempStr = null;
            string[] files = Directory.GetFiles(directory, "*.lnk");
            if (files == null || files.Length < 1)
            {
                return tempStrs;
            }

            for (int i = 0; i < files.Length; i++)
            {
                tempStr = this.GetAppPathFromQuick(files[i]);
                if (tempStr == targetPath)
                {
                    tempStrs.Add(files[i]);
                }
            }

            return tempStrs;
        }

        /// <summary>
        /// ��ȡ��ݷ�ʽ��Ŀ���ļ�·��-�����ж��Ƿ��Ѿ��������Զ�����
        /// </summary>
        /// <param name="shortcutPath"></param>
        /// <returns></returns>
        private string GetAppPathFromQuick(string shortcutPath)
        {
            if (System.IO.File.Exists(shortcutPath))
            {
                Type wshShellType = Type.GetTypeFromProgID("WScript.Shell")!;
                dynamic shell = Activator.CreateInstance(wshShellType)!;
                dynamic shortct = shell.CreateShortcut(shortcutPath);
                return shortct.TargetPath;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// ����·��ɾ���ļ�-����ȡ������ʱ�Ӽ��������Ŀ¼ɾ������Ŀ�ݷ�ʽ
        /// </summary>
        /// <param name="path">·��</param>
        private void DeleteFile(string path)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// �������ϴ�����ݷ�ʽ-�����Ҫ���Ե���
        /// </summary>
        /// <param name="desktopPath">�����ַ</param>
        /// <param name="appPath">Ӧ��·��</param>
        private void CreateDesktopQuick(string desktopPath = "", string quickName = "", string appPath = "")
        {
            List<string> shortcutPaths = this.GetQuickFromFolder(desktopPath, appPath);
            //���û���򴴽�
            if (shortcutPaths.Count < 1)
            {
                this.CreateShortcut(desktopPath, quickName, appPath, "��������", appPath);
            }
        }




        public bool setOpen(Boolean bol)
        {
            try
            {
                if (bol)
                {
                    string strName = AppPaths.Executable;
                    if (!System.IO.File.Exists(strName))//�ж�Ҫ�Զ����е�Ӧ�ó����ļ��Ƿ����
                        return false;
                    string strnewName = strName.Substring(strName.LastIndexOf("\\") + 1);//��ȡӦ�ó����ļ�����������·��
                    RegistryKey registry = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);//����ָ��������
                    if (registry == null)//��ָ�����������
                        registry = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");//�򴴽�ָ��������
                    registry.SetValue(strnewName, strName);//���ø�������µ�"��ֵ��"
                }
                else
                {
                    string strName = AppPaths.Executable;
                    if (!System.IO.File.Exists(strName))//�ж�Ҫȡ����Ӧ�ó����ļ��Ƿ����
                        return false;
                    string strnewName = strName.Substring(strName.LastIndexOf("\\") + 1);///��ȡӦ�ó����ļ�����������·��
                    RegistryKey registry = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);//��ȡָ��������
                    if (registry == null)//��ָ�����������
                        registry = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");//�򴴽�ָ��������
                    registry.DeleteValue(strnewName, false);//ɾ��ָ��"������"�ļ�/ֵ�� 
                }
                return true;
            }
            catch (SecurityException)
            {
                _notifier?.ShowWarning("提示", "没有管理员权限，无法使用这一项高级功能");
                return false;
            }

        }
    }
}
