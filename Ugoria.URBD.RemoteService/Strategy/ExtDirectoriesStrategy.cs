using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.RemoteService.Kit;
using Ugoria.URBD.RemoteService.Strategy;
using Ugoria.URBD.Shared;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Ugoria.URBD.Contracts;

namespace Ugoria.URBD.RemoteService.Strategy
{
    public class ExtDirectoriesStrategy : ICommandStrategy
    {
        private ExtDirectoriesContext context;
        private NetworkConnection netConn;

        public NetworkConnection NetConn
        {
            get { return netConn; }
        }

        public ExtDirectoriesContext Context
        {
            get { return context; }
        }
        private volatile bool isInterrupt;

        public bool IsInterrupt
        {
            get { return isInterrupt; }
        }

        private bool isComplete = false;

        public bool IsComplete
        {
            get { return isComplete; }
        }

        private FtpKit ftpKit;

        public ExtDirectoriesStrategy(ExtDirectoriesContext context)
        {
            this.context = context;
            this.ftpKit = new FtpKit(context.FtpAddress, new System.Net.NetworkCredential(context.Username, context.Password), context.FtpDelayTime, context.FtpAttemptCount);
        }

        public void StartLaunch()
        {
            context.StartTime = DateTime.Now;
            context.LaunchGuid = Guid.NewGuid();
        }

        public bool EndLaunch()
        {
            foreach (KeyValuePair<string, string> dirPair in context.Directories)
            {
                CopyFiles(new Uri(string.Format("{0}/{1}", context.FtpAddress, dirPair.Key)),
                    new DirectoryInfo(string.Format(@"{0}\{1}", context.BasePath, dirPair.Value)));
            }
            return true;
        }

        private IEnumerable<ZipEntry> UnzipFile(string path)
        {
            FileInfo zipFileInfo = new FileInfo(path);
            using (ZipFile zipFile = new ZipFile(zipFileInfo.FullName))
            {
                foreach (ZipEntry entry in zipFile.Entries)
                {
                    entry.Extract(zipFileInfo.Directory.FullName, ExtractExistingFileAction.OverwriteSilently);
                    // файл не распакован
                    if (!File.Exists(string.Format(@"{0}\{1}", zipFileInfo.Directory.FullName, entry.FileName)))
                        throw new ZipException("Файл не распакован куда положено");
                    yield return entry;
                }
            }
        }

        private void CopyFiles(Uri ftpPath, DirectoryInfo dirSource)
        {
            if (!dirSource.Exists)
                dirSource.Create();

            foreach (FtpEntry ftpEntry in ftpKit.GetListEntrys(ftpPath))
            {
                if (isInterrupt)
                    break;
                string entryLocalPath = String.Format("{0}/{1}", dirSource.FullName, ftpEntry.Name);
                try
                {
                    if (ftpEntry.Type == FtpEntryType.Directory)
                    {
                        CopyFiles(new Uri(String.Format("{0}/{1}", ftpPath, ftpEntry.Name)), new DirectoryInfo(entryLocalPath));
                        continue;
                    }
                    // попытка скопировать файл
                    try
                    {
                        if (!ftpKit.DownloadFile(ftpEntry, entryLocalPath))
                            continue;
                    }
                    catch (UnauthorizedAccessException ex) // что-то с доступом, попытка изменения атрибутов
                    {
                        LogHelper.Write2Log(ex);
                        FileAttributes attr = File.GetAttributes(entryLocalPath);
                        if (attr.HasFlag(FileAttributes.ReadOnly)) // файл разрешен только на чтение
                        {
                            LogHelper.Write2Log(string.Format("Удаление атрибута \"Только чтение\" у файла {0}", entryLocalPath), LogLevel.Information); 
                            File.SetAttributes(entryLocalPath, attr & ~FileAttributes.ReadOnly); // удаляем атрибут 
                            if (!ftpKit.DownloadFile(ftpEntry, entryLocalPath))
                                continue;
                        }
                    }

                    // определение архива с файлами (сперва по расширению файла - для экономии времени), для последующей распаковки
                    if (entryLocalPath.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase) && Regex.IsMatch(entryLocalPath, "pack[0-9]{8}.zip$"))
                    {
                        foreach (ZipEntry entry in UnzipFile(entryLocalPath))
                        {
                            context.Files.Add(new ExtDirectoriesFile
                            {
                                fileName = ftpEntry.Uri.ToString() + "/" + entry.FileName,
                                fileSize = entry.UncompressedSize,
                                createdDate = entry.LastModified
                            });
                        }
                    }
                    else
                    {
                        context.Files.Add(new ExtDirectoriesFile
                        {
                            fileName = ftpEntry.Uri.ToString(),
                            fileSize = ftpEntry.Size,
                            createdDate = ftpEntry.CreatedTime
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Write2Log(ex);
                    if (ex is WebException)
                    {
                        File.Delete(entryLocalPath);
                    }
                    context.Files.Add(new ExtDirectoriesFile
                    {
                        fileName = ftpEntry.Uri.ToString(),
                        fileSize = 0,
                        createdDate = DateTime.MinValue
                    });
                }
            }
        }

        public void Interrupt()
        {
            isInterrupt = true;
        }

        public void Prepare()
        {
            if (new Uri(context.BasePath).IsUnc)
            {
                //netConn = new NetworkConnection(context.BasePath, new NetworkCredential(context.Username, context.Password));
            }
        }

        public void Conclusion()
        {
            isComplete = true;
            /*if (netConn != null)
                netConn.Dispose();*/
        }
    }
}
