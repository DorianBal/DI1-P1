using Server.Models;

namespace Server.Persistence.Contracts;

public interface IConsultantsRepository
{
    Task SaveConsultant(Consultant consultant);
    Task<bool> DeleteConsultant(Consultant consultant);
    Task<Consultant?> GetConsultantById(int idConsultant);
    Task<bool> DeleteConsultantById(int? idConsultant);
}
