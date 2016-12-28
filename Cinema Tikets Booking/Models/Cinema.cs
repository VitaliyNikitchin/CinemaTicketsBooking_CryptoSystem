using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class Cinema
    {
        //[JsonIgnore]
        public int CinemaId { get; set; }
        public string Name { get; set; }

        //[JsonIgnore]
        public virtual ICollection<FilmSession> FilmSessions { get; set; }
    }
}