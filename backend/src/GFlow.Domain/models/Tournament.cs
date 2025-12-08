using System.Reflection.Metadata.Ecma335;

namespace GFlow.Domain.Models
{
    public class Tournament
    {
        public int Id {get; set;}
        public string Title {get; set;}
        public TournamentMetaData MetaData {get; set;}

        public Tournament(int id, string title, string startDate, string endDate)
        {
            this.Id = id;
            this.Title = title;
            this.MetaData = new TournamentMetaData(startDate, endDate);
        }

    }

    public class TournamentMetaData
    {
        public string StartDate {get; set;}
        public string EndDate {get; set;}
        public int? NumberOfRounds {get; set;}
        public int? NumberOfRegisteredPlayers {get; set;}

        public TournamentMetaData(string startDate, string endDate)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            NumberOfRounds= null;
            NumberOfRegisteredPlayers = null;
        }
    }
}