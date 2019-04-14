using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Decypher
{
    class Program
    {
        public int maxKeyLength = 6;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Choose the directory where the cypted files are located...");

            new Program().DecryptFiles();

            Console.ReadKey();
        }
        
        private void DecryptFiles()
        {
            FolderBrowserDialog fdg = new FolderBrowserDialog();

            if (fdg.ShowDialog() != DialogResult.OK) return;

            var decypheredDir = Directory.CreateDirectory(fdg.SelectedPath + "/Decyphered");

            foreach (string cryptedFile in Directory.GetFiles(fdg.SelectedPath))
            {
                var fileData = File.ReadAllBytes(cryptedFile);

                string letterFilter = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.,?éàè':()*";
                string common = "easintrluod";
                string keys = "abcdefghijklmnopqrstuvwxyz";
                
                var result = Enumerable
                    .Range(1, maxKeyLength)
                    .Select(keyIndex => keys.Select(guess => new
                    {
                        Guess = guess,
                        Letters = PartialDecrypt(fileData, keyIndex, guess)
                            .GroupBy(x => x)
                            .OrderByDescending(x => x.Count())
                            .Select(x => x.Key)
                            .ToArray()
                    })
                    .OrderBy(x => common.Select(c => new string(x.Letters).IndexOf(c)).Max())
                    .Where(x => x.Letters.Take(10).All(y => letterFilter.Contains(y)))
                    .First())
                .Select(x => x.Guess)
                .ToArray();

                var text = XORFile(new string(result), fileData);

                var writer = File.CreateText(decypheredDir.FullName + "/" + Path.GetFileName(cryptedFile));
                writer.Write(text);
                writer.Close();

                Console.WriteLine($"File : {Path.GetFileNameWithoutExtension(cryptedFile)}, Key : {new string(result)}");
            }

            System.Diagnostics.Process.Start("explorer.exe", fdg.SelectedPath);
        }

        private string PartialDecrypt(byte[] fileData, int keyIndex, char guess)
        {
            string val = Encoding.ASCII.GetString(Enumerable
                .Range(0, fileData.Length / maxKeyLength)
                .Select(x => x * maxKeyLength + keyIndex - 1)
                .Select(index => (byte)(fileData[index] ^ guess))
                .ToArray());

            return val.ToLower();
        }

        private string XORFile(string key, byte[] file)
        {
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var res = file.Select((ch, i) => (byte)(ch ^ key[i % key.Length])).ToArray();

            return Encoding.ASCII.GetString(res);
        }
    }
}