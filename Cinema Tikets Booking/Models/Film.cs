using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class Film
    {
        [JsonIgnore]
        public int FilmId { get; set; }

        public string Name { get; set; }
        public string ImageLink { get; set; }
        public string Genre { get; set; }
        public string Country { get; set; }
        public string Year { get; set; }
        public string DirectedBy { get; set; }
        public string Studio { get; set; }
        public string Budget { get; set; }
        public string UkrainePremiere { get; set; }
        public string WorldPremiere { get; set; }
        public string Duration { get; set; }
        public string ActorsList { get; set; }

        [JsonIgnore]
        public virtual ICollection<FilmSession> FilmSessions { get; set; }

        /*
        public override int GetHashCode()
        {
            return Name.GetHashCode() + ImageLink.GetHashCode() + Genre.GetHashCode() + Country.GetHashCode() +
                Year.GetHashCode() + DirectedBy.GetHashCode() + Studio.GetHashCode() + Budget.GetHashCode() +
                UkrainePremiere.GetHashCode() + WorldPremiere.GetHashCode() + Duration.GetHashCode() + ActorsList.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            if (obj.GetType() != this.GetType()) return false;

            Film film = (Film) obj;
            if (Name.Equals(film.Name) && ImageLink.Equals(film.ImageLink) &&
                Genre.Equals(film.Genre) && Country.Equals(film.Country) &&
                Year.Equals(film.Year) && DirectedBy.Equals(film.DirectedBy) &&
                Studio.Equals(film.Studio) && Budget.Equals(film.Budget) &&
                UkrainePremiere.Equals(film.UkrainePremiere) && WorldPremiere.Equals(film.WorldPremiere) &&
                Duration.Equals(film.Duration) && ActorsList.Equals(film.ActorsList))
            {
                return true;
            }
            else return false;
        }*/
    }
}