using FernusSWFExporter;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using SevenZip;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace zFernusExporter
{
    public partial class Form1 : Form
    {
        const int SWF_WIDTH = 612;//651;
        const int SWF_HEIGHT = 785; //850;
        public Form1()
        {
            InitializeComponent();
        }

        private void Log(string message)
        {
            this.Invoke((MethodInvoker)delegate
            {
                TB_Logs.Text += message + Environment.NewLine;
                L_ProcessStatus.Text = message;
            });
        }
        private enum Methods
        {
            Sysb_frns,
            Sys1_dll
        }
        private class SysBType
        {
            [JsonProperty("totalFrames")]
            public int PageCount { get; set; }
            [JsonProperty("frames")]
            public Frame[]? Pages { get; set; }
        }
        private class Frame
        {
            [JsonProperty("width")]
            public int Width { get; set; }
            [JsonProperty("height")]
            public int Height { get; set; }
            [JsonProperty("data")]
            public string? Data { get; set; }
        }
        static byte[] DecryptPageData(string data, dynamic fCode)
        {
            string[] split = data.Split("+/=");
            int n1 = fCode.f1;
            int n2 = fCode.f2;
            int n3 = fCode.f3;

            string base64decoded = FernusMethods.Decode(split[0], (n1 + n2 + n3).ToString()) + split[1];
            byte[] decodedBytes = Convert.FromBase64String(base64decoded);

            byte[] output = FernusMethods.Decrypte(decodedBytes, fCode);
            return output;
        }
        private static async Task LZMADecompressAsync(string inputPath, string outputPath, IProgress<int>? progress)
        {
            SevenZip.Compression.LZMA.Decoder decoder = new();

            using FileStream inputFileStream = new FileStream(inputPath, FileMode.Open);
            using FileStream outputFileStream = new(outputPath, FileMode.OpenOrCreate);
            byte[] properties = new byte[5];
            if (await inputFileStream.ReadAsync(properties, 0, 5) != 5)
            {
                throw new InvalidDataException("Input file is not in the correct format.");
            }

            decoder.SetDecoderProperties(properties);

            long uncompressedSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int b = inputFileStream.ReadByte();
                if (b < 0)
                {
                    throw new InvalidDataException("Input file is not in the correct format.");
                }
                uncompressedSize |= (long)(byte)b << (8 * i);
            }

            long compressedSize = inputFileStream.Length - inputFileStream.Position;
            decoder.Code(inputFileStream, outputFileStream, compressedSize, uncompressedSize, null);
        }
        private static bool isJavaInstalled()
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c java.exe -version",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            p?.WaitForExit();

            return p.ExitCode == 0;
        }
        private async void MethodTwo(int pId)
        {
            Log("Fernus patlatiliyor.. xD");
            Process? fernus = Process.GetProcessById(pId);

            //get fernus exe path and make sure its real
            string? fernusExePath = fernus.MainModule?.FileName;
            if (fernusExePath == null)
            {
                Log("Fernus exe path alinamadi.");
                return;
            }

            //get fernus command line args and make sure its real
            string? commandLineArgs = GetCommandLineArgs(pId);
            if (string.IsNullOrEmpty(commandLineArgs))
            {
                Log("command line args alinamadi. bu iste bir bokluk var hocam");
                return;
            }

            fernus.Kill();

            //async wait for 2 seconds to make sure fernus is killed
            await Task.Delay(2000);

            //remove first arg of command line args which is the exe path
            commandLineArgs = commandLineArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];

            Log(fernusExePath);
            Log(commandLineArgs);

            //relaunch fernus with same args
            fernus = Process.Start(new ProcessStartInfo
            {
                FileName = fernusExePath,
                Arguments = commandLineArgs,
                UseShellExecute = false,
                CreateNoWindow = false
            });

            //get on top before fernus covers the entire screen
            this.TopMost = true;

            Log("fernustan kitap verileri dizlaniyor..");

            //appdata local temp 
            string path = @"C:\";

            //watch for sys1.dll file creation and hook to it so we can get the data before its able to delete it
            string sys1Path = "";
            WaitForChangedResult res = new();

            await Task.Run(() =>
            {
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.IncludeSubdirectories = true;
                watcher.Path = path;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = "sys1.dll";
                watcher.EnableRaisingEvents = true;
                watcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) =>
                {
                    Log("veri yakalandi!! dizlaniyor..");
                    try
                    {
                        FileStream stream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.None);
                        fernus?.Kill();
                        stream.Close();
                    }
                    finally
                    {
                        sys1Path = e.FullPath;
                    }
                });
                //wait for the file to be created or time out after 10 seconds before continuing with the rest of the code
                //on time out log error and return
                res = watcher.WaitForChanged(WatcherChangeTypes.Changed, 15000);
                watcher.Dispose();
            });

            if (res.TimedOut)
            {
                Log("method 2 fail.");
                return;
            }

            //copy sys1.dll over to our app folder for further processing
            string selfFolder = Path.GetDirectoryName(Application.ExecutablePath)!;
            string tempFolder = Path.Combine(selfFolder, "tmp_swf_fix");
            Directory.CreateDirectory(tempFolder);
            //empty dir if not empty
            if (Directory.EnumerateFiles(tempFolder).Any())
            {
                Directory.Delete(tempFolder, true);
                Directory.CreateDirectory(tempFolder);
            }
            string newSys1Path = Path.Combine(tempFolder, "sys1.swf");
            File.Copy(sys1Path, newSys1Path, true);


            Log($"sayfalar duzeltiliyor... ");

            string pArgs = $"-jar swf-dimension.jar -i \"{tempFolder}\" -X {SWF_WIDTH} -Y {SWF_HEIGHT}";
            await Process.Start(new ProcessStartInfo
            {
                FileName = "java.exe",
                Arguments = pArgs,
                UseShellExecute = false,
                CreateNoWindow = false

            })!.WaitForExitAsync();

            Log($"PDF Olusturuluyor, bu 2 dakika kadar surebilir... ");

            string sArgs = $"-jar swf-convert.jar pdf \"{newSys1Path}\" -o {Guid.NewGuid()}.pdf --image-format jpg --ignore-empty";
            await Process.Start(new ProcessStartInfo
            {
                FileName = "java.exe",
                Arguments = sArgs,
                UseShellExecute = false,
                CreateNoWindow = false

            })!.WaitForExitAsync();

            Log("Bitti");

        }
        private async void B_Start_Click(object sender, EventArgs e)
        {
            TB_Logs.Clear();
            Log("Java kontrol ediliyor..");
            if (!isJavaInstalled())
            {
                Log("Java yuklu degil. Lutfen java 21 yukleyin.");
                return;
            }

            Log("Başlatılıyor...");
            if (!IsFernusZKitapRunning())
            {
                Log("Fernus-z-Kitap uygulaması çalıştırılmamış.");
                return;
            }

            Log("Yayinci bilgisi aliniyor..");

            int pId = GetFernusZKitapProcessId();
            string? commandLineArgs = GetCommandLineArgs(pId);
            if (commandLineArgs == null)
            {
                Log("Yayinci bilgisi alinamadi.");
                return;
            }

            string[] args = commandLineArgs.Split('"', StringSplitOptions.RemoveEmptyEntries);
            string mainFolderPath = args[2].Split("||")[0];

            if (!Directory.Exists(mainFolderPath))
            {
                Log("Yayinci bilgisi alinamadi.");
                return;
            }

            string? publisherName = null;
            List<string> publisherFolderCandidates = new();

            foreach (string folder in Directory.EnumerateDirectories(mainFolderPath))
            {
                if (Directory.EnumerateFiles(folder).Any(x => x.Contains("frn.kxk")))
                {
                    publisherFolderCandidates.Add(folder);
                }
            }

            string bookName = "";

            if (publisherFolderCandidates.Count == 1)
            {
                bookName = Path.GetFileName(publisherFolderCandidates[0]);
                publisherName = bookName.Split("-")[0];
            }

            else if (publisherFolderCandidates.Count > 1)
            {
                SelectionBox selectionBox = new();
                selectionBox.items = publisherFolderCandidates;
                selectionBox.ShowDialog();
                if (selectionBox.SelectedItem == null) return;
                bookName = selectionBox.SelectedItem;
                publisherName = bookName.Split("-")[0];
            }

            if (publisherName == null)
            {
                Log("Yayinci bilgisi alinamadi.");
                return;
            }

            Log($"Yayinci bilgisi alindi. ({publisherName})");

            Log("kitap verileri aranıyor...");
            if (!File.Exists(GetSysbLocation()))
            {
                Log("kitap verileri bulunamadı. 2. yontem deneniyor..");
                MethodTwo(pId);
                //Log("kitap verileri bulunamadı. lutfen z-kitap uygulamasini acmayi deneyin.");
                return;
            }

            Log("kitap verileri cikartiliyor..");
            string tempSysBPath = Path.GetTempPath() + Guid.NewGuid() + ".zemi";
            await Task.Run(async () =>
            {
                await LZMADecompressAsync(GetSysbLocation(), tempSysBPath, null);
            });

            Log("kitap verileri okunuyor...");
            string sysBRaw = File.ReadAllText(tempSysBPath);
            SysBType? sysB = JsonConvert.DeserializeObject<SysBType>(sysBRaw);
            if(sysB == null)
            {
                Log("sysb.frns dosyasi kolpa. baska yontem dene");
                return;
            }
            File.Delete(tempSysBPath);
            if (sysB == null)
            {
                Log("kitap verileri okunurken bir hata olustu.");
                return;
            }


            int _width = sysB.Pages![0].Width;
            int _height = sysB.Pages[0].Height;
            //651 850
            //MessageBox.Show(string.Format("Width: {0}, Height: {1}", _width, _height));

            Log("desifreleme islemi baslatiliyor..");
            Log($"{sysB.PageCount} sayfa algilandi.");

            string loc = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "tmp");
            Directory.CreateDirectory(loc);
            if (Directory.EnumerateFiles(loc).Any()) Directory.Delete(loc, true);
            Directory.CreateDirectory(loc);

            Log("please enter your fCode");

            byte[] codes = { 1, 10, 20 };
            if (publisherName == "ari")
            {
                codes = new byte[] { 7, 14, 21 };
            }

            dynamic fCode = new
            {
                f1 = publisherName.Length + codes[0],
                f2 = publisherName.Length + codes[1],
                f3 = publisherName.Length + codes[2]
            };
            //get user input with inputbox.
            string fCodeStr = Interaction.InputBox("Please enter your fCode (default [345] = 1x10x20)", "fCode", "1x10x20");
            //continue if not empty and has 3 parts
            if (!string.IsNullOrEmpty(fCodeStr) && fCodeStr.Split('x').Length == 3)
                fCode = new
                {
                    f1 = publisherName.Length + int.Parse(fCodeStr.Split('x')[0]),
                    f2 = publisherName.Length + int.Parse(fCodeStr.Split('x')[1]),
                    f3 = publisherName.Length + int.Parse(fCodeStr.Split('x')[2])
                };

            for (int i = 0; i < sysB.PageCount; i++)
            {
                L_ProcessStatus.Text = ($"[{i + 1} / {sysB.PageCount}]. Desifre ediliyor..");

                Frame page = sysB.Pages[i];
                string pageData = page.Data!;

                byte[] decryptedData = { };
                try
                {
                    decryptedData = DecryptPageData(pageData, fCode);
                }
                catch
                {
                    Log("desifre basarisiz, obur metodu dene");
                    return;
                }
                await File.WriteAllBytesAsync(Path.Combine(loc, $"{i}.swf"), decryptedData);
            }

            Log($"sayfalar duzeltiliyor... ");

            string pArgs = $"-jar swf-dimension.jar -i \"{loc}\" -X {_width} -Y {_height}";
            await Process.Start(new ProcessStartInfo
            {
                FileName = "java.exe",
                Arguments = pArgs,
                UseShellExecute = false,
                CreateNoWindow = false

            })!.WaitForExitAsync();

            Log($"PDF Olusturuluyor, bu 2 dakika kadar surebilir... ");

            string sArgs = $"-jar swf-convert.jar pdf \"{loc}\" -o {bookName}.pdf --image-format jpg --ignore-empty";
            await Process.Start(new ProcessStartInfo
            {
                FileName = "java.exe",
                Arguments = sArgs,
                UseShellExecute = false,
                CreateNoWindow = false

            })!.WaitForExitAsync();

            Log("Bitti");
        }

        private static string GetSysbLocation()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "sysb.frns");
        }

        private static bool IsFernusZKitapRunning()
        {
            return Process.GetProcessesByName("Fernus z-Kitap").Length > 0;
        }

        private static int GetFernusZKitapProcessId()
        {
            Process[] processes = Process.GetProcessesByName("Fernus z-Kitap");
            if (processes.Length > 0)
            {
                return processes[0].Id;
            }
            return -1;
        }


        private static string? GetCommandLineArgs(int pId)
        {
            string? commandLineArgs = string.Empty;

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + pId))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    commandLineArgs = obj["CommandLine"].ToString();
                    break;
                }
            }

            return commandLineArgs;
        }

        private void Methods_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is not RadioButton b) return;
            switch(b.Name.Replace("RB_", "").ToLower())
            {
                case "sysb":
                    break;
            }
        }
    }
}
