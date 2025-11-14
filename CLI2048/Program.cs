using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace CLI2048;

class Program
{
    private const int BoardSize = 4;
    private static readonly int[,] Board = new int[BoardSize, BoardSize];

    private static readonly string[] Colors =
    {
        "[37;40m", // 0: White text on black background
        "[37;41m", // 2: White text on red background
        "[37;42m", // 4: White text on green background
        "[30;43m", // 8: Black text on yellow background
        "[37;44m", // 16: White text on blue background
        "[37;45m", // 32: White text on magenta background
        "[37;46m", // 64: White text on cyan background
        "[30;47m", // 128: Black text on white background
        "[37;41m", // 256: White text on red background
        "[37;42m", // 512: White text on green background
        "[30;43m", // 1024: Black text on yellow background
        "[37;40m", // 2048: White text on black background
    };

    private const string LineEnd = "[0m\n";

    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 4;

//    [DllImport("kernel32.dll")]
 //   private static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

   // [DllImport("kernel32.dll")]
    //private static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

   // [DllImport("kernel32.dll", SetLastError = true)]
    //private static extern nint GetStdHandle(int nStdHandle);

    private static void EnableANSI()
    {
      //  nint handle = GetStdHandle(StdOutputHandle);
    //    GetConsoleMode(handle, out uint mode);
        //mode |= EnableVirtualTerminalProcessing;
      //  SetConsoleMode(handle, mode);
    }

    public static void Main(string[] args)
    {
        bool screensaver = args.Length > 0 && args[0] == "screensaver";
        EnableANSI();

        Random random = new();
        int count = 0;
        while (true)
        {
            int x = random.Next(0, BoardSize), y = random.Next(0, BoardSize);
            while (Board[x, y] != 0)
            {
                x = random.Next(0, BoardSize);
                y = random.Next(0, BoardSize);
            }

            Board[x, y] = random.Next(0, 10) == 0 ? 4 : 2;
            if (!BoardHasValidMove())
                break;
            
            if (screensaver && count++ % 5000 == 0)
            {
                Console.SetCursorPosition(0, 0);
                RenderBoard();
            }

            if (screensaver)
            {
                if (MoveUp()) continue;
                if (MoveLeft()) continue;
                if (MoveRight()) continue;
                MoveDown();
            }
            else
            {
                Console.WriteLine("Use WASD or arrow keys to move");
                bool success = false;
                while (!success)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.W:
                        case ConsoleKey.UpArrow:
                            success = MoveUp();
                            break;
                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                            success = MoveLeft();
                            break;
                        case ConsoleKey.S:
                        case ConsoleKey.DownArrow:
                            success = MoveDown();
                            break;
                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            success = MoveRight();
                            break;
                    }
                }
            }
        }
            
        Console.Clear();
        RenderBoard();
        Console.WriteLine("Game over!");
        int score = Board.Cast<int>().Max();
        int log = (int)Math.Log2(score);
        Console.WriteLine($"Highest tile: {GetColor(log == int.MinValue ? 0 : log)} {score} {LineEnd}");
        Console.ReadKey(true);
    }

    private static void RenderBoard()
    {
        const int cellWidth = 6;
        const int cellHeight = 3;
        string grid = "";

        for (int y = 0; y < BoardSize; y++)
        {
            for (int h = 0; h < cellHeight; h++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    int log = (int)Math.Log2(Board[x, y]);
                    grid += GetColor(log == int.MinValue ? 0 : log);
                    if (h != cellHeight / 2 || Board[x, y] == 0)
                    {
                        grid += "".PadLeft(cellWidth);
                    }
                    else
                    {
                        string num = Board[x, y].ToString();
                        int padOffset = num.Length - 1;
                        string numText = "".PadLeft(Math.Max(0, (cellWidth / 2) - padOffset)) + num;
                        numText = numText.PadRight(cellWidth);
                        if (numText.Length > cellWidth)
                        {
                            numText = numText[..(int)Math.Floor(cellWidth / 2f)] + numText[(int)(numText.Length - Math.Floor(cellWidth / 2f))..];
                        }
                        grid += numText;
                    }
                }

                grid += LineEnd;
            }
        }

        Console.WriteLine(grid);
    }

    private static bool BoardHasValidMove()
    {
        if (Board.Cast<int>().Any(x => x == 0))
            return true;

        for (int x = 0; x < BoardSize; x++)
        for (int y = 0; y < BoardSize; y++)
        {
            // Check for horizontal matches
            if (x + 1 != BoardSize && Board[x, y] == Board[x + 1, y])
                return true;

            // Check for vertical matches
            if (y + 1 != BoardSize && Board[x, y] == Board[x, y + 1])
                return true;
        }

        return false;
    }

    private static bool MoveUp()
    {
        bool success = false;
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                if (Board[x, y] == 0)
                    continue;
                if (y == 0)
                    continue;
                int i = y;
                if (Board[x, y - 1] == 0)
                {
                    while (i > 0 && Board[x, i - 1] == 0)
                        i--;
                    Board[x, i] = Board[x, y];
                    Board[x, y] = 0;
                    success = true;
                }

                if (i == 0)
                    continue;
                if (Board[x, i - 1] == Board[x, i])
                {
                    Board[x, i - 1] *= 2;
                    Board[x, i] = 0;
                    success = true;
                }
            }
        }

        return success;
    }

    private static bool MoveLeft()
    {
        bool success = false;
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (Board[x, y] == 0)
                    continue;
                if (x == 0)
                    continue;
                int i = x;
                if (Board[x - 1, y] == 0)
                {
                    while (i > 0 && Board[i - 1, y] == 0)
                        i--;
                    Board[i, y] = Board[x, y];
                    Board[x, y] = 0;
                    success = true;
                }

                if (i == 0)
                    continue;
                if (Board[i - 1, y] == Board[i, y])
                {
                    Board[i - 1, y] *= 2;
                    Board[i, y] = 0;
                    success = true;
                }
            }
        }

        return success;
    }

    private static bool MoveDown()
    {
        bool success = false;
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = BoardSize - 1; y >= 0; y--)
            {
                if (Board[x, y] == 0)
                    continue;
                if (y == BoardSize - 1)
                    continue;
                int i = y;
                if (Board[x, y + 1] == 0)
                {
                    while (i < BoardSize - 1 && Board[x, i + 1] == 0)
                        i++;
                    Board[x, i] = Board[x, y];
                    Board[x, y] = 0;
                    success = true;
                }

                if (i == BoardSize - 1)
                    continue;
                if (Board[x, i + 1] == Board[x, i])
                {
                    Board[x, i + 1] *= 2;
                    Board[x, i] = 0;
                    success = true;
                }
            }
        }

        return success;
    }

    private static bool MoveRight()
    {
        bool success = false;
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = BoardSize - 1; x >= 0; x--)
            {
                if (Board[x, y] == 0)
                    continue;
                if (x == BoardSize - 1)
                    continue;
                int i = x;
                if (Board[x + 1, y] == 0)
                {
                    while (i < BoardSize - 1 && Board[i + 1, y] == 0)
                        i++;
                    Board[i, y] = Board[x, y];
                    Board[x, y] = 0;
                    success = true;
                }

                if (i == BoardSize - 1)
                    continue;
                if (Board[i + 1, y] == Board[i, y])
                {
                    Board[i + 1, y] *= 2;
                    Board[x, y] = 0;
                    success = true;
                }
            }
        }

        return success;
    }

    private static string GetColor(int number)
    {
        return Colors[Math.Min(number, Colors.Length - 1)];
    }
}
