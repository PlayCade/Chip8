using System;
using System.IO;
using System.Threading;
using Cade.Common.Interfaces;

namespace Cade.Chip8
{
    public class Chip8Metadata : ICadeExtension
    {
        private Core.Chip8 _chip8;
        private static int _updatesPerSecond = 25;
        private static int _waitTicks = 1000 / _updatesPerSecond;
        private static int _maxFrameskip = 5;
        private static int _maxUpdatesPerSecond = 60;
        private static int _minWaitTicks = 1000 / _maxUpdatesPerSecond;
        private bool isRunning;
        private Chip8InputManager _inputManager = new Chip8InputManager();
        
        public void Load(string path)
        {
            var game = File.ReadAllBytes(path);
            _chip8 = new Core.Chip8(game);

        }

        public void Run()
        {
            long nextUpdate = Environment.TickCount;
            long lastUpdate = Environment.TickCount;

            isRunning = true;

            while (isRunning)
            {
                while (Environment.TickCount < lastUpdate + _minWaitTicks)
                {
                    Thread.Sleep(0);
                }

                lastUpdate = Environment.TickCount;
                var framesSkipped = 0;
                while (Environment.TickCount > nextUpdate && framesSkipped < _maxFrameskip)
                {
                    _chip8.keys = _inputManager.CheckKeys();
                    // UPDATE HERE
                    nextUpdate += _waitTicks;
                    framesSkipped++;
                }

                // Calculate interpolation for smooth animation between states:
                //var interpolation = ((float)Environment.TickCount + _waitTicks - nextUpdate) / _waitTicks;

                // Render-events:
                // repaint(interpolation);
            }
        }

        public string CoreName { get; }
        public string CoreDescription { get; }
        public string CoreDeveloper { get; }
        public string PlatformName { get; }
        public string PlatformDescription { get; }
        public string PlatformDeveloper { get; }
        public int MaxPlayers { get; }
        public DateTime ReleaseDate { get; }
        public string[] SupportedFileExtensions { get; }

        public Chip8Metadata()
        {
            CoreName = "Chip8";
            CoreDescription = "CHIP-8 Extension for the Cade Arcade System";
            CoreDeveloper = "Cade";
            PlatformName = "CHIP-8";
            PlatformDescription = "Chip 8 Interpreter";
            PlatformDeveloper = "Joseph Weisbecker";
            MaxPlayers = 1;
            ReleaseDate = new DateTime(1970, 01, 01);
            SupportedFileExtensions = new [] {"c8"};
        }
    }
}