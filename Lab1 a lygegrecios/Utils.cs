using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Lab1
{
   
    public class Utils
    {
        
        public static List<Student> ReadStudents(string filePath)
        {
            var students = new List<Student>();
            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(';');
                if (parts.Length == 3)
                {
                    string name = parts[0];
                    int year = int.Parse(parts[1]);
                    double grade = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);

                    students.Add(new Student(name, year, grade));
                }
            }
            return students;
        }

        public static double Function(Student student)
        {
            double result = 0;
            for(int i = 0; i < student.grade * student.name.Length * 4000000; i ++)
            {
                result += student.year;
            }

            return result;
        }
    }
}
