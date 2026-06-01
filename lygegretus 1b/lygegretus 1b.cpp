#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <sstream>
#include <iomanip>
#include <omp.h>
#include <algorithm>
#include <memory>

struct Student {
    std::string name;
    int year;
    double grade;

    std::string toString() const {
        std::ostringstream oss;
        oss << std::left << std::setw(15) << name
            << " | " << year
            << " | " << std::right << std::setw(5) << grade;
        return oss.str();
    }
};

struct CalcedStudent {
    Student originalData;
    double computedValue;

    std::string toString() const {
        std::ostringstream oss;
        oss << std::left << std::setw(15) << originalData.name
            << " | " << originalData.year
            << " | " << std::right << std::setw(5) << originalData.grade
            << " | " << computedValue;
        return oss.str();
    }
};

struct result_monitor {
    std::unique_ptr<CalcedStudent[]> arr;
    int count = 0;
    int capacity;

    result_monitor(int size) : capacity(size) {
        arr = std::make_unique<CalcedStudent[]>(size);
    }

    void addItemSorted(const CalcedStudent& item) {
#pragma omp critical
        {
            int pos = 0;
            while (pos < count && arr[pos].computedValue < item.computedValue) pos++;

           
            for (int i = count; i > pos; i--) {
                arr[i] = arr[i - 1];
            }

            arr[pos] = item;
            count++;
        }
    }

    std::vector<CalcedStudent> getItems() const {
        std::vector<CalcedStudent> copy;
        copy.reserve(count);
        for (int i = 0; i < count; i++) {
            copy.push_back(arr[i]);
        }
        return copy;
    }
};

int Compute(const Student& s) {
    long long iterations = static_cast<long long>(s.grade * s.name.length() * 4000000);
    long long result = 0;
    for (long long i = 0; i < iterations; i++) {
        result += s.year;
    }
    return static_cast<int>(result);
}

int main() {
    const std::string INPUT_FILE = "C:\\Users\\imkke\\Desktop\\lygegretus 1b\\lygegretus 1b\\x64\\Debug\\IFF3_10_ŽilinskasK_L1_dat_1.txt";
    const std::string OUTPUT_FILE = "C:\\Users\\imkke\\Desktop\\lygegretus 1b\\lygegretus 1b\\x64\\Debug\\IFF3_10_ŽilinskasK_L1_rez.txt";

    std::ifstream fin(INPUT_FILE);
 

    std::vector<Student> data;
    std::string line;
    while (std::getline(fin, line)) {
        std::stringstream ss(line);
        std::string part;
        std::vector<std::string> parts;
        while (std::getline(ss, part, ';')) parts.push_back(part);
        if (parts.size() == 3) {
            Student s;
            s.name = parts[0];
            s.year = std::stoi(parts[1]);
            s.grade = std::stod(parts[2]);
            data.push_back(s);
        }
    }
    fin.close();

    int n = static_cast<int>(data.size());
    int num_threads = 0;
    std::cout << "kiek giju? (2-" << n / 4 << ")\n";
    std::cin >> num_threads;
    std::cout << "giju skaicius pasirinktas...\n";

    if (num_threads <= 1 || num_threads > n / 4) {
        std::cout << "netinkamas giju kiekis\n";
        return 1;
    }

    std::vector<int> start(num_threads);
    std::vector<int> end(num_threads);
    int base = n / num_threads;
    int remainder = n % num_threads;
    int idx = 0;
    for (int i = 0; i < num_threads; i++) {
        start[i] = idx;
        int size = base + (i < remainder ? 1 : 0);
        idx += size;
        end[i] = idx;
    }

    result_monitor results(n);
    int sum_year = 0;
    double sum_grade = 0.0;

#pragma omp parallel num_threads(num_threads)
    {
        int tid = omp_get_thread_num();
        int local_sum_year = 0;
        double local_sum_grade = 0.0;

        for (int i = start[tid]; i < end[tid]; i++) {
            int val = Compute(data[i]);
            if (val % 2 == 0) {

                CalcedStudent cs{ data[i], val };
                results.addItemSorted(cs);

                
                std::cout << cs.toString()<< " added by " << tid << "\n";

                local_sum_year += data[i].year;
                local_sum_grade += data[i].grade;
            }
            else {
                std::cout << data[i].toString() << " " << val << " not added by " << tid << "\n";
            }
        }

#pragma omp critical
        {
            sum_year += local_sum_year;
            sum_grade += local_sum_grade;
        }
    }

    std::ofstream fout(OUTPUT_FILE);


    for (auto& r : results.getItems()) {
        fout << r.toString() << "\n";
    }

    fout << "suma metu: " << sum_year << "\n";
    fout << "suma pazymiu: " << sum_grade << "\n";

    fout.close();
    return 0;
}