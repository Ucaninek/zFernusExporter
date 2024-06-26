using FernusSWFExporter;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using SevenZip;
using System;
using System.Diagnostics;
using System.Management;

namespace zFernusExporter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Log(string message)
        {
            TB_Logs.Text += message + Environment.NewLine;
            L_ProcessStatus.Text = message;
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

        private async void B_Start_Click(object sender, EventArgs e)
        {
            TB_Logs.Clear();
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

            foreach (string folder in Directory.EnumerateDirectories(mainFolderPath))
            {
                if (Directory.EnumerateFiles(folder).Any(x => x.Contains("frn.kxk")))
                {
                    publisherName = Path.GetFileName(folder).Split("-")[0];
                    break;
                }
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
                Log("kitap verileri bulunamadı. lutfen z-kitap uygulamasini acmayi deneyin.");
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
            File.Delete(tempSysBPath);
            if (sysB == null)
            {
                Log("kitap verileri okunurken bir hata olustu.");
                return;
            }


            int _width = sysB.Pages![0].Width;
            int _height = sysB.Pages[0].Height;

            Log("desifreleme islemi baslatiliyor..");
            Log($"{sysB.PageCount} sayfa algilandi.");

            string loc = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "tmp");
            Directory.CreateDirectory(loc);
            if (Directory.EnumerateFiles(loc).Any()) Directory.Delete(loc, true);
            Directory.CreateDirectory(loc);

            for (int i = 0; i < sysB.PageCount; i++)
            {
                Log($"[{i + 1} / {sysB.PageCount}]. Desifre ediliyor..");

                Frame page = sysB.Pages[i];
                string pageData = page.Data!;

                dynamic fCode = new
                {
                    f1 = publisherName.Length + 1,
                    f2 = publisherName.Length + 10,
                    f3 = publisherName.Length + 20
                };

                byte[] decryptedData = DecryptPageData(pageData, fCode);
                await File.WriteAllBytesAsync(Path.Combine(loc, $"{i}.swf"), decryptedData);
            }

            Log($"sayfalar duzeltiliyor... ");

            string pArgs = $"-jar swf-dimension.jar -i \"{loc}\" -X {_width} -Y {_height}";
            await Process.Start(new ProcessStartInfo
            {
                FileName = "java.exe",
                Arguments = pArgs,
                UseShellExecute = false,
                CreateNoWindow = true

            })?.WaitForExitAsync();

            Log($"PDF Olusturuluyor, bu 2 dakika kadar surebilir... ");

            string sArgs = $"-jar swf-convert.jar pdf \"{loc}\" -o {Guid.NewGuid()}.pdf --image-format jpg --ignore-empty";
            await Process.Start(new ProcessStartInfo
            {
                FileName = "java.exe",
                Arguments = sArgs,
                UseShellExecute = false,
                CreateNoWindow = true

            })?.WaitForExitAsync();

            Log("Bitti");
        }

        private static string GetSysbLocation()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "sysb.frns");
        }

        private static bool IsFernusZKitapRunning()
        {
            return Process.GetProcessesByName("Fernus-z-Kitap").Length > 0;
        }

        private static int GetFernusZKitapProcessId()
        {
            Process[] processes = Process.GetProcessesByName("Fernus-z-Kitap");
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
    }
}
