using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Numerics;

[Serializable]
class Employee
{
    public string PassportSeries { get; set; }
    public string PassportNumber { get; set; }
    public decimal Salary { get; set; }
    public List<Characteristic> Characteristics { get; set; }

    public Employee(string passportSeries, string passportNumber, decimal salary)
    {
        PassportSeries = passportSeries;
        PassportNumber = passportNumber;
        Salary = salary;
        Characteristics = new List<Characteristic>();
    }

    public void AddCharacteristic(string property, double rating)
    {
        Characteristics.Add(new Characteristic(property, rating));
    }

    public List<Characteristic> GetCharacteristics()
    {
        return Characteristics;
    }

    public override string ToString()
    {
        return $"Passport: {PassportSeries}-{PassportNumber}, Salary: {Salary}";
    }
}

[Serializable]
class Characteristic
{
    public string Property { get; set; }
    public double Rating { get; set; }

    public Characteristic(string property, double rating)
    {
        Property = property;
        Rating = rating;
    }
}

[Serializable]
class EmployeeContainer<T> : IEnumerable<T>
    where T : Employee
{
    public List<T> Employees = new List<T>();

    public List<T> SortEmployeesByPassport()
    {
        List<T> sortedList = Employees.OrderBy(e => e.PassportSeries + e.PassportNumber).ToList();
        return sortedList;
    }

    public List<T> SortEmployeesBySalary()
    {
        List<T> sortedList = Employees.OrderBy(e => e.Salary).ToList();
        return sortedList;
    }

    public void AddEmployee(T employee)
    {
        Employees.Add(employee);
    }

    public void Serialize(string fileName)
    {
        IFormatter formatter = new BinaryFormatter();
        using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            formatter.Serialize(stream, this);
        }
    }

    public static EmployeeContainer<T> Deserialize(string fileName)
    {
        IFormatter formatter = new BinaryFormatter();
        using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return (EmployeeContainer<T>)formatter.Deserialize(stream);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Employees.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void RemoveEmployee(string passportSeries, string passportNumber)
    {
        Employees.RemoveAll(e => e.PassportSeries == passportSeries && e.PassportNumber == passportNumber);
    }

    public List<T> SearchEmployeesByPassport(string passportSeries, string passportNumber)
    {
        return Employees.Where(e => e.PassportSeries == passportSeries && e.PassportNumber == passportNumber).ToList();
    }

    public List<T> SearchEmployeesBySalary(decimal minSalary, decimal maxSalary)
    {
        return Employees.Where(e => e.Salary >= minSalary && e.Salary <= maxSalary).ToList();
    }
}

class Program
{
    static void Main(string[] args)
    {
        EmployeeContainer<Employee> container = new EmployeeContainer<Employee>();
        bool isAutoMode = false;

        foreach (string arg in args)
        {
            if (arg.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                isAutoMode = true;
                break;
            }
        }

        if (isAutoMode)
        {
            AutoAddAndSaveData(container);
        }
        else
        {
            while (true)
            {
                Console.WriteLine("Choose an action:");
                Console.WriteLine("1. Display employees");
                Console.WriteLine("2. Add employee");
                Console.WriteLine("3. Sort employees by salary");
                Console.WriteLine("4. Serialize or deserialize container");
                Console.WriteLine("5. Remove employee by passport");
                Console.WriteLine("6. Search employee by passport");
                Console.WriteLine("7. Exit");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayEmployees(container);
                        break;
                    case "2":
                        AddEmployee(container);
                        break;
                    case "3":
                        DisplaySortedEmployees(container.SortEmployeesBySalary());
                        break;
                    case "4":
                        Console.Write("Enter the file name for serialization or deserialization: ");
                        string fileName = Console.ReadLine();
                        if (File.Exists(fileName))
                        {
                            container = EmployeeContainer<Employee>.Deserialize(fileName);
                            Console.WriteLine($"Data loaded from file '{fileName}'.");
                        }
                        else
                        {
                            container.Serialize(fileName);
                            Console.WriteLine($"Data saved to file '{fileName}'.");
                        }
                        break;
                    case "5":
                        Console.Write("Enter passport series to remove: ");
                        string removeSeries = Console.ReadLine();
                        Console.Write("Enter passport number to remove: ");
                        string removeNumber = Console.ReadLine();
                        container.RemoveEmployee(removeSeries, removeNumber);
                        Console.WriteLine($"Employee with passport series {removeSeries} and passport number {removeNumber} removed.");
                        break;
                    case "6":
                        Console.Write("Enter passport series to search: ");
                        string searchSeries = Console.ReadLine();
                        Console.Write("Enter passport number to search: ");
                        string searchNumber = Console.ReadLine();
                        List<Employee> searchResult = container.SearchEmployeesByPassport(searchSeries, searchNumber);
                        if (searchResult.Count == 0)
                        {
                            Console.WriteLine($"No employees found with passport series {searchSeries} and passport number {searchNumber}");
                        }
                        else
                        {
                            Console.WriteLine($"Employees with passport series {searchSeries} and passport number {searchNumber}:");
                            EmployeeContainer<Employee> searchResultContainer = new EmployeeContainer<Employee>();
                            searchResultContainer.Employees.AddRange(searchResult);
                            DisplayEmployees(searchResultContainer);
                        }
                        break;
                    case "7":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }

    static void AutoAddAndSaveData(EmployeeContainer<Employee> container)
    {
        Employee newEmployee = new Employee("XYZ", "98765", 60000.00m);
        newEmployee.AddCharacteristic("Experience", 5.5);
        container.AddEmployee(newEmployee);
        container.Serialize("test");
        Console.WriteLine("Data added and saved to 'test' file.");
    }

    static void AddEmployee(EmployeeContainer<Employee> container)
    {
        Console.Write("Enter passport series: ");
        string passportSeries = Console.ReadLine();
        Console.Write("Enter passport number: ");
        string passportNumber = Console.ReadLine();
        Console.Write("Enter salary: ");
        decimal salary = decimal.Parse(Console.ReadLine());
        Employee newEmployee = new Employee(passportSeries, passportNumber, salary);

        while (true)
        {
            Console.Write("Add a characteristic (Y/N): ");
            string choice = Console.ReadLine().Trim().ToLower();

            if (choice == "y")
            {
                Console.Write("Enter characteristic property: ");
                string property = Console.ReadLine();
                Console.Write("Enter characteristic rating: ");
                double rating = double.Parse(Console.ReadLine());
                newEmployee.AddCharacteristic(property, rating);
                break;
            }
            else if (choice == "n")
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid choice. Please enter Y or N.");
            }
        }

        container.AddEmployee(newEmployee);
        Console.WriteLine("Employee added.");
    }

    static void DisplayEmployees(EmployeeContainer<Employee> container)
    {
        foreach (Employee employee in container)
        {
            Console.WriteLine(employee);
            List<Characteristic> characteristics = employee.GetCharacteristics();
            foreach (Characteristic characteristic in characteristics)
            {
                Console.WriteLine($"Characteristic: {characteristic.Property}, Rating: {characteristic.Rating}");
            }
        }
    }

    static void DisplaySortedEmployees(List<Employee> employees)
    {
        EmployeeContainer<Employee> sortedContainer = new EmployeeContainer<Employee>();
        sortedContainer.Employees.AddRange(employees);
        DisplayEmployees(sortedContainer);
    }
}
