using TestDockerApp.Model;

namespace TestDockerApp.Services;

public class EmployeeService
{
    private readonly List<Employee> _employees = new();

    public IEnumerable<Employee> GetAllEmployees()
    {
        return _employees;
    }

    public Employee? GetEmployeeById(int id)
    {
        return _employees.FirstOrDefault(e => e.ID == id);
    }

    public bool CreateEmployee(Employee employee)
    {
        if (_employees.Any(e => e.ID == employee.ID))
        {
            return false; // Employee with the same ID already exists
        }

        _employees.Add(employee);
        return true;
    }

    public bool UpdateEmployee(int id, Employee updatedEmployee)
    {
        var employee = _employees.FirstOrDefault(e => e.ID == id);
        if (employee == null)
        {
            return false; // Employee not found
        }

        employee.Name = updatedEmployee.Name;
        employee.Designation = updatedEmployee.Designation;
        employee.Salary = updatedEmployee.Salary;
        employee.DepartmentID = updatedEmployee.DepartmentID;

        return true;
    }

    public bool DeleteEmployee(int id)
    {
        var employee = _employees.FirstOrDefault(e => e.ID == id);
        if (employee == null)
        {
            return false; // Employee not found
        }

        _employees.Remove(employee);
        return true;
    }
}