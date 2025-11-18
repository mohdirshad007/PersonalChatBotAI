using Microsoft.AspNetCore.Mvc;
using TestDockerApp.Model;

namespace TestDockerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private static readonly List<Employee> Employees = new();

    // GET: api/Employee
    [HttpGet]
    public ActionResult<IEnumerable<Employee>> GetAllEmployees()
    {
        return Ok(Employees);
    }

    // GET: api/Employee/{id}
    [HttpGet("{id}")]
    public ActionResult<Employee> GetEmployeeById(int id)
    {
        var employee = Employees.FirstOrDefault(e => e.ID == id);
        if (employee == null)
        {
            return NotFound($"Employee with ID {id} not found.");
        }
        return Ok(employee);
    }

    // POST: api/Employee
    [HttpPost]
    public ActionResult<Employee> CreateEmployee([FromBody] Employee employee)
    {
        if (Employees.Any(e => e.ID == employee.ID))
        {
            return BadRequest($"Employee with ID {employee.ID} already exists.");
        }

        Employees.Add(employee);
        return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.ID }, employee);
    }

    // PUT: api/Employee/{id}
    [HttpPut("{id}")]
    public ActionResult UpdateEmployee(int id, [FromBody] Employee updatedEmployee)
    {
        var employee = Employees.FirstOrDefault(e => e.ID == id);
        if (employee == null)
        {
            return NotFound($"Employee with ID {id} not found.");
        }

        employee.Name = updatedEmployee.Name;
        employee.Designation = updatedEmployee.Designation;
        employee.Salary = updatedEmployee.Salary;
        employee.DepartmentID = updatedEmployee.DepartmentID;

        return NoContent();
    }

    // DELETE: api/Employee/{id}
    [HttpDelete("{id}")]
    public ActionResult DeleteEmployee(int id)
    {
        var employee = Employees.FirstOrDefault(e => e.ID == id);
        if (employee == null)
        {
            return NotFound($"Employee with ID {id} not found.");
        }

        Employees.Remove(employee);
        return NoContent();
    }
}