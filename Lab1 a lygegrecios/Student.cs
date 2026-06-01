using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Lab1
{
    public class Student
    {

        public string name {  get; set; }
        public int year { get; set; }
        public double grade { get; set; }

        public Student(string name, int year, double grade)
        {
            this.name = name;
            this.year = year;
            this.grade = grade;
        }
        public override string ToString()
        {
                return $"{name,-15}" + " | " + year + " | " + $"{grade,5}";
        }
    }
    public class StudentCalced
    {
        public Student student { get; set; }
        public int calculated { get; set; }

        public StudentCalced(Student student, int calculated)
        {
            this.student = student;
            this.calculated = calculated;
        }
        public override string ToString() 
        {
            return $"{student.name, -15}" + " | " + student.year + " | " + $"{student.grade, 5}" + " | " + calculated;
        }


    }
}
