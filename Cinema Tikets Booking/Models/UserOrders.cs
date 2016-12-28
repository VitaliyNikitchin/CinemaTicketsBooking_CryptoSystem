using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Cinema_Security.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models
{
    public class UserOrders
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int FilmSessionId { get; set; }
        public string ReservedPlaces { get; set; }


        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        [JsonIgnore]
        public virtual FilmSession FilmSession { get; set; }
    }
}