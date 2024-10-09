using System.Numerics;

using Microsoft.EntityFrameworkCore;

using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class EmployeesRepository(WssDbContext context) : IEmployeesRepository
{
    public async Task SaveEmployee(Employee employee)
    {
        if (employee.Id is null)
        {
            await context.AddAsync(employee);
        }

        await context.SaveChangesAsync();
    }

    public async Task FireEmployee(Employee employee)
    {
        context.Remove(employee);//peut-être appeler la list employees si erreur

        await context.SaveChangesAsync();
    }

    public async Task SaveEmployeeFromConsultant(Consultant consultant, int idCompany)
    {
        var newConsultant = new Employee(consultant.Name, idCompany, consultant.GameId, consultant.SalaryRequirement);

        foreach (var item in consultant.Skills)
        {
            newConsultant.Skills.Add(item);
        }

        Console.WriteLine("\n\nCode0124 : " + newConsultant.CompanyId);
        await SaveEmployee(newConsultant);
    }
    public async Task<Employee?> GetEmployeetById(int idemployee)
    {
        return await context.Employees.FirstOrDefaultAsync(c => c.Id == idemployee);
    }

    public async Task<bool> DeleteEmployeeById(int? idemployee)
    {
        // Await the task to get the actual consultant object
        var employee = await context.Employees.FirstOrDefaultAsync(c => c.Id == idemployee);

        if (employee == null)
        {
            return false; // or handle the case where the consultant is not found
        }

        context.Employees.Remove(employee);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task EndOfTraining()
    {
        foreach (var employee in context.Employees)
        {
            employee.enformation = false;
        }

        await context.SaveChangesAsync();
    }
}
