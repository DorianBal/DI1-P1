using System.Numerics;
using System.Reflection.Metadata;

using Microsoft.EntityFrameworkCore;

using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class ProjectsRepository(WssDbContext context) : IProjectsRepository
{
    public async Task SaveProject(Project projet)
    {
        if (projet.Id is null)
        {
            await context.AddAsync(projet);
        }

        await context.SaveChangesAsync();
    }

    public async Task AttributeProjectToCompany(Project project, int idCompany)
    {
        var newProject = new Project(project.Name, project.GameId, project.Revenu, project.ProjectDuration);

        foreach (var item in project.Skills)
        {
            newProject.Skills.Add(item);
        }

        Console.WriteLine("\n\nCode0124 : " + newProject.CompanyId);
        newProject.CompanyId = idCompany;
        await SaveProject(newProject);
    }

    public async Task<Skill?> GetSkillsById(int? idProject)
    {
        return await context.Skills.FirstOrDefaultAsync(c => c.Id == idProject);
    }

    public async Task<bool> DeleteProjectById(int? idProject)
    {
        // Await the task to get the actual consultant object
        var employee = await context.Projects.FirstOrDefaultAsync(c => c.Id == idProject);

        if (employee == null)
        {
            return false; // or handle the case where the consultant is not found
        }

        context.Projects.Remove(employee);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EndOfProject(int? idCompany)
    {
        var company = await context.Companies.FirstOrDefaultAsync(c => c.Id == idCompany);

        if (company == null)
        {
            return false; // or handle the case where the company is not found
        }

        foreach (var project in context.Projects)
        {
            if(project.ProjectDuration==0)
            {
                // Donner l'argent à la compagnie voulu
                company.Treasury += project.Revenu;
            }
        }

        await context.SaveChangesAsync();

        return true;
    }
}
