
using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateEmployeeParams(string EmployeeName, int? CompanyId = null, Company? Company = null);

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeParams>
{
    public CreateEmployeeValidator()
    {
        RuleFor(p => p.EmployeeName).NotEmpty();
        RuleFor(p => p.CompanyId).NotEmpty().When(p => p.Company is null);
        RuleFor(p => p.Company).NotEmpty().When(p => p.CompanyId is null);
    }
}

public class CreateEmployee(
    ICompaniesRepository companiesRepository,
    IEmployeesRepository employeesRepository,
    ISkillsRepository skillsRepository,
    IGameHubService gameHubService
) : IAction<CreateEmployeeParams, Result<Employee>>
{
    public async Task<Result<Employee>> PerformAsync(CreateEmployeeParams actionParams)
    {
        var rnd = new Random();

        var actionValidator = new CreateEmployeeValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (employeeName, companyId, company) = actionParams;

        company ??= await companiesRepository.GetById(companyId!.Value);

        if (company is null)
        {
            Result.Fail($"Company with Id \"{companyId}\" not found.");
        }

        IEnumerable<int> salaries = [];

        salaries = salaries.Append(1500); // Salaire de base (hors skills)

        var randomSalary = salaries.ToList()[rnd.Next(salaries.Count() - 1)];

        var employee = new Employee(employeeName, company!.Id!.Value, company!.Player.GameId, randomSalary);

        var randomSkills = await skillsRepository.GetRandomSkills(5);

        var totalSkillsLevel = 0;

        foreach (var randomSkill in randomSkills)
        {
            var leveledSkill = rnd.Next(6);
            employee.Skills.Add(new LeveledSkill(randomSkill.Name, leveledSkill));
            totalSkillsLevel += leveledSkill;
        }

        employee.Salary += totalSkillsLevel * 100; // Nouveau salaire, en fonction des niveaux des skills

        await employeesRepository.SaveEmployee(employee);

        await gameHubService.UpdateCurrentGame(gameId: company.Player.GameId);

        return Result.Ok(employee);
    }
}
