using DC.Domain.Entities;

namespace DC.Domain.Interfaces
{
    public interface ITeamRepository : IRepository<Team>
    {
        // First Item of the Return is a Team and the other one checks the given sportId exists or not
        Task<(Team?, bool)> GetByTeamNameAndSportIdAsync(string teamName, int sportId);
    }
}