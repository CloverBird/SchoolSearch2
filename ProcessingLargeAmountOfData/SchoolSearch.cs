using System.Diagnostics;

namespace ProcessingLargeAmountOfData
{
    public class SchoolSearch
    {
        // Список студентів, що будуть зчитані з файлу
        private List<Student> Students;
        private List<Teacher> Teachers;

        // Словник назв команд та дій, що цим командам відповідають
        private readonly Dictionary<char, Action<string[]>> commandsDictionary;

        public SchoolSearch(string studentsFileName, string teachersFileName)
        {
            Students = new List<Student>();
            Teachers = new List<Teacher>();
            ReadFiles(studentsFileName, teachersFileName);

            commandsDictionary = new Dictionary<char, Action<string[]>>
            {
                { 'S', CompleteCommandStudent },
                { 'T', CompleteCommandTeacher },
                { 'C', CompleteCommandClassroom },
                { 'B', CompleteCommandBus },
                { 'G', CompleteCommandGrade }
            };
        }

        #region fillingTheLists
        public void ReadFiles(string studentsFileName, string teachersFileName)
        {
            CheckFilesForExisting(studentsFileName, teachersFileName);

            ReadStudents(studentsFileName);
            ReadTeachers(teachersFileName);
        }

        public void CheckFilesForExisting(string studentsFileName, string teachersFileName)
        {
            // Якщо з ім'я файлу неправильне, або такого файлу немає, то нормально буде кинути виключення
            if (string.IsNullOrWhiteSpace(studentsFileName) || !File.Exists(studentsFileName) 
            || string.IsNullOrWhiteSpace(teachersFileName) || !File.Exists(teachersFileName))
                throw new ArgumentNullException("The files' names is invalid.");
        }

        public void ReadStudents(string studentsFileName)
        {
            var lines = File.ReadAllLines(studentsFileName);

            for (int i = 0; i < lines.Length; i++)
            {
                var dividedFields = DivideLine(lines[i]);
                //якщо рядок пустий, то просто пропускаємо його
                if (dividedFields == null)
                    continue;

                Students.Add(new Student(dividedFields));
            }
        }

        public void ReadTeachers(string teachersFileName)
        {
            var lines = File.ReadAllLines(teachersFileName);

            for (int i = 0; i < lines.Length; i++)
            {
                var dividedFields = DivideLine(lines[i]);
                //якщо рядок пустий, то просто пропускаємо його
                if (dividedFields == null)
                    continue;

                Teachers.Add(new Teacher(dividedFields));
            }
        }

        public static string[] DivideLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            line = line.Replace(" ", "");

            return line.Split(',');
        }
        #endregion

