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
}
