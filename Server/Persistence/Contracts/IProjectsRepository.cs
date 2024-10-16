using Server.Models;

namespace Server.Persistence.Contracts;

public interface IProjectsRepository
{
    Task SaveProject(Project projet);
    Task AttributeProjectToCompany(Project project, int idCompany);
    Task<Skill?> GetSkillsById(int? idProject);
    Task<bool> DeleteProjectById(int? idemployee);
    Task<bool> EndOfProject(int? idCompany);
}
