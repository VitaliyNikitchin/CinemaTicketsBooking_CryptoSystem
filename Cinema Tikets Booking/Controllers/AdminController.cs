using Cinema_Security.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WebApi.Models;

namespace Cinema_Security.Controllers
{
    public class AdminController : Controller
    {             
        [HttpGet]
        [Route("api/ParseData/{date}")]
        public string ParseData(string date)    //02-02-2016
        {
            date = date.Replace("-", ".");      //20.20.2016
            string source = "http://kino-teatr.ua/kinoafisha-kiev.phtml?date=" + date;
            this.InsertDataIntoDB(this.GetDataFromSource(source, date));
            return "Saved";
        }

        [NonAction]
        private void InsertDataIntoDB(IEnumerable<FilmSession> list)
        {
            using (var db = new ApplicationDbContext())
            {
                foreach (FilmSession filmSession in list)
                {
                    if (db.Cinemas.Any(c => c.Name == filmSession.Cinema.Name))
                    {
                        filmSession.Cinema = db.Cinemas
                            .SingleOrDefault(c => c.Name == filmSession.Cinema.Name);
                    }
                    if (db.Films.Any(c => c.Name == filmSession.Film.Name))
                    {
                        filmSession.Film = db.Films
                           .SingleOrDefault(f => f.Name == filmSession.Film.Name);
                    }
                    db.FilmSessions.Add(filmSession);
                    db.SaveChanges();
                }
            }
        }

        [NonAction]
        private IEnumerable<FilmSession> GetDataFromSource(string source, string date)
        {
            List<FilmSession> filmSessionList = new List<FilmSession>();
            Dictionary<string, Film> existingFilmList = new Dictionary<string, Film>();

            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var webClient = new WebClient() { Encoding = System.Text.Encoding.UTF8 };
            var document = parser.Parse(webClient.DownloadString(source));

            foreach (var item in document.QuerySelectorAll("#afishaKtName"))
            {
                foreach (var subItem in item.NextElementSibling.QuerySelectorAll("#afishaItem"))
                {
                    var filmSession = new FilmSession();
                    var filmDetailsLink = subItem.QuerySelector(".filmName a").GetAttribute("href").Trim();
                    if (existingFilmList.Keys.Contains(filmDetailsLink))
                        filmSession.Film = existingFilmList[filmDetailsLink];
                    else
                    {
                        Thread filmParserThread = new Thread(() =>
                        { filmSession.Film = ParseFilmDetails(subItem.QuerySelector(".filmName a").GetAttribute("href").Trim()); });
                        filmParserThread.Start();
                        filmParserThread.Join();
                    }

                    filmSession.Cinema = new Cinema() { Name = Regex.Replace(item.QuerySelector("a").TextContent.Trim(), "[\".]", "") };
                    filmSession.HallName = Regex.Replace(subItem.QuerySelector(".filmZal").TextContent.Trim(), "[\".]", "");

                    AngleSharp.Dom.IElement el = subItem.QuerySelector(".filmShows .time a");
                    filmSession.DateTime = DateTime.Parse(
                        (el == null) ? date + " 14:00" : date + " " + el.TextContent.Trim());

                    filmSession.Price = subItem.QuerySelector(".filmPrices").TextContent.Trim();
                    filmSession.SeatIsFree = seatIsFree;
                    filmSessionList.Add(filmSession);
                }
            }
            return filmSessionList;
        }


        private Film ParseFilmDetails(string source)
        {
            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var webClient = new WebClient() { Encoding = System.Text.Encoding.UTF8 };
            var document = parser.Parse(webClient.DownloadString(source));
            var filmInfoElement = document.QuerySelector("#filmInfo");

            Film film = new Film();

            if (filmInfoElement.QuerySelector("strong[itemprop='name']") != null)
                film.Name = Regex.Replace(filmInfoElement.QuerySelector("strong[itemprop='name']").TextContent.Trim(), "[\".]", "");
            else film.Name = "Film 1";

            if (filmInfoElement.QuerySelector("strong:contains('Год')") != null)
                film.Year = Regex.Replace(filmInfoElement.QuerySelector("strong:contains('Год')").NextSibling.TextContent.Trim(), "[\".]", "");
            else film.Year = "2015";

            if (filmInfoElement.QuerySelector("strong:contains('Страна')") != null)
                film.Country = Regex.Replace(filmInfoElement.QuerySelector("strong:contains('Страна')").NextSibling.TextContent.Trim(), "[\".]", "");
            else film.Country = "USA";

            if (filmInfoElement.QuerySelector("[itemprop='director']") != null)
                film.DirectedBy = Regex.Replace(filmInfoElement.QuerySelector("[itemprop='director']").TextContent.Trim(), "[\".]", "");
            else film.DirectedBy = "Дэвид Йетс";

            if (filmInfoElement.QuerySelector("strong:contains('Студия')") != null)
                film.Studio = Regex.Replace(filmInfoElement.QuerySelector("strong:contains('Студия')").NextSibling.TextContent.Trim(), "[\".]", "");
            else film.Studio = "Warner Bros";

            if (filmInfoElement.QuerySelector("[itemprop='genre']") != null)
                film.Genre = Regex.Replace(filmInfoElement.QuerySelector("[itemprop='genre']").TextContent.Trim(), "[\".]", "");
            else film.Genre = "Default genre";

            if (filmInfoElement.QuerySelector("strong:contains('Бюджет')") != null)
                film.Budget = Regex.Replace(filmInfoElement.QuerySelector("strong:contains('Бюджет')").NextSibling.TextContent.Trim(), "[\".]", "");
            else film.Budget = "5 000 000 000 $";

            if (filmInfoElement.QuerySelector("strong:contains('Премьера (в Украине)')") != null)
                film.UkrainePremiere = Regex.Replace(filmInfoElement.QuerySelector("strong:contains('Премьера (в Украине)')").NextSibling.TextContent.Trim(), "[\".]", "");
            else film.UkrainePremiere = "19.12.2010";

            if (filmInfoElement.QuerySelector("strong:contains('Премьера (в мире)')") != null)
                film.WorldPremiere = Regex.Replace(filmInfoElement.QuerySelector("strong:contains('Премьера (в мире)')").NextSibling.TextContent.Trim(), "[\".]", "");
            else film.WorldPremiere = "20.12.2010";

            if (filmInfoElement.QuerySelector("strong:contains('Продолжительность')") != null)
                film.Duration = Regex.Replace(filmInfoElement.QuerySelector("strong:contains('Продолжительность')").NextSibling.TextContent.Trim(), "[\".]", "");
            else film.Duration = "82 мин";

            if (document.QuerySelector("img[itemprop='image']").GetAttribute("src") != null)
                film.ImageLink = document.QuerySelector("img[itemprop='image']").GetAttribute("src");
            else film.ImageLink = "/Images/default.png";

            if (document.QuerySelectorAll("#actorsList li") != null)
            {
                List<string> actorList = new List<string>();
                for (int i = 0; i < document.QuerySelectorAll("#actorsList li").Count() - 1; i++)
                    actorList.Add(document.QuerySelectorAll("#actorsList li")[i].TextContent.Trim());
                film.ActorsList = JsonConvert.SerializeObject(actorList);
            }
            else film.ActorsList = "[]";

            return film;
        }

        private string seatIsFree = "[" +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]," +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]," +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]," +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]," +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]," +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]," +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]," +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]," +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]," +
                        "[true,true,true,true,true,true,true,true,true,true,true,true,true,true,true]" +
                    "]";

    }
}