using IniParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeleteOldFiles
{
    internal class Program
    {
        public static readonly string INI_FILENAME = "_config.ini";

        static void Main(string[] args)
        {
            var IniDataParser = new FileIniDataParser();
            var iniData = IniDataParser.ReadFile(INI_FILENAME);
            var dir = iniData["CONFIG"]["DIR"];
            var dirArray = dir.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var confirmDisabled = false;
            try
            {
                confirmDisabled = int.Parse(iniData["CONFIG"]["NO_CONFIRM"]) > 0;
            }
            catch { }
            int days = 60;
            try
            {
                days = int.Parse(iniData["CONFIG"]["DAYS"]);
            }
            catch { }
            var deleteEmptyDir = false;
            try
            {
                deleteEmptyDir = int.Parse(iniData["CONFIG"]["DELETE_ENPTY_DIR"]) > 0;
            }
            catch { }
            var logEnabled = false;
            try
            {
                logEnabled = int.Parse(iniData["CONFIG"]["LOG"]) > 0;
            }
            catch { }
            Console.WriteLine();
            Console.WriteLine($"Will delete all files over {days} days old in:");
            List<FileInfo> deleteList = new List<FileInfo>();
            var now = DateTime.Now;
            foreach (var s in dirArray)
            {
                var dirPath = s.Trim();
                if (string.IsNullOrEmpty(dirPath)) continue;
                var dInfo = new DirectoryInfo(dirPath);
                if (dInfo.Exists)
                {
                    Logger.WriteLine(dirPath, ConsoleColor.Green);
                    var files = dInfo.GetFiles("*", SearchOption.AllDirectories);
                    foreach (var f in files)
                    {
                        if ((now - f.LastWriteTime).TotalDays > days)
                            deleteList.Add(f);
                    }
                }
                else
                {
                    Logger.WriteLine($"{dirPath}  (Not exist)", ConsoleColor.Red);
                }
            }
            Console.WriteLine();
            int count = deleteList.Count;
            if (count == 0)
            {
                Console.WriteLine($"No old file to be deleted.");
            }
            else
            {
                void doTask()
                {
                    Logger logger = new Logger($"{now:yyyy-MM-dd  HHmmss}");
                    int deletedCount = 0;
                    int deletedFolderCount = 0;
                    for (int i = 0; i < count; i++)
                    {
                        var f = deleteList[i];
                        try
                        {
                            f.Delete();
                            logger.WriteLine($"[{i + 1} / {count}] \t {f.FullName}  [{f.CreationTime:yyyy-MM-dd}]", ConsoleColor.White, logEnabled);
                            deletedCount++;
                            try
                            {
                                f.Directory.Delete();
                                logger.WriteLine($"Deleted Empty Folder: {f.DirectoryName}  [{f.CreationTime:yyyy-MM-dd}]", ConsoleColor.White, logEnabled);
                                deletedFolderCount++;
                            }
                            catch { }
                        }
                        catch (Exception ex)
                        {
                            logger.WriteLine($"[{i + 1} / {count}] \t {f.FullName}  ({ex.Message})", ConsoleColor.Red, logEnabled);
                        }
                    }
                    Logger.WriteLine($"{deletedCount} files and {deletedFolderCount} folders have been deleted.", ConsoleColor.Green);
                }
                if (confirmDisabled)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"{count}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($" files will be deleted...");
                    doTask();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"{count}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($" files will be deleted. Continue?  (Y/N)  ");
                    var c = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    Console.WriteLine();
                    if (c == 'Y' || c == 'y')
                    {
                        doTask();
                    }
                }

            }
            Console.WriteLine();
            Console.WriteLine("Press any key to exit ... ");
            Console.ReadKey();
        }
    }
}
