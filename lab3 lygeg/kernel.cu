#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include <iostream>
#include <fstream>
#include <sstream>
#include <vector>
#include <cstring>
#include <iomanip>
#include <cstdio>
#include <cstdlib>
#include <stdio.h>


struct Student {
    char name[15];   // fixed-length for GPU
    int year;
    double grade;       // int for simplicity

    std::string toString() const {
        std::ostringstream oss;
        oss << std::left << std::setw(15) << name
            << " | " << year
            << " | " << std::right << std::setw(5) << grade;
        return oss.str();
    }
};

// Kernel: convert students to string, only for grade >= 5
__global__ void studentToStringKernelFiltered(Student* students, char* output, int studentCount, int stringLen, int* resultCount)
{
    int idx = blockIdx.x * blockDim.x + threadIdx.x;
    int stride = blockDim.x * gridDim.x;

    for (int i = idx; i < studentCount; i += stride)
    {
        if (students[i].grade >= 5)
        {
            // get index in output array
            int posIndex = atomicAdd(resultCount, 1);
            char* dest = output + posIndex * stringLen;

            // clear destination
            for (int j = 0; j < stringLen; j++) dest[j] = ' ';

            // copy name
            for (int j = 0; j < 15 && students[i].name[j] != '\0'; j++)
                dest[j] = students[i].name[j];

            int pos = 15;

            

            // store year as single char
            dest[pos++] = '0' + students[i].year;
            dest[pos++] = ' ';  // space separator

            // grade
            if (students[i].grade == 10) {
                dest[pos++] = '1';
                dest[pos++] = '0';
            }
            else {
                dest[pos++] = ' ';
                dest[pos++] = '0' + students[i].grade;
               
            }

            // null terminate
            if (pos < stringLen) dest[pos] = '\0';
        }
    }
}

// Host helper function
cudaError_t studentsToStringsFiltered(Student* h_students, int count, char* h_output, int stringLen, int* h_resultCount)
{
    Student* d_students;
    char* d_output;
    int* d_resultCount;
    cudaError_t status;

    cudaMalloc(&d_students, count * sizeof(Student));
    cudaMalloc(&d_output, count * stringLen * sizeof(char));
    cudaMalloc(&d_resultCount, sizeof(int));

    // initialize result count to 0
    cudaMemcpy(d_resultCount, h_resultCount, sizeof(int), cudaMemcpyHostToDevice);

    cudaMemcpy(d_students, h_students, count * sizeof(Student), cudaMemcpyHostToDevice);

    int threadsPerBlock = 32;   //threads in block/////////////////////
    int blocks = 2;             //blocks///////////////////////////////

    studentToStringKernelFiltered <<<blocks, threadsPerBlock >>> (d_students, d_output, count, stringLen, d_resultCount);

    status = cudaDeviceSynchronize();
    if (status != cudaSuccess) return status;

    cudaMemcpy(h_output, d_output, count * stringLen * sizeof(char), cudaMemcpyDeviceToHost);
    cudaMemcpy(h_resultCount, d_resultCount, sizeof(int), cudaMemcpyDeviceToHost);

    cudaFree(d_students);
    cudaFree(d_output);
    cudaFree(d_resultCount);

    return cudaSuccess;
}

int main()
{
    const std::string INPUT_FILE = "C:\\Users\\imkke\\Desktop\\lab3 lygeg\\x64\\Debug\\IFF3_10_ŽilinskasK_L1_dat_1.txt";
    const std::string OUTPUT_FILE = "C:\\Users\\imkke\\Desktop\\lab3 lygeg\\x64\\Debug\\IFF3_10_ŽilinskasK_L1_rez.txt";

   

    std::ifstream fin(INPUT_FILE);
    if (!fin) { std::cerr << "File not found\n"; return 1; }

    std::vector<Student> students;
    std::string line;
    while (std::getline(fin, line)) {
        std::stringstream ss(line);
        std::string part;
        std::vector<std::string> parts;
        while (std::getline(ss, part, ';')) parts.push_back(part);
        if (parts.size() == 3) {
            Student s;
            std::strncpy(s.name, parts[0].c_str(), sizeof(s.name) - 1);
            s.name[sizeof(s.name) - 1] = '\0'; // ensure null-termination
            s.year = std::stoi(parts[1]);
            s.grade = std::stod(parts[2]);
            students.push_back(s);
        }
    }
    fin.close();

    for (const Student& s : students)
        std::cout << s.toString() << std::endl;
    std::cout << "Total students: " << students.size() << "\n";

    const int stringLen = 20; // max length for combined string
    char* output = new char[students.size() * stringLen];
    int h_resultCount = 0;

    cudaError_t status = studentsToStringsFiltered(students.data(), students.size(), output, stringLen, &h_resultCount);
    if (status != cudaSuccess) {
        std::cerr << "CUDA failed!\n";
        return 1;
    }

    remove(OUTPUT_FILE.c_str());
    std::ofstream fout(OUTPUT_FILE, std::ios::app);
   
    std::cout << "\nreuzltatas (grade >= 5):\n";
    for (int i = 0; i < h_resultCount; i++) {
        std::cout << output + i * stringLen << std::endl;
        fout << output + i * stringLen << std::endl;
    }

    delete[] output;
    cudaDeviceReset();
    return 0;
}
