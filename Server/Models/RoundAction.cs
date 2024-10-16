using System.Text.Json.Serialization;

using Server.Hubs.Records;
using System.Text.Json;

namespace Server.Models;

public enum RoundActionType
{
    RoundAction,
    SendEmployeeForTraining,
    ParticipateInCallForTenders,
    RecruitAConsultant,
    FireAnEmployee,
    PassMyTurn,
    GenerateNewConsultant,
}

public class RoundAction
{
    public int? PlayerId { get; init; }

    public string? ActionType { get; init; }

    public string? Payload { get; init; }

    public RoundActionOverview ToOverview()
    {
        return new RoundActionOverview(ActionType!, JsonSerializer.Serialize(Payload), (int) PlayerId!);
    }
}
