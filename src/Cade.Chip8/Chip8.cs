﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Cade.Chip8
{
    public class Chip8
    {
        static ushort MemorySize = 4096;
        static ushort RegisterSize = 16;

        public byte[] keys = new byte[16];
        public byte[] Graphics = new byte[64 * 32];

        public byte soundTimer;

        byte[] Registers = new byte[RegisterSize];
        byte[] Memory = new byte[MemorySize];

        ushort opcode;
        ushort pc;
        ushort I;

        public byte delayTimer;        
        public bool drawFlag;

        ushort[] Stack = new ushort[16];
        ushort sp;

        byte[] File;

        byte[] FontSet = {
          0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
          0x20, 0x60, 0x20, 0x20, 0x70, // 1
          0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
          0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
          0x90, 0x90, 0xF0, 0x10, 0x10, // 4
          0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
          0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
          0xF0, 0x10, 0x20, 0x40, 0x40, // 7
          0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
          0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
          0xF0, 0x90, 0xF0, 0x90, 0x90, // A
          0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
          0xF0, 0x80, 0x80, 0x80, 0xF0, // C
          0xE0, 0x90, 0x90, 0x90, 0xE0, // D
          0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
          0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        private Task _task;
        private CancellationTokenSource _cts;

        public Chip8(byte[] file)
        {
            File = file;

            pc = 0x200;
            I = 0;
            delayTimer = 0;
            soundTimer = 0;
            sp = 0;
            opcode = 0;

            for (int i = 0; i < Registers.Length; i++)
            {
                Registers[i] = 0;
            }

            for (int i = 0; i < Memory.Length; i++)
            {
                Memory[i] = 0;
            }

            for (int i = 0; i < Graphics.Length; i++)
            {
                Graphics[i] = 0;
            }

            for (int i = 0; i < Stack.Length; i++)
            {
                Stack[i] = 0;
            }

            for (int i = 0; i < 80; i++)
            {
                Memory[i] = FontSet[i];
            }

            LoadFileIntoMemory();
        }

        void LoadFileIntoMemory()
        {
            if(File == null)
            {
                Console.Error.WriteLine("File not loaded");
                return;
            }

            if(File.Length > (4096 - 512))
            {
                Console.Error.WriteLine("File is too large to load into memory");
                return;
            }

            for (int i = 0; i < File.Length; i++)
            {
                Memory[i + 512] = File[i];
            }
            Console.WriteLine("File loaded into CHIP-8 memory");
        }

        public Task Start(CancellationTokenSource cts)
        {
            if (_task == null)
            {
                _cts = cts;
                CancellationToken token = _cts.Token;

                _task = Task.Factory.StartNew(() =>
                {
                    Stopwatch sw = new Stopwatch ();
                    sw.Start();

                    while (!token.IsCancellationRequested)
                    {
                        if (sw.Elapsed >= TimeSpan.FromSeconds(1.0 / 540))
                        {
                            EmulateCycle();
                            sw.Restart();
                        }
                        Thread.Yield();
                    }
                }, token);
            }
            return _task;
        }

        public void EmulateCycle()
        {
            // Fetch Opcode
            opcode = (ushort)(Memory[pc] << 8 | Memory[pc + 1]);

            // Decode Opcode
            // Execute Opcode
            switch(opcode & 0xF000)
            {
                case 0x0000:
                    switch(opcode & 0x000F)
                    {
                        case 0x0000: // 00E0: Clears the screen.
                            for (int i = 0; i < (64 * 32); i++)
                            {
                                Graphics[i] = 0x0;
                            }
                            drawFlag = true;
                            pc += 2;
                            break;
                        case 0x000E: // 00EE: Returns from a subroutine.
                            sp--;
                            pc = Stack[sp];
                            pc += 2;
                            break;
                    }
                    break;
                case 0x1000: // 1NNN: Jumps to address NNN.
                    pc = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x2000: // 2NNN: Calls subroutine at NNN.
                    Stack[sp] = pc;
                    sp++;
                    pc = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x3000: // 3XNN: Skips the next instruction if VX equals NN. (Usually the next instruction is a jump to skip a code block).
                    if (Registers[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
                    {
                        pc += 4;
                    }
                    else
                    {
                        pc += 2;
                    }
                    break;
                case 0x4000: // 4XNN: Skips the next instruction if VX doesn't equal NN. (Usually the next instruction is a jump to skip a code block).
                    if (Registers[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF))
                    {
                        pc += 4;
                    }
                    else
                    {
                        pc += 2;
                    }
                    break;
                case 0x5000: // 5XY0: Skips the next instruction if VX equals VY. (Usually the next instruction is a jump to skip a code block).
                    if (Registers[(opcode & 0x0F00) >> 8] == Registers[(opcode & 0x00F0) >> 4])
                    {
                        pc += 4;
                    }
                    else
                    {
                        pc += 2;
                    }
                    break;
                case 0x6000: // 6XNN: Sets VX to NN.
                    Registers[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);
                    pc += 2;
                    break;
                case 0x7000: // 7XNN: Adds NN to VX. (Carry flag is not changed).
                    Registers[(opcode & 0x0F00) >> 8] += (byte)(opcode & 0x00FF);
                    pc += 2;
                    break;
                case 0x8000:
                    switch(opcode & 0x000F)
                    {
                        case 0x0000: // 8XY0: Sets VX to the value of VY.
                            Registers[(opcode & 0x0F00) >> 8] = Registers[(opcode & 0x00F0) >> 4];
                            pc += 2;
                            break;
                        case 0x0001: // 8XY1: Sets VX to VX or VY. (Bitwise OR operation).
                            Registers[(opcode & 0x0F00) >> 8] |= Registers[(opcode & 0x00F0) >> 4];
                            pc += 2;
                            break;
                        case 0x0002: // 8XY2: Sets VX to VX and VY. (Bitwise AND operation)
                            Registers[(opcode & 0x0F00) >> 8] &= Registers[(opcode & 0x00F0) >> 4];
                            pc += 2;
                            break;
                        case 0x0003: // 8XY3: Sets VX to VX xor VY.
                            Registers[(opcode & 0x0F00) >> 8] ^= Registers[(opcode & 0x00F0) >> 4];
                            pc += 2;
                            break;
                        case 0x0004: // 8XY4: Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                            if(Registers[(opcode & 0x00F0) >> 4] > (0xFF - Registers[(opcode & 0x0F00) >> 8]))
                            {
                                Registers[0xF] = 1;
                            } 
                            else
                            {
                                Registers[0xF] = 0;
                            }
                            Registers[(opcode & 0x0F00) >> 8] += Registers[(opcode & 0x00F0) >> 4];
                            pc += 2;
                            break;
                        case 0x0005: // 8XY5: VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            if(Registers[(opcode & 0x00F0) >> 4] > Registers[(opcode & 0x0F00) >> 8])
                            {
                                Registers[0xF] = 0;
                            }
                            else
                            {
                                Registers[0xF] = 1;    
                            }
                            Registers[(opcode & 0x0F00) >> 8] -= Registers[(opcode & 0x00F0) >> 4];
                            pc += 2;
                            break;
                        case 0x0006: // 8XY6: Shifts VY right by one and stores the result to VX (VY remains unchanged). VF is set to the value of the least significant bit of VY before the shift.
                            Registers[0xF] = (byte)(Registers[(opcode & 0x0F00) >> 8] & 0x1);
                            Registers[(opcode & 0x0F00) >> 8] = (Registers[(opcode & 0x00F0) >> 8] >>= 1);
                            pc += 2;
                            break;
                        case 0x0007: // 8XY7: Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            if(Registers[(opcode & 0x0F00) >> 8] > Registers[(opcode & 0x00F0) >> 4])
                            {
                                Registers[0xF] = 0;
                            }
                            else 
                            {
                                Registers[0xF] = 1;
                            }
                            Registers[(opcode & 0x0F00) >> 8] = (byte)(Registers[(opcode & 0x00F0) >> 4] - Registers[(opcode & 0x0F00) >> 8]);
                            pc += 2;
                            break;
                        case 0x000E: // 8XYE: Shifts VY left by one and copies the result to VX. VF is set to the value of the most significant bit of VY before the shift.
                            Registers[0xF] = (byte)(Registers[(opcode & 0x0F00) >> 8] >> 7);
                            Registers[(opcode & 0x0F00) >> 8] = (Registers[(opcode & 0x00F0) >> 4] <<= 1);
                            pc += 2;
                            break;
                    }
                    break;
                case 0x9000: // 9XY0: Skips the next instruction if VX doesn't equal VY. (Usually the next instruction is a jump to skip a code block).
                    if (Registers[(opcode & 0x0F00) >> 8] != Registers[(opcode & 0x00F0) >> 4])
                    {
                        pc += 4;
                    }
                    else
                    {
                        pc += 2;
                    }
                    break;
                case 0xA000: // ANNN: Sets I to the address NNN.
                    I = (ushort)(opcode & 0x0FFF);
                    pc += 2;
                    break;
                case 0xB000: // BNNN: Jumps to the address NNN plus V0.
                    pc = (ushort)((opcode & 0x0FFF) + Registers[0]);
                    break;
                case 0xC000: // CXNN: Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and NN.
                    Random rnd = new Random();
                    var randNum = (ushort)rnd.Next(256);
                    Registers[(opcode & 0x0F00) >> 8] = (byte)(randNum & (opcode & 0x00FF));
                    pc += 2;
                    break;
                case 0xD000:
                    ushort x = Registers[(opcode & 0x0F00) >> 8];
                    ushort y = Registers[(opcode & 0x00F0) >> 4];
                    ushort height = (ushort)(opcode & 0x000F);
                    ushort pixel;

                    Registers[0xF] = 0;
                    for (int yline = 0; yline < height; yline++)
                    {
                        pixel = Memory[I + yline];
                        for (int xline = 0; xline < 8; xline++)
                        {
                            if ((pixel & (0x80 >> xline)) != 0)
                            {
                                if (Graphics[(x + xline + ((y + yline) * 64))] == 1)
                                    Registers[0xF] = 1;
                                Graphics[x + xline + ((y + yline) * 64)] ^= 1;
                            }
                        }
                    }

                    drawFlag = true;
                    pc += 2;
                    break;
                case 0xE000:
                    switch(opcode & 0x000F)
                    {
                        case 0x000E: // EX9E: Skips the next instruction if the key stored in VX is pressed. (Usually the next instruction is a jump to skip a code block).
                            if(keys[Registers[(opcode & 0x0F00) >> 8]] != 0)
                            {
                                pc += 4; 
                            }
                            else
                            {
                                pc += 2;
                            }                                    
                            break;
                        case 0x0001: // EXA1: Skips the next instruction if the key stored in VX isn't pressed. (Usually the next instruction is a jump to skip a code block)
                            if(keys[Registers[(opcode & 0x0F00) >> 8]] == 0)
                            {
                                pc += 4; 
                            }
                            else
                            {
                                pc += 2;
                            }    
                            break;
                    }
                    break;
                case 0xF000:
                    switch(opcode & 0x00FF)
                    {
                        case 0x0007: // FX07: Sets VX to the value of the delay timer.
                            Registers[(opcode & 0x0F00) >> 8] = delayTimer;
                            pc += 2;
                            break;
                        case 0x000A: // FX0A: A key press is awaited, and then stored in VX. (Blocking Operation. All instruction halted until next key event)
                            var keyPressed = false;

                            for (int i = 0; i < keys.Length; i++)
                            {
                                if(keys[i] != 0)
                                {
                                    Registers[(opcode & 0x0F00) >> 8] = (byte)i;
                                    keyPressed = true;
                                    break;
                                }
                            }
                            if (!keyPressed)
                            {
                                return;
                            }
                            pc += 2;
                            break;
                        case 0x0015: // FX15: Sets the delay timer to VX.
                            delayTimer = Registers[(opcode & 0x0F00) >> 8];
                            pc += 2;
                            break;
                        case 0x0018: // FX18: Sets the sound timer to VX.
                            soundTimer = Registers[(opcode & 0x0F00) >> 8];
                            pc += 2;
                            break;
                        case 0x001E: // FX1E: Adds VX to I.
                            if (I + Registers[(opcode & 0x0F00) >> 8] > 0xFFF)
                            {
                                Registers[0xF] = 1;
                            }
                            else
                            {
                                Registers[0xF] = 0;
                            }
                            I += Registers[(opcode & 0x0F00) >> 8];
                            pc += 2;
                            break;
                        case 0x0029: // FX29: Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                            I = (ushort)(Registers[(opcode & 0x0F00) >> 8] * 0x5);
                            pc += 2;
                            break;
                        case 0x0033: // FX33: Stores the Binary-coded decimal representation of VX at the addresses I, I plus 1, and I plus 2
                            Memory[I] = (byte)(Registers[(opcode & 0x0F00) >> 8] / 100);
                            Memory[I + 1] = (byte)((Registers[(opcode & 0x0F00) >> 8] / 10) % 10);
                            Memory[I + 2] = (byte)((Registers[(opcode & 0x0F00) >> 8] % 100) % 10);
                            pc += 2;
                            break;
                        case 0x0055: // FX55: Stores V0 to VX (including VX) in memory starting at address I. I is increased by 1 for each value written.
                            for (int i = 0; i <= ((opcode & 0x0F00) >> 8); i++)
                                Memory[I + i] = Registers[i];
                            
                            I += (ushort)(((opcode & 0x0F00) >> 8) + 1);
                            pc += 2;
                            break;
                        case 0x0065: // FX65: Fills V0 to VX (including VX) with values from memory starting at address I. I is increased by 1 for each value written.
                            for (int i = 0; i <= ((opcode & 0x0F00) >> 8); ++i)
                                Registers[i] = Memory[I + i];

                            I += (ushort)(((opcode & 0x0F00) >> 8) + 1);
                            pc += 2;
                            break;
                    }
                    break;
                default:
                    Console.WriteLine("Unimplemented OpCode");
                    throw new NotImplementedException();
                    break;
            }
        }


    }
}
