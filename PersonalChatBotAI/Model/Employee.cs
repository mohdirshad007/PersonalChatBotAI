namespace TestDockerApp.Model;

public class Employee
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentID { get; set; }
}