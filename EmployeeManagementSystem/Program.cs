using EmployeeManagementSystem.Services;

namespace EmployeeManagementSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("     Welcome to Employee Management System        ");
            Console.WriteLine("==================================================");

            var menu = new EmployeeConsoleMenu();
            menu.Run();

            Console.WriteLine("\nThank you for using Employee Management System. Goodbye!");
        }
    }
}
