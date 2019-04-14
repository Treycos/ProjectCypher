using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bonus
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting decyphering...");

            var p = new Program();
            p.GetExpected();
            p.Pick();
        }

        readonly int Maxlength = 8;
        readonly string ValidChars = "VIGENR";
        SHA1 sha = new SHA1Managed();
        byte[] Expected;

        private void GetExpected()
        {
            Expected = Enumerable.Range(0, "5FEBCC207F0CAA6D9A940696C16A70860BB6B7A3".Length / 2)
                .Select(x => Convert.ToByte("5FEBCC207F0CAA6D9A940696C16A70860BB6B7A3".Substring(x * 2, 2), 16))
                .ToArray();
        }

        private void Pick(int level = 0, string prefix = "")
        {
            level++;
            foreach (char c in ValidChars)
            {
                var guess = prefix + c;
                var rot = guess.Reverse().ToArray();
                var data = rot.Select(let => (char)((let + 13 - 65) % 26 + 65)).ToArray();

                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));

                var correct = hash.Zip(Expected, (a, b) => a == b).All(x => x);

                //Console.WriteLine(prefix + c);
                if(prefix + c == "VIGENERE")
                {
                    var hex = BitConverter.ToString(hash);
                }

                if (level == 1) Console.WriteLine(prefix + c);

                if (correct)
                {
                    Console.WriteLine($"Solution found: {prefix + c}");
                    return;
                }

                if (level < Maxlength) Pick(level, prefix + c);
            }
        }

        private void KeyGen(int max)
        {
            int key = 0;
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUCWXYZ0123456789";
            Console.WriteLine($"Max keys: {Math.Pow(chars.Length, max)}");

            do
            {
                int current = key;
                string gen = "";
                do
                {
                    gen += chars[current % chars.Length];
                    current /= chars.Length;
                } while (current != 0);
                Console.WriteLine($"{gen}");
                key++;
                //if (key % 10000000 == 0) Debug.WriteLine($"Code: {gen}, N°{key}");
            } while (key < Math.Pow(chars.Length, 6));
        }
    }
}
