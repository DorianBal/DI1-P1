using Server.Hubs.Records;

namespace Server.Models;

public class Project(string name, int gameId, int revenu, int projectDuration)
{
    public int? Id { get; private set; }

    public string Name { get; set; } = name;

    public int Revenu { get; set; } = revenu;

    public ICollection<LeveledSkill> Skills { get; } = [];

    public int GameId { get; set; } = gameId;

    public Game Game { get; set; } = null!;

    public int? CompanyId { get; set; } = null!;

    public Company Company { get; set; } = null!;

    public int ProjectDuration { get; set; } = projectDuration;

    public ProjectOverview ToOverview()
    {
        return new ProjectOverview(
            Id is null ? 0 : 
            (int) Id, 
            Name, 
            Revenu, 
            Skills.Select(s => s.ToOverview()).ToList(),
            ProjectDuration
        );
    }
}
