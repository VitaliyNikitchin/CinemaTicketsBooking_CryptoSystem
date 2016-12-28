using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class FilmSessionJsonModel
    {        
        public string FilmName { get; set; }
        public string HallName { set; get; }
        public string Time { set; get; }
        public string Price { set; get; }
        public string FilmImageLink { set; get; }
        public string FilmDetailsLink { set; get; }
        public string AvailableSeatsLink { set; get; }
        //filmSessionId for Vlad
    }

    public class CinemaJsonModel
    {
        public string CinemaName { get; set; }
        public List<FilmSessionJsonModel> FilmSession { set; get; }
    }

    public class SeatCoordinates
    {
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }

        public override int GetHashCode()
        {
            return RowNumber.GetHashCode() + SeatNumber.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            if (obj.GetType() != this.GetType()) return false;

            SeatCoordinates seat = (SeatCoordinates)obj;
            if (RowNumber.Equals(seat.RowNumber) && SeatNumber.Equals(seat.SeatNumber))            
                return true;            
            else return false;
        }
    }

    public class UserSeats
    {
        public string allHallSeats { set; get; }
        public string userReservedSeats { set; get; }        
    }
}