using System;
using Cade.Common.Interfaces;

namespace Cade.Chip8
{
    public class Chip8Metadata : ICadeMetadata
    {
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