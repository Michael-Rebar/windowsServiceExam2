using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceExam2
{
    public partial class ServiceExam2 : ServiceBase
    {
        FileSystemWatcher fileSystemWatcher;

        public ServiceExam2()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //EventLog.WriteEntry("Exam 2 Service has started.");
            fileSystemWatcher = new FileSystemWatcher("C:\\Folder1")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            fileSystemWatcher.Created += FolderDirectoryChanged;
            fileSystemWatcher.Deleted += FolderDirectoryChanged;
            fileSystemWatcher.Changed += FolderDirectoryChanged;
            fileSystemWatcher.Renamed += FolderDirectoryChanged;
        }

        private void FolderDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            var message = $"{e.ChangeType} - {e.FullPath} {System.Environment.NewLine}";
            var serviceLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            //Duplicate created file from Folder1 to Folder2
            if ($"{e.ChangeType}" == "Created")
            {
                EventLog.WriteEntry("Copy created Folder1 File to Folder2");

                string[] filepaths = Directory.GetFiles("C:\\Folder1");
                foreach(var files in filepaths)
                {
                    string fileName = Path.GetFileName(files);
                    string sourceFile = System.IO.Path.Combine("C:\\Folder1", fileName);
                    string destFile = System.IO.Path.Combine("C:\\Folder2", fileName);

                    try
                    {
                        File.Copy(sourceFile, destFile, true);
                        EventLog.WriteEntry($"{e.ChangeType} - {destFile} {System.Environment.NewLine}");
                        File.AppendAllText($"{serviceLocation}\\logEvents\\log-{DateTime.Now.ToString("yyyy-MM-dd")}.txt", $"{e.ChangeType} - {destFile} {System.Environment.NewLine}");
                    }
                    catch (FileNotFoundException)
                    {
                        EventLog.WriteEntry("File not found.");
                    }
                    catch (UnauthorizedAccessException)
                    { 
                        EventLog.WriteEntry("No access to folder.");
                    }                    
                }
            }

            //Log to daily log file
            File.AppendAllText($"{serviceLocation}\\logEvents\\log-{DateTime.Now.ToString("yyyy-MM-dd")}.txt", message);
            //Log to Event Viewer
            EventLog.WriteEntry(message);
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Service Exam 2 has stopped.");
        }
    }
}
