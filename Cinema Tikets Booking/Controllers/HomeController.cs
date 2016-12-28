using Cinema_Security.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApi.Models;

namespace Cinema_Security.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string date)
        {
            return View(this.GetAllMVC(date));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        [Route("Home/FilmDetails/{filmId}")]
        public ActionResult FilmDetails(int filmId)
        {
            using (var db = new ApplicationDbContext())
            {
                return View(db.Films.First(film => film.FilmId == filmId));
            }
        }

        [HttpGet]
        [Route("api/getAllMVC/{date}")]
        public List<CinemaJsonModel> GetAllMVC(string date)
        {
            List<CinemaJsonModel> list = new List<CinemaJsonModel>();
            CinemaJsonModel cinemaJsonModel = null;
            List<string> uniqueCinemaNames = new List<string>();

            DateTime dd = DateTime.Parse(date);
            using (var db = new ApplicationDbContext())
            {
                foreach (FilmSession filmSessionItem in db.FilmSessions
                    .Where(fs => DbFunctions.TruncateTime(fs.DateTime) == dd)
                    .ToList())
                {
                    if (!uniqueCinemaNames.Contains(filmSessionItem.Cinema.Name))
                    {
                        uniqueCinemaNames.Add(filmSessionItem.Cinema.Name);
                        cinemaJsonModel = new CinemaJsonModel();
                        cinemaJsonModel.CinemaName = filmSessionItem.Cinema.Name;
                        cinemaJsonModel.FilmSession = new List<FilmSessionJsonModel>();
                        list.Add(cinemaJsonModel);
                    }

                    cinemaJsonModel.FilmSession.Add(new FilmSessionJsonModel()
                    {
                        FilmName = filmSessionItem.Film.Name,
                        HallName = filmSessionItem.HallName,
                        Time = filmSessionItem.DateTime.ToString("H:mm"),
                        Price = filmSessionItem.Price,
                        FilmDetailsLink = "/Home/FilmDetails/" + filmSessionItem.FilmId.ToString(),
                        AvailableSeatsLink = "/Orders/AvailableSeats/" + filmSessionItem.FilmSessionId
                    });
                }
                return list;
            }
        }
    }
}