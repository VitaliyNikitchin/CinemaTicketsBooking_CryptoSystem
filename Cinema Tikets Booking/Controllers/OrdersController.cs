using Cinema_Security.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using WebApi.Models;
using WebApi.Providers;

namespace Cinema_Security.Controllers
{
    public class OrdersController : Controller
    {
        //[Authorize]
        [HttpGet]
        [Route("Orders/AvailableSeats/{filmSessionId}")]
        public ActionResult AvailableSeats(int filmSessionId)
        {
            using (var db = new ApplicationDbContext())
            {
                return View(db.FilmSessions
                    .Include(o => o.Film)
                    .Include(o => o.Cinema)
                    .First(fs => fs.FilmSessionId == filmSessionId));
            }
        }


        [HttpGet]
        [Route("Orders/OrderPage")]
        public ActionResult OrderPage()
        {
            return View();
        }


        //[Authorize]
        [HttpGet]
        [Route("Orders/ShowReservedSeats/{orderId}")]
        public ActionResult ShowReservedSeats(int orderId)
        {
            using (var db = new ApplicationDbContext())
            {
                UserSeats seats = new UserSeats()
                {
                    userReservedSeats = db.UserOrders.First(o => o.Id == orderId).ReservedPlaces,
                    allHallSeats = db.UserOrders.First(o => o.Id == orderId).FilmSession.SeatIsFree
                };
                return View(seats);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("Orders/EncryptedOrders")]
        [ValidateInput(false)]
        public object ShowEncryptedOrders(string RSAPublicKey)
        {   
            //for encription data using client public key
            var rsaProvider = new RSACryptoServiceProvider(1024);
            rsaProvider.FromXmlString(RSAPublicKey);    //set public rsa key of client to server rsaProvider

            var rsaSignatureProvider = new RSACryptoServiceProvider();
            var rsaSignatureProviderPublicKey = rsaSignatureProvider.ToXmlString(false);  //public key of rsa signature provider

            //create new instance of pbkdf with random iv
            var pbKdf = new Rfc2898DeriveBytes("8080808080808080", 16);
            byte[] aesKey = pbKdf.GetBytes(16);//System.Text.Encoding.UTF8.GetBytes("808080808080808e");
            byte[] aesIv = pbKdf.GetBytes(16);//System.Text.Encoding.UTF8.GetBytes("8080808080808080");

            using (var db = new ApplicationDbContext())
            {
                var userId = User.Identity.GetUserId();
                var orders = db.UserOrders
                    .Where(o => o.UserId == userId)
                    .Select(o => new
                    {
                        OrderId = o.Id,
                        CinemaName = o.FilmSession.Cinema.Name,
                        FilmName = o.FilmSession.Film.Name,
                        HallName = o.FilmSession.HallName,
                        FilmDetailsLink = "/Home/FilmDetails/" + o.FilmSession.FilmId.ToString(),
                        Price = o.FilmSession.Price,
                        DateTime = o.FilmSession.DateTime
                    }).ToList();

                var encryptedAesKey = rsaProvider.Encrypt(aesKey, true);
                var encryptedAesIv = rsaProvider.Encrypt(aesIv, true);
                var signedAesKey = rsaSignatureProvider.SignData(encryptedAesKey, new SHA1CryptoServiceProvider());

                var responseData = new
                {
                    encryptedAesKey = System.Convert.ToBase64String(encryptedAesKey),
                    encryptedAesIv = System.Convert.ToBase64String(encryptedAesIv),
                    orders = AESCryptoProvider.Encrypt(JsonConvert.SerializeObject(orders), aesKey, aesIv),
                    signedData = System.Convert.ToBase64String(signedAesKey),
                    signaturePublicKey = rsaSignatureProviderPublicKey
                };
                return JsonConvert.SerializeObject(responseData);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("Orders/MakeOrder")]
        public string MakeOrder(List<SeatCoordinates> reservedSeats, int filmSessionId)
        {
            using (var db = new ApplicationDbContext())
            {
                FilmSession filmSession = db.FilmSessions.First(fs => fs.FilmSessionId == filmSessionId);
                var filmSessionSeats = JsonConvert.DeserializeObject<JArray>(filmSession.SeatIsFree);

                foreach (var seat in reservedSeats)
                {
                    if ((bool)((JArray)filmSessionSeats[seat.RowNumber])[seat.SeatNumber])
                    {
                        ((JArray)filmSessionSeats[seat.RowNumber])[seat.SeatNumber] = false;
                    }
                }

                filmSession.SeatIsFree = JsonConvert.SerializeObject(filmSessionSeats);

                UserOrders order = new UserOrders()
                {
                    UserId = User.Identity.GetUserId(),
                    FilmSessionId = filmSession.FilmSessionId,
                    ReservedPlaces = JsonConvert.SerializeObject(reservedSeats)
                };
                db.Entry(filmSession).State = EntityState.Modified;
                db.UserOrders.Add(order);
                db.SaveChanges();
            }

            return "{\"responseCode\":200}";
        }
    }
}