using PawRescue.Application.Interfaces;
using PawRescue.Domain.Entities;
using PawRescue.Domain.Enums;

namespace PawRescue.Application.Services;

/// <summary>
/// CURRENT (rule-based) priority. Volunteers see cases sorted by this score.
///
/// FUTURE FEATURE — Automatic emergency prioritization:
/// an "AiPriorityStrategy : IPriorityStrategy" can replace this class in DI
/// without changing ReportService (Liskov / Open-Closed).
/// </summary>
public class SeverityPriorityStrategy : IPriorityStrategy
{
    public int Calculate(AnimalReport report)
    {
        var score = report.Severity switch
        {
            SeverityLevel.Critical => 70,
            SeverityLevel.High => 50,
            SeverityLevel.Medium => 30,
            _ => 10
        };

        score += report.Condition switch
        {
            AnimalCondition.Injured => 20,
            AnimalCondition.Sick => 15,
            AnimalCondition.Starving => 10,
            AnimalCondition.Abandoned => 5,
            _ => 0
        };

        // Older unattended reports slowly gain priority (max +10).
        var hoursOpen = (DateTime.UtcNow - report.CreatedAt).TotalHours;
        score += (int)Math.Min(10, hoursOpen / 6);

        return Math.Clamp(score, 0, 100);
    }
}