        // Метод, що запускає основні функції програми
        public void Run()
        {
            while (true)
            {
                PrintMenu();

                var answer = Console.ReadLine();
                if (answer == null || answer == string.Empty)
                {
                    Console.WriteLine("\nYou've entered a wrong command.\n");
                    continue;
                }

                if (answer == "Q" || answer == "Quit")
                    break;

                ChooseAndCompleteCommand(answer);
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine("Choose your action (divided by ':'):");
            Console.WriteLine("S[tudent]: <lastName> - find all students with this last name.");
            Console.WriteLine("S[tudent]: <lastName>: B[us]: <busNumber> - find all students with this last name who go to school by this bus route.");
            Console.WriteLine("T[eacher]: <lastName> - find all students those are teached by teacher with this last name");
            Console.WriteLine("C[lassroom]: <number> - find all students in this classroom");
            Console.WriteLine("C[lassroom]: <number>: T[eacher] - find all teachers in this classroom");
            Console.WriteLine("B[us]: <number> - find all students go to school by this bus route.");
            Console.WriteLine("G[rade]: <number> - find all students in that grade.");
            Console.WriteLine("G[rade]: <number>: T[eacher] - find all teachers who teachs in this grade.");
            Console.WriteLine("Q[uit] - quit the program.");
        }

        #region commandsBasePack
        private void ChooseAndCompleteCommand(string answer)
        {
            var command = answer.Replace(" ", "").Split(':');

            if (commandsDictionary.ContainsKey(command[0][0]))
            {
                var stopwatch = new Stopwatch();

                stopwatch.Start();
                commandsDictionary[command[0][0]].Invoke(command);
                stopwatch.Stop();

                Console.WriteLine("Time spent: " + stopwatch.ElapsedMilliseconds + "\n\n");
            }

            else
                Console.WriteLine("\nYou've entered the wrong command. Please choose again\n");
        }

        // метод знаходить студентів у основному полі Students, що підходять за заданою умовою condition
        private List<Student> FindStudents(Func<Student, bool> condition)
        {
            var students = new List<Student>();

            foreach (var student in Students)
                if (condition(student))
                    students.Add(student);

            return students;
        }
        // метод друкує студентів, формат виводу залежить від action, що передається до методу як параметр
        private void PrintStudents(List<Student> students, Action<Student> printFields, bool printTeacher)
        {
            foreach (var student in students)
            {
                if (printTeacher)
                {
                    foreach (var teacher in Teachers)
                        if (teacher.Classroom == student.Classroom)
                        {
                            printFields(student);
                            Console.Write($"{teacher,-20}|\n");
                        }
                }
                else
                {
                    printFields(student);
                    Console.WriteLine();
                }
            }
        }

        private static bool CommandExists(string command, string fullCommandName)
        {
            return !(command.Length > 1 && command != fullCommandName);
        }
        #endregion

        #region commands
        // друкує студентів, що мають задане прізвище
        private void CompleteCommandStudent(string[] command)
        {
            if (!CommandExists(command[0], "Student")) return;

            List<Student> students;
            // Перевіряємо чи є друга команда
            if (command.Length == 4)
            {
                // якщо існує друга команда і вона правильна, то до друкування додаємо колонку "Bus"
                if (command[2][0] == 'B' && CommandExists(command[2], "Bus"))
                {
                    students = FindStudents(student => student.LastName.ToLower() == command[1].ToLower() && student.Bus == int.Parse(command[3]));
                    PrintStudents(students,
                       student => Console.Write($"|{student,-20}|{student.Grade,-2}|{student.Classroom,-4}|{student.Bus,-3}|"), true);
                    return;
                }

                else
                {
                    Console.WriteLine("\nYou've entered the wrong command. Please choose again\n");
                    return;
                }
            }
            // якщо тільки одна команда задана
            students = FindStudents(student => student.LastName.ToLower() == command[1].ToLower());
            PrintStudents(students,
                student => Console.Write($"|{student,-20}|{student.Grade,-2}|{student.Classroom,-4}|"), true);
        }
        // друкує студентів, вчитель яких має задане прізвище
        private void CompleteCommandTeacher(string[] command)
        {
            if (!CommandExists(command[0], "Teacher")) return;

            foreach(var teacher in Teachers)
            {
                if (teacher.LastName.ToLower() == command[1].ToLower())
                {
                    var students = FindStudents(student => student.Classroom == teacher.Classroom);

                    PrintStudents(students,
                        student => Console.Write($"|{student,-20}|{teacher,-20}|"), false);
                }
            }
        }
        // друкує студентів, що їдуть за особовим автобусним маршрутом
        private void CompleteCommandBus(string[] command)
        {
            if (!CommandExists(command[0], "Bus")) return;

            int bus = int.Parse(command[1]);

            var students = FindStudents(student => student.Bus == bus);

            PrintStudents(students,
                student => Console.Write($"|{student,-20}|{student.Grade,-2}|{student.Classroom,-4}|{student.Bus,-3}|"), false);
        }
        // друкує студентів, що навчаються в заданій класній кімнаті, або вчителів, що ведуть у цій класній кімнаті
        private void CompleteCommandClassroom(string[] command)
        {
            if (!CommandExists(command[0], "Classroom")) return;

            int classroom = int.Parse(command[1]);

            if (command.Length == 3)
            {
                if (command[2][0] == 'T' && CommandExists(command[2], "Teacher"))
                    foreach (var teacher in Teachers)
                        if (teacher.Classroom == classroom)
                            Console.WriteLine($"|{teacher,-20}|{teacher.Classroom,-4}|");


                return;
            }

            var students = FindStudents(student => student.Classroom == classroom);

            PrintStudents(students,
                student => Console.Write($"|{student,-20}|{student.Classroom,-4}|"), false);
        }
        //друкує студентів, що навчаються в заданому класі, або вчителів, що в цьому класі навчають
        private void CompleteCommandGrade(string[] command)
        {
            if (!CommandExists(command[0], "Grade")) return;

            int grade = int.Parse(command[1]);
            var students = FindStudents(student => student.Grade == grade);

            if (command.Length == 3)
            {
                if (command[2][0] == 'T' && CommandExists(command[2], "Teacher"))
                    foreach (var teacher in Teachers)
                        foreach (var student in students)
                            if (teacher.Classroom == student.Classroom)
                            {
                                Console.WriteLine($"|{teacher,-20}|{grade,-4}|{teacher.Classroom,-4}|");
                                break;
                            }

                return;
            }

            PrintStudents(students,
                student => Console.Write($"|{student,-21}|{student.Grade,-4}|{student.Classroom,-4}|"), false);
        }
        #endregion
    }
}