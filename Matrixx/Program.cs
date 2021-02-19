using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text;


namespace MatrixHomeWork
{
    public static class Matrix
    {
        public const int Width = 80; // ширина
        public const int Height = 40; // высота
        public const int Lines = 50; // линии
        public const int StartDelay = 10; // остановка 

        private static async Task LoopLine() // петля линии
        {
            while (true)
            {
                var column = RandomHelper.Rand(0, Width); // получаем рандомное число для столбца


                await MatrixLine.StartNew(column);
            }
        }
        public static async Task Start()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < Lines; i++)
            {
                var task = Task.Run(LoopLine);
                tasks.Add(task);
                await Task.Delay(StartDelay);
            }
        }
    }
    internal class MatrixLine
    {
        private const int MinLength = 3;
        private const int MaxLength = 12;
        private const int MinUpdateTime = 10;
        private const int MaxUpdateTime = 100;

        private const string Symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXUZ*/+-=?~";
        private readonly int Column;

        private readonly int Length;
        private readonly int UpdateTime;
        private char _previous1 = ' ';
        private char _previous2 = ' ';
        private int Row;

        private MatrixLine(int column)
        {
            Length = RandomHelper.Rand(MinLength, MaxLength + 1);
            UpdateTime = RandomHelper.Rand(MinUpdateTime, MaxUpdateTime + 1);
            Column = column;
        }

        public static async Task StartNew(int column)
        {
            var ml = new MatrixLine(column);
            await ml.Start();
        }

        private async Task Start()
        {
            for (var i = 0; i < Matrix.Height + Length; i++)
            {
                Step();
                await Task.Delay(UpdateTime);
            }
        }

        private static bool InBounds(int row)
        {
            return row > 0 && row < Matrix.Height;
        }

        private void Step()
        {
            if (InBounds(Row - 2))
            {
                ConsoleHelper.Display(new ConsoleTask(Column, Row - 2, _previous2, ConsoleColor.DarkGreen));
            }

            if (InBounds(Row - 1))
            {
                ConsoleHelper.Display(new ConsoleTask(Column, Row - 1, _previous1, ConsoleColor.Green));
                _previous2 = _previous1;
            }

            if (InBounds(Row))
            {
                var symbol = Symbols[RandomHelper.Rand(0, Symbols.Length)];
                ConsoleHelper.Display(new ConsoleTask(Column, Row, symbol, ConsoleColor.White));
                _previous1 = symbol;
            }

            if (InBounds(Row - Length))
            {
                ConsoleHelper.Display(new ConsoleTask(Column, Row - Length, ' ', ConsoleColor.Black));
            }

            Row++;
        }
    }
    public class ConsoleTask
    {
        public readonly ConsoleColor Color;
        public readonly int Column;
        public readonly int Row;
        public readonly char Symbol;

        public ConsoleTask(int column, int row, char symbol, ConsoleColor color)
        {
            Color = color;
            Column = column;
            Row = row;
            Symbol = symbol;
        }
    }

    public static class ConsoleHelper
    {
        private static readonly ConcurrentQueue<ConsoleTask> Queue = new ConcurrentQueue<ConsoleTask>();
        private static bool _inProcess;

        static ConsoleHelper()
        {
            Console.CursorVisible = false;
            Console.OutputEncoding = Encoding.UTF8;
        }

        public static void Display(ConsoleTask task)
        {
            Queue.Enqueue(task);
            DisplayCore();
        }

        private static void DisplayCore()
        {
            while (true)
            {
                if (_inProcess)
                {
                    return;
                }

                lock (Queue)
                {
                    if (_inProcess)
                    {
                        return;
                    }

                    _inProcess = true;
                }

                while (Queue.TryDequeue(out var task))
                {
                    Console.SetCursorPosition(task.Column, task.Row);
                    Console.ForegroundColor = task.Color;
                    Console.Write(task.Symbol);
                }

                lock (Queue)
                {
                    _inProcess = false;
                    if (!Queue.IsEmpty)
                    {
                        continue;
                    }
                }

                break;
            }
        }
    }

    public static class RandomHelper
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> Random =
                new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        public static int Rand(int min, int max)
        {
            return Random.Value.Next(min, max);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(Matrix.Width, Matrix.Height);

            var task = Task.Run(Matrix.Start);

            Console.ReadKey();
        }
    }
}

//пример задачи с многопоточностью
/*using System;
using System.Threading;
namespace ThreadingEx 
{
    class Program 
    {
        public static void ThreadProc() 
        {
            for (int i = 0; i < 10; i++) 
            {
                Console.WriteLine($"Поток под номером: {i}");
                Thread.Sleep(0);
            }
        }
        public static void Main() 
        {
            Thread t = new Thread(new ThreadStart(ThreadProc));
            t.Start();
            for (int i = 0; i < 4; i++) 
            {
                Console.WriteLine("Главный поток данных.");
                Thread.Sleep(0);
            }
        }
    }
}*/