using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace Chip8.Engine
{
    public class CPU
    {
        //current opcode
        private ushort opcode;

        //main memory and variables
        private byte[] memory;
        private byte[] v;

        //index register and program counter
        private ushort I;
        private ushort pc;

        public bool[,] screen;
        private bool[] input;

        //stack and stack pointer
        private ushort[] stack;
        private ushort sp;

        private char delayTimer;
        private char soundTimer;

        private Random rand;

        //shortcut for opcodes
        private byte vx;
        private byte vy;

        public bool DrawFlag;

        byte[] chip8_fontset = new byte[]
        {
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

        public CPU()
        {
            memory = new byte[4096];
            v = new byte[16];
            screen = new bool[64,32];
            input = new bool[16];
            stack = new ushort[16];
            rand = new Random();
        }

        public void Initialize()
        {
            ResetCPU();

            LoadRom("Roms/testrom2.ch8");

            Console.WriteLine("rom successfully loaded");
            DrawFlag = true;
        }

        public void ResetCPU()
        {
            pc = 0x200;
            opcode = 0;
            I = 0;
            sp = 0;

            //clear stack
            for (int i = 0; i < stack.Length; i++)
                stack[i] = 0;
            //clear variables
            for (int i = 0; i < v.Length; i++)
                v[i] = 0;
            //clear memory
            for (int i = 0; i < memory.Length; i++)
                memory[i] = 0;

            ClearDisplay();
            LoadFontSet();
        }

        public void LoadRom(string path)
        {
            byte[] fileBytes = File.ReadAllBytes(path);
            LoadRom(fileBytes);
        }

        public void LoadRom(byte[] data)
        {
            for (int b = 0; b < data.Length; b++)
            {
                memory[b + 512] = data[b];
            }
        }

        public void Cycle()
        {
            ReadInput();

            //fetch opcode
            opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);
            pc += 2;
            ExecuteOpcode();

            //update timers
            if (delayTimer > 0)
                --delayTimer;

            if (soundTimer > 0)
            {
                if (soundTimer == 1)
                    Console.Beep();
                --soundTimer;
            }
        }

        #region Display Helper Methods

        private bool SetPixel(int x, int y, bool value)
        {
            x %= 64;
            y %= 32;
            bool was = Get(x, y);
            if(value)
            {
                Set(x, y);
            }
            else
            {
                Unset(x, y);
                if (was)
                    return true;
            }
            return false;
        }

        public bool Get(int x, int y)
        {
            return screen[x, y];
        }

        private void Set(int x, int y)
        {
            int xi = x / 8;
            int xb = x % 8;

            screen[x,y] = true;
        }

        private void Unset(int x, int y)
        {
            int xi = x / 8;
            int xb = x % 8;

            screen[x, y] = false;
        }

        private void ClearDisplay()
        {
            //Console.WriteLine("clearing display");
            for (int x = 0; x < 64; x++)
                for (int y = 0; y < 32; y++)
                    screen[x, y] = false;
        }

        #endregion

        private void ReadInput()
        {
            input[0] = KeyboardInput.Check(Keys.D1);
            input[1] = KeyboardInput.Check(Keys.D2);
            input[2] = KeyboardInput.Check(Keys.D3);
            input[3] = KeyboardInput.Check(Keys.D4);
            input[4] = KeyboardInput.Check(Keys.Q);
            input[5] = KeyboardInput.Check(Keys.W);
            input[6] = KeyboardInput.Check(Keys.E);
            input[7] = KeyboardInput.Check(Keys.R);
            input[8] = KeyboardInput.Check(Keys.A);
            input[9] = KeyboardInput.Check(Keys.S);
            input[10] = KeyboardInput.Check(Keys.D);
            input[11] = KeyboardInput.Check(Keys.F);
            input[12] = KeyboardInput.Check(Keys.Y);
            input[13] = KeyboardInput.Check(Keys.X);
            input[14] = KeyboardInput.Check(Keys.C);
            input[15] = KeyboardInput.Check(Keys.V);
        }

        private void LoadFontSet()
        {
            for (int b = 0; b < chip8_fontset.Length; b++)
            {
                memory[b] = chip8_fontset[b];
            }
        }

        private void UnknownOpcode()
        {
#if DEBUG
            throw new Exception("unknown opcode " + opcode);
#else
            Console.WriteLine("unknown opcode " + opcode);
#endif
        }

#region Opcodes

        private void ExecuteOpcode()
        {
            vx = (byte)((opcode & 0x0F00) >> 8);
            vy = (byte)((opcode & 0x00F0) >> 4);

            switch (opcode & 0xF000)
            {
                case 0x0000:
                    Op0();
                    break;

                case 0x1000:
                    pc = (ushort)(opcode & 0x0FFF);
                    break;

                case 0x2000:
                    stack[sp] = pc;
                    ++sp;
                    pc = (ushort)(opcode & 0x0FFF);
                    break;

                case 0x3000:
                    if (v[vx] == (opcode & 0x00FF))
                        pc += 2;
                    break;

                case 0x4000:
                    if (v[vx] != (opcode & 0x00FF))
                        pc += 2;

                    break;

                case 0x5000:
                    if (v[vx] == v[vy])
                        pc += 2;

                    break;

                case 0x6000:
                    v[vx] = (byte)(opcode & 0x00FF);
                    break;

                case 0x7000:
                    v[vx] += (byte)(opcode & 0x00FF);
                    break;

                case 0x8000:
                    Op8();
                    break;

                case 0x9000:
                    if (v[vx] != v[vy])
                        pc += 2;

                    break;

                case 0xA000:
                    I = (ushort)(opcode & 0x0FFF);
                    break;

                case 0xB000:
                    pc = (ushort)(v[0] + (opcode & 0xFFF));
                    break;

                case 0xC000:
                    v[vx] = (byte)(rand.Next() & (opcode & 0x00FF));
                    break;

                case 0xD000:
                    bool changed = false;
                    for( int y = 0; y < (opcode & 0x000F); y++)
                    {
                        int inc = 7;
                        for (int x = 0; x < 8; x++)
                        {
                            bool value = (memory[I + y] & (1 << (inc % 8))) != 0;
                            //Console.WriteLine(string.Format("reading bit {0} of adress {1} (value: 0x{3:X}): {2}", inc%8,I + y - vy, value,memory[I + y - vy]));
                            changed = changed | SetPixel(x + v[vx], y + v[vy], value);
                            inc--;
                        }

                    }
                    if (changed)
                        v[0xf] = 1;
                    DrawFlag = true;
                    break;

                case 0xE000:
                    OpE();
                    break;

                case 0xF000:
                    OpF();
                    break;

                default:
                    UnknownOpcode();
                    break;
            }
        }

        private void Op0()
        {
            switch (opcode & 0x000F)
            {
                case 0x0000:
                    ClearDisplay();
                    break;
                case 0x000E:
                    --sp;
                    pc = stack[sp];
                    break;
            }
        }

        private void Op8()
        {

            switch (opcode & 0x000F)
            {
                case 0x0000:
                    v[vx] = v[vy];
                    break;
                case 0x0001:
                    v[vx] = (byte)(v[vx] | v[vy]);
                    break;

                case 0x0002:
                    v[vx] = (byte)(v[vx] & v[vy]);
                    break;

                case 0x0003:
                    v[vx] = (byte)(v[vx] ^ v[vy]);
                    break;

                case 0x0004:
                    if(v[vy] > (0xFF - v[vx]))
                    {
                        v[0xF] = 1; //carry
                    }
                    else
                    {
                        v[0xF] = 0;
                    }
                    v[vx] += v[vy];
                    break;

                case 0x0005:
                    if (v[vy] > v[vx])
                    {
                        v[0xF] = 1; //carry
                    }
                    else
                    {
                        v[0xF] = 0;
                    }
                    v[vx] -= v[vy];
                    break;

                case 0x0006:
                    v[0xF] = (byte)(v[vx] & 1);
                    v[vx] >>= 1;
                    break;

                case 0x0007:
                    if(v[vx] - v[vy] < 0)
                    {
                        v[0x0F] = 1;
                    }
                    else
                    {
                        v[0xF] = 0;
                    }
                    v[vx] = (byte)(v[vy] - v[vx]);
                    break;

                case 0x000E:
                    v[0xF] = (byte)(v[vx] & (1 << 7));
                    v[vx] <<= 1;
                    break;

                default:
                    UnknownOpcode();
                    break;
            }
        }

        private void OpE()
        {
            switch (opcode & 0x00FF)
            {
                case 0x009E:
                    if (input[v[vx]])
                        pc += 2;
                    break;

                case 0x00A1:
                    if (!input[v[vx]])
                        pc += 2;
                    break;

                default:
                    UnknownOpcode();
                    break;
            }
        }

        private void OpF()
        {

            switch (opcode & 0x00FF)
            {
                case 0x0007:
                    v[vx] = (byte)delayTimer;
                    break;

                case 0x0015:
                    delayTimer = (char)v[vx];
                    break;

                case 0x0018:
                    soundTimer = (char)v[vx];
                    break;

                case 0x001E:
                    ushort old = I;
                    I += v[vx];
                    if (old < I)
                        v[0x0F] = 1;
                    else
                        v[0x0F] = 0;
                    break;

                case 0x0029:
                    I = (ushort)((opcode & 0x000F) * 5);
                    break;

                case 0x0033:
                    memory[I] = (byte)(v[vx] / 100);
                    memory[I + 1] = (byte)(v[vx] / 10 % 10);
                    memory[I + 2] = (byte)(v[vx] % 100 % 10);
                    break;

                case 0x0055:
                    for( byte i = 0; i <= vx; i++)
                    {
                        memory[I + i] = v[i];
                    }
                    break;

                case 0x0065:
                    for (byte i = 0; i <= vx; i++)
                    {
                        v[i] = memory[I + i];
                    }
                    break;

                default:
                    UnknownOpcode();
                    break;
            }
        }

#endregion

    }
}
