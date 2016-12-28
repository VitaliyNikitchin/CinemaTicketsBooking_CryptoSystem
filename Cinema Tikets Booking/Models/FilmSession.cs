using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class FilmSession
    {
        public int FilmSessionId { get; set; }

        //[JsonIgnore]
        public int FilmId { get; set; }

        //[JsonIgnore]
        public int CinemaId { get; set; }

        public string HallName { get; set; }
        public DateTime DateTime { get; set; }
        public string Price { get; set; }
        public string SeatIsFree { get; set; }

        public virtual Film Film { get; set; }
        public virtual Cinema Cinema { get; set; }

        //[JsonIgnore]
        public virtual ICollection<UserOrders> UserReservations { get; set; }
    }
}