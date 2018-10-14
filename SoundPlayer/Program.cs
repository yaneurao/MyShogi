using System;
using System.Collections.Generic;
using System.IO;
using SoundPlayer.Sound;

namespace SoundPlayer
{
    class FileManager
    {
        public FileManager(string path)
        {
            _entries = new HashSet<string>(
                Directory.GetFiles(path, "*.wav", SearchOption.AllDirectories));
            _files = new Dictionary<string, WaveFile>();
        }

        public WaveFile Load(string path)
        {
            if (!_entries.Contains(path))
            {
                return null;
            }

            try
            {
                return _files[path];
            }
            catch (KeyNotFoundException)
            {
                var file = new WaveFile(path);
                _files[path] = file;
                return file;
            }
        }

        private HashSet<string> _entries;
        private Dictionary<string, WaveFile> _files;
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: SoundServer.exe [sound dir]");
                Environment.Exit(1);
            }

            var manager = new FileManager(args[0]);
            Listen(manager);
        }

        private static void Listen(FileManager manager)
        {
            var player = new Player();
            var line = Console.ReadLine();
            while (line != null)
            {
                line = line.Trim();
                if (line == "exit")
                {
                    break;
                }

                var file = manager.Load(line);
                if (file != null)
                {
                    player.Play(file);
                }

                line = Console.ReadLine();
            }
        }
    }
}
