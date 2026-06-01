using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;


namespace Lab1
{
    internal class Program
    {
        public const string DATA1 = "IFF3_10_ŽilinskasK_L1_dat_1.txt";
        public const string DATA2 = "IFF3_10_ŽilinskasK_L1_dat_2.txt";
        public const string DATA3 = "IFF3_10_ŽilinskasK_L1_dat_3.txt";
        public const string RES = "IFF3_10_ŽilinskasK_L1_rez.txt";
        public static int WORK_COUNTER = 0;
        public static int MAX;
        public static int THREAD_COUNT;
        static void Main(string[] args)
        {
            File.Delete(RES);
            List<Student> data = Utils.ReadStudents(DATA1);

            
            DataMonitor dataMonitor = new DataMonitor(data.Count/2);
            ResultMonitor resultMonitor = new ResultMonitor(data.Count);
            List<Thread> threads = new List<Thread>();

            MAX = data.Count;

            File.AppendAllText(RES, "pradiniai, viso: "+MAX+"\n");
            for (int i = 0; i < MAX; i++)
            {
                File.AppendAllText(RES, $"{i,2}. " + data[i].ToString() + "\n");
            }

            //resultMonitor.AddStudent(new StudentCalced(new Student("a",1,3),1));

            Console.WriteLine("kiek giju? (2 - " + (int)data.Count/4 + ")");

            if (int.TryParse(Console.ReadLine(),out THREAD_COUNT))
            {
                for (int i = 0; i < THREAD_COUNT; i++)
                {
                    Thread t = new Thread(() => workerThread(dataMonitor, resultMonitor));
                    threads.Add(t);
                    t.Start();
                }
            }
            else
            {
                Console.WriteLine("netinkamas ivedimas");
            }
            for (int i = 0; i < data.Count; i++)
            {
                dataMonitor.AddStudent(data[i]);
 
            }

            foreach (var t in threads)
            {
                t.Join();
            }
            Console.WriteLine("all threads finished");

            File.AppendAllText(RES, "rezultatai. viso: "+resultMonitor.GetCount()+"\n");
            int j =0;
            while (resultMonitor.GetCount()>0)
            {
                File.AppendAllText(RES, $"{j,2}. " + resultMonitor.GetStudent().ToString() + "\n");
                j++;
            }

        }

        static void workerThread(DataMonitor dataMonitor, ResultMonitor resultMonitor)
        {
            
            while (WORK_COUNTER<=MAX-THREAD_COUNT)
            {
                Student item = dataMonitor.GetStudent();
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " is working on " + item.ToString());

                int num = (int)Utils.Function(item);
                

                
                if (item.name.Length > 5)
                {
                    
                    resultMonitor.AddStudent(new StudentCalced(item,(int)num));
                    //Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("(" + $"{++WORK_COUNTER,2}" + ") " + Thread.CurrentThread.ManagedThreadId + " worked.. " + item.ToString() + " added with num: " + num);
                    //Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    //Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("(" + $"{++WORK_COUNTER,2}" + ") " + Thread.CurrentThread.ManagedThreadId + " worked.. " +item.ToString() + " not added with num: " + num);
                    //Console.ForegroundColor = ConsoleColor.White;
                }
                
            }
            Console.WriteLine("thread " + Thread.CurrentThread.ManagedThreadId + " finished");
        }
    }
}
