using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskLockingResearch
{
    class Program
    {
        public static object ConsoleWriteLock = new object();

        public static void WriteLine(string str)
        {
            lock (ConsoleWriteLock)
            {
                Console.WriteLine("TaskId:" + Task.CurrentId.Value + " - " + str);
            }
        }

        static void Main(string[] args)
        {
            var lockingService = new LockingService();

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            var task1 = new Task(() =>
            {
                Program.WriteLine("Task1: Attempt to get Guid1");
                using (lockingService.GetLock(guid1))
                {
                    Program.WriteLine("Task1: Got Guid1");
                    Program.WriteLine("Task1: Attempt to get Guid2");
                    using (lockingService.GetLock(guid2))
                    {
                        Program.WriteLine("Task1: Got Guid2");
                    }
                    Program.WriteLine("Task1: Released Guid2");
                    Program.WriteLine("Task1: Attempt to get Guid3");
                    using (lockingService.GetLock(guid3))
                    {
                        Program.WriteLine("Task1: Got Guid3");
                    }
                    Program.WriteLine("Task1: Released Guid3");
                }
                Program.WriteLine("Task1: Released Guid1");
            });

            var task2 = new Task(() =>
            {
                Program.WriteLine("Task2: Attempt to get Global Lock");
                using (lockingService.GetGlobalLock())
                {
                    Program.WriteLine("Task2: Got Global Lock");
                    Thread.Sleep(1000);
                }
                Program.WriteLine("Task2: Released Global Lock");
            });

            var task3 = new Task(() =>
            {
                Program.WriteLine("Task3: Attempt to get Guid1");
                using (lockingService.GetLock(guid1))
                {
                    Program.WriteLine("Task3: Got Guid1");
                }
                Program.WriteLine("Task3: Released Guid1");
            });

            var task4 = new Task(() =>
            {
                Program.WriteLine("Task4: Attempt to get Guid1");
                using (lockingService.GetLock(guid1))
                {
                    Program.WriteLine("Task4: Got Guid1");
                }
                Program.WriteLine("Task4: Released Guid1");
            });

            var task5 = new Task(() =>
            {
                Program.WriteLine("Task5: Attempt to get Guid3");
                using (lockingService.GetLock(guid3))
                {
                    Program.WriteLine("Task5: Got Guid3");
                }
                Program.WriteLine("Task5: Released Guid3");
            });

            var task6 = new Task(() =>
            {

                Program.WriteLine("Task6: Attempt to get Global Lock");
                using (lockingService.GetGlobalLock())
                {
                    Program.WriteLine("Task6: Got Global Lock");
                }
                Program.WriteLine("Task6: Released Global Lock");
            });

            task5.Start();
            task1.Start();
            task2.Start();
            task3.Start();
            task6.Start();
            task4.Start();

            task1.Wait();
            task2.Wait();
            task3.Wait();
            task4.Wait();
            task5.Wait();
            task6.Wait();

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }

}
