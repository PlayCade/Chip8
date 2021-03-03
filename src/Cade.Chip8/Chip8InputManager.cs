using System;
using System.Collections.Generic;
using Cade.Common.Interfaces;
using Cade.Common.Models;

namespace Cade.Chip8
{
    public sealed class Chip8InputManager : ICadeInputManager
    {
        private static Lazy<List<Input>> _lazy = new Lazy<List<Input>>(() => new List<Input>
        {
            new Input
                {
                    Name = "Button 1",
                    Active = false,
                    Number = 1,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button 2",
                    Active = false,
                    Number = 2,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button 3",
                    Active = false,
                    Number = 3,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button 4",
                    Active = false,
                    Number = 4,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button 5",
                    Active = false,
                    Number = 5,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button 6",
                    Active = false,
                    Number = 6,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button 7",
                    Active = false,
                    Number = 7,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button 8",
                    Active = false,
                    Number = 8,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button 9",
                    Active = false,
                    Number = 9,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button A",
                    Active = false,
                    Number = 10,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button B",
                    Active = false,
                    Number = 11,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button C",
                    Active = false,
                    Number = 12,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button D",
                    Active = false,
                    Number = 13,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button E",
                    Active = false,
                    Number = 14,
                    Player = 1,
                    Type = InputType.Button
                },
                new Input
                {
                    Name = "Button F",
                    Active = false,
                    Number = 15,
                    Player = 1,
                    Type = InputType.Button
                }
        });

        public List<Input> Inputs()
        {
            return _lazy.Value;
        }

        public void Update(List<Input> inputs)
        {
            
            _lazy = new Lazy<List<Input>>(() => inputs);
        }

        int ICadeInputManager.MaxPlayers()
        {
            return 1;
        }
    }
}