using Server.Hubs.Records;

namespace Server.Models;

public class Employee(string name, int companyId, int gameId, int salary)
{
    public int? Id { get; private set; }

    public string Name { get; set; } = name;

    public int Salary { get; set; } = salary;

    public ICollection<LeveledSkill> Skills { get; } = [];

    public int GameId { get; set; } = gameId;

    public Game Game { get; set; } = null!;

    public int CompanyId { get; set; } = companyId;

    public Company Company { get; set; } = null!;

    public bool enformation { get; set; } = false;

    public int dureeformation { get; set; } = -1;

    public bool enprojet { get; set; } = false;

    public EmployeeOverview ToOverview()
    {
        return new EmployeeOverview(
            Id is null ? 0 : (int) Id, Name, Salary,
            Skills.Select(s => s.ToOverview()).ToList(), enformation, dureeformation, enprojet
        );
    }
}
