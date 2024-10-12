using Server.Models;

namespace Server.Persistence.Contracts;

public interface IEmployeesRepository
{
    Task SaveEmployee(Employee employee);
    Task FireEmployee(Employee employee);
    Task SaveEmployeeFromConsultant(Consultant employee, int idCompany);
    Task<Employee?> GetEmployeetById(int idemployee);
    Task<bool> DeleteEmployeeById(int? idemployee);
    Task dureetrainingreduceeachturn();
    Task EndOfTraining();
}
