using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab1
{
    class DataMonitor
    {
       
        private Student[] students;
        private int count;
        private int size;

        private readonly object _lock = new object();


        public DataMonitor(int num)
        {
            students = new Student[num];
            count = 0;
            size = num;
        }
        public void AddStudent(Student student)
        {

            lock(_lock){
                while (count >= size)
                {
                    Monitor.Wait(_lock);
                }
                students[count] = student;
                count++;
                Monitor.Pulse(_lock);
            }
            
        }


        public Student GetStudent()
        {

            lock (_lock)
            {
                while (count <= 0)
                {
                    Monitor.Wait(_lock);

                }
                count--;
                Student rtrn = students[count];
                students[count] = null;
                

                Monitor.Pulse(_lock);
                return rtrn;
            }

        }

    }

    class ResultMonitor
    {
        StudentCalced[] students;
        int count;
        int size;

        private readonly object _lock = new object();

        public ResultMonitor(int num)
        {
            size = num;
            this.students = new StudentCalced[size];
            this.count = 0;
        }
        public int GetCount()
        {
            return count;
        }
        public void AddStudent(StudentCalced student)
        {
           
            lock (this._lock)
            {

                int iter = 0;
                while(iter < count)
                {
                    if(student.calculated > students[iter].calculated)
                    {                      
                        for (int i = count; i>iter; i--)
                        {
                            students[i] = students[i-1];
                        }
                        students[iter] = student;
                        count++;
                        Monitor.Pulse(_lock);
                        return;
                    }
                    iter++;
                }
                students[count] = student;
                count++;
                Monitor.Pulse(_lock);
            }
        }

        public StudentCalced GetStudent()
        {
            lock (this._lock)
            {
               
                while(count == 0) Monitor.Wait(_lock);
                

                count--;
                StudentCalced rtrn = students[count];
                students[count] = null;

                Monitor.Pulse(_lock);
                return rtrn;
            }
        }
    }
}
