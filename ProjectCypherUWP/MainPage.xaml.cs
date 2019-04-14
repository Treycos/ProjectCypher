using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ProjectCypherUWP
{
    public sealed partial class MainPage : Page
    {
        public string[] FrenchWords;
        public int maxKeyLength = 6;

        public MainPage()
        {
            DecryptFiles();
            Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);

            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DecryptFiles();
        }

        private async void DecryptFiles()
        {
            StorageFolder fileFolder = await Package.Current.InstalledLocation.GetFolderAsync("Files");
            StorageFolder outputFolder = ApplicationData.Current.LocalFolder;

            var cryptedFiles = await fileFolder.GetFilesAsync();

            foreach (StorageFile cryptedFile in cryptedFiles)
            {
                var fileBuffer = await FileIO.ReadBufferAsync(cryptedFile);
                var fileData = fileBuffer.ToArray();
                
                string letterFilter = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.,?éàè':()*";
                string common = "easintrluod";
                string keys = "abcdefghijklmnopqrstuvwxyz";
                Regex noiseFilter = new Regex(@"(.)\1{3,}");


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
                
                var outputFile = await outputFolder.CreateFileAsync(cryptedFile.Name, CreationCollisionOption.ReplaceExisting);
                FileIO.WriteTextAsync(outputFile, text);
            }
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

        private void KeyGen()
        {
            int key = 0;
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            Debug.WriteLine($"Max keys: {Math.Pow(chars.Length, 6)}");

            do
            {
                int current = key;
                string gen = "";
                do
                {
                    gen += chars[current % chars.Length];
                    current /= chars.Length;
                } while (current != 0);
                Debug.WriteLine($"{gen}");
                key++;
                //if (key % 10000000 == 0) Debug.WriteLine($"Code: {gen}, N°{key}");
            } while (key < Math.Pow(chars.Length, 6));
        }
    }
}
