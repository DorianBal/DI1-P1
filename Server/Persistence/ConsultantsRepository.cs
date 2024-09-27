
using Server.Models;
using Server.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
namespace Server.Persistence;

public class ConsultantsRepository(WssDbContext context) : IConsultantsRepository
{
    public async Task SaveConsultant(Consultant consultant)
    {
        if (consultant.Id is null)
        {
            await context.AddAsync(consultant);
        }

        await context.SaveChangesAsync();
    }

    public async Task<bool> DeleteConsultant(Consultant consultant)
    {
        context.Consultants.Remove(consultant);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<Consultant?> GetConsultantById(int idConsultant)
    {
        return await context.Consultants.FirstOrDefaultAsync(c => c.Id == idConsultant);
    }

    public async Task<bool> DeleteConsultantById(int? idConsultant)
    {
        // Await the task to get the actual consultant object
        var consultant = await context.Consultants.FirstOrDefaultAsync(c => c.Id == idConsultant);

        if (consultant == null)
        {
            return false; // or handle the case where the consultant is not found
        }

        context.Consultants.Remove(consultant);
        await context.SaveChangesAsync();

        return true;
    }
}
