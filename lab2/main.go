package main

import (
	"bufio"
	"fmt"
	"os"
	"strconv"
	"strings"
	"sync"
	"time"
)

// STUDENT
type Student struct {
	Name  string
	Year  int
	Grade float64
}

func NewStudent(name string, year int, grade float64) *Student {
	return &Student{
		Name:  name,
		Year:  year,
		Grade: grade,
	}
}

// ToString equivalent
func (s Student) String() string {
	return fmt.Sprintf("%-15s | %d | %5.2f", s.Name, s.Year, s.Grade)
}

//

// CALCED STUDENT
type StudentCalced struct {
	Student    Student
	Calculated float64
}

func NewStudentCalced(student Student, calced float64) *StudentCalced {
	return &StudentCalced{
		Student:    student,
		Calculated: calced,
	}
}
func (s StudentCalced) String() string {
	return fmt.Sprintf("%-15s | %d | %5.2f | %v", s.Student.Name, s.Student.Year, s.Student.Grade, s.Calculated)
}

//UTILS

func Function(student Student) float64 {

	result := 0.0
	for i := 0; i < (int(student.Grade) * len(student.Name) * 4000001); i++ {
		result += float64(student.Year)
	}
	return result
}

func ReadStudents(filePath string) ([]Student, error) {
	file, err := os.Open(filePath)
	if err != nil {
		return nil, err
	}
	defer file.Close()

	var students []Student
	scanner := bufio.NewScanner(file)

	for scanner.Scan() {
		line := scanner.Text()
		parts := strings.Split(line, ";")
		if len(parts) == 3 {
			name := strings.TrimSpace(parts[0])
			year, err1 := strconv.Atoi(strings.TrimSpace(parts[1]))
			grade, err2 := strconv.ParseFloat(strings.TrimSpace(parts[2]), 64)

			if err1 == nil && err2 == nil {
				students = append(students, *NewStudent(name, year, grade))
			}

		}
	}

	if err := scanner.Err(); err != nil {
		return nil, err
	}

	return students, nil
}

//

// DATA THREAD
func DataThread() {
	var students = [15]Student{}
	Count := 0
	Status := false
	for state := range DataState {

		//Getting
		if state == 0 {
			Status = <-InitialEmpty
			if Status == true {
				continue
			}
			fmt.Println("getting...")
			if Count >= 14 {
				DataFull <- true
				fmt.Println("data full so continuing...")
				continue
			} else {
				fmt.Println("data not full")
				DataFull <- false
			}

			students[Count] = <-DataReceiveChannel
			fmt.Println("DATA RECEIVED STUDENT :" + students[Count].String())
			Count++

		}
		//Sending
		if state == 1 {
			InitialStatusCarry <- Status
			if Count == 0 {
				DataEmpty <- true

				fmt.Println("data empty so continuing...")
				continue
			} else {
				DataEmpty <- false
			}
			Count--
			fmt.Println("DATA SENT STUDENT :" + students[Count].String())
			DataSendChannel <- students[Count]

		}

	}

}

//

// RESULT THREAD
func ResultThread(wg *sync.WaitGroup) {
	var students = []StudentCalced{}
	var stdnt StudentCalced
	for i := 0; i < 30; i++ {
		fmt.Println("asking for student!!!!!!!!!!!!!!!!!!!!!!!!!!")
		stdnt = <-ResultReceiveChannel
		fmt.Println("student got: " + stdnt.String())
		//fmt.Printf("RESULT THREAD GOT STUDNET CHECKING VALUE!!!! VALUE IS: ")
		//fmt.Println(stdnt)
		if (stdnt != StudentCalced{}) {
			students = append(students, stdnt)
		}

	}
	fmt.Println("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
	//time.Sleep(time.Second * 2)
	wg.Wait()
	for i := 0; i < len(students); i++ {
		//fmt.Println(students[i])
		ResultSendChannel <- students[i]
	}
	close(ResultSendChannel)
}

//

// WORKER THREAD
func Worker(wg *sync.WaitGroup) {
	CheckEmpty := false
	CheckInitialEmpty := false
	//ok := false
	var calced StudentCalced
	var student Student
	for {
		fmt.Println("taking from data...")
		DataState <- 1
		CheckInitialEmpty = <-InitialStatusCarry
		CheckEmpty = <-DataEmpty
		if !CheckEmpty {
			fmt.Println("found data not empty")
			student = <-DataSendChannel
			calced = *NewStudentCalced(student, Function(student))

			if int(calced.Calculated)%2 == 0 {
				fmt.Printf("added: ")
				fmt.Println(student)

				ResultReceiveChannel <- calced

			} else {
				ResultReceiveChannel <- StudentCalced{}
				fmt.Println("not added...")
			}
		} else {
			//CheckInitialEmpty = <-InitialEmpty
			if CheckInitialEmpty {
				fmt.Println("routine ending...!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
				time.Sleep(time.Second * 3)
				wg.Done()
				return
			} else {
				fmt.Println("found data empty so sleeping")
				time.Sleep(time.Second * 1)
			}
		}
	}

}

//

var DataReceiveChannel = make(chan Student)
var DataSendChannel = make(chan Student)
var DataState = make(chan int)
var DataFull = make(chan bool)
var DataEmpty = make(chan bool)

var ResultSendChannel = make(chan StudentCalced)
var ResultReceiveChannel = make(chan StudentCalced)

var InitialEmpty = make(chan bool)
var InitialStatusCarry = make(chan bool)

func main() {
	//s := "gopher"
	//fmt.Printf("Hello and welcome, %s!\n", s)

	Students, _ := ReadStudents("IFF3_10_ZilinskasK_L1_dat_2.txt")

	var threadCount int
	n, err := fmt.Scanln(&threadCount)
	if threadCount > int(len(Students)/4) || threadCount < 2 || err != nil || n != 1 {
		fmt.Println("blogai")
		return
	}
	fmt.Printf("darbininku: ")
	fmt.Println(threadCount)

	var wg sync.WaitGroup
	for i := 0; i < threadCount; i++ {
		wg.Add(1)
		go Worker(&wg)

	}

	go DataThread()
	go ResultThread(&wg)

	CheckFull := false
	for i := 0; i < len(Students); i++ {

		DataState <- 0
		InitialEmpty <- false

		CheckFull = <-DataFull
		if !CheckFull {
			DataReceiveChannel <- Students[i]
		} else {
			time.Sleep(time.Second * 1)
			i--
		}

	}
	for i := 0; i < threadCount; i++ {
		DataState <- 0
		InitialEmpty <- true
	}
	close(InitialEmpty)
	//close(ResultReceiveChannel)
	wg.Wait()
	//close(DataState)
	//close(ResultReceiveChannel)

	fmt.Println("GONNA ASK FO RESULTS AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
	//wg.Wait()
	//time.Sleep(time.Second * 2)
	file, err := os.Create("IFF3_10_ZilinskasK_L1_rez.txt")
	if err != nil {
		fmt.Println("Error creating file:", err)
		return
	}
	defer file.Close()
	writer := bufio.NewWriter(file)

	cnt := 0
	for i := range ResultSendChannel {

		fmt.Printf(fmt.Sprintln(cnt, " ", i))

		line := fmt.Sprintln(cnt, " ", i)
		writer.WriteString(line)

		cnt++
	}

	writer.Flush()
	close(DataState)
	close(ResultReceiveChannel)

}
