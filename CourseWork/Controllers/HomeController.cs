using CourseWork.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CourseWork.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public VenatorContext db;

        public HomeController(VenatorContext context)
        {
            db = context;
            if (db.AllRoles.FirstOrDefault(r => r.Id == 1) == null)
            {
                db.AllRoles.Add(new AllRoles());
                db.SaveChanges();
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(User user)
        {
            var foundLogin = db.Users.FirstOrDefault(u => u.Username == user.Username);
            if (foundLogin != null)
            {
                ViewBag.ErrorUser = "Логин " + user.Username + " уже используется!";
                return View();
            }
            var foundEmail = db.Users.FirstOrDefault(u => u.Email == user.Email);
            if (foundEmail != null)
            {
                ViewBag.ErrorUser = "Email " + user.Email + " уже используется!";
                return View();
            }

            if (user.Username == "admin")
            {
                AddRoleToUser(user, "admin");
            }
            AddRoleToUser(user, "user");
            user.DateOfRegistration = DateTime.Now;
            db.Users.Add(user);
            db.SaveChanges();

            await Authenticate(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login login)
        {
            if (ModelState.IsValid)
            {
                User user = db.Users.FirstOrDefault(u => u.Username == login.Username && u.Password == login.Password);
                if (user != null)
                {
                    await Authenticate(user);
                    return RedirectToAction("Index", "Home");
                }
                ViewBag.ErrorLogin = login;
                return View();
            }
            else
            {
                ModelState.AddModelError("", "Некоректные данные!");
                return View(login);
            }
        }

        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username)
            };
            foreach (var role in GetAllUserRoles(user))
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
            }
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "admin")]
        public IActionResult AllUsers(int change = 1, string msg = "")
        {
            var query = from b in db.Users
                        orderby b.Id
                        select b;
            ViewBag.CreateDelete = change;
            ViewBag.Message = msg;

            ViewBag.AllUsers = query;
            ViewBag.AllRoles = GetAllPossibleRoles();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteUser(int id)
        {
            User user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("AllUsers", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult ChangeRoles(int id, List<string> roles)
        {
            User user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                SetAllUserRoles(user, roles);
                db.SaveChanges();
            }
            return RedirectToAction("AllUsers", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult AddRole()
        {
            return RedirectToAction("AllUsers", "Home", new { change = 2 });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult AddRole(string role)
        {
            string msg = AddPossibleRole(role);
            int success = 2;
            if (msg == "")
            {
                success = 1;
            }
            return RedirectToAction("AllUsers", "Home", new { change = success, msg = msg });
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult RemoveRole()
        {
            return RedirectToAction("AllUsers", "Home", new { change = 3 });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult RemoveRole(string role)
        {
            string msg = RemovePossibleRole(role);
            int success = 3;
            if (msg == "")
            {
                success = 1;
            }
            return RedirectToAction("AllUsers", "Home", new { change = success, msg = msg });
        }

        [HttpGet]
        public IActionResult Comments()
        {
            var query = from b in db.Comments
                        orderby b.Id
                        select b;
            ViewBag.AllComments = query;
            foreach (var item in query)
            {
                db.Entry(item).Reference(p => p.User).Load();
            }
            return View();
        }

        [HttpPost]
        public IActionResult Comments(Comment comment)
        {
            var query = from b in db.Users
                        where b.Username == User.Identity.Name
                        select b;
            comment.IsHidden = false;
            comment.User = query.First();
            comment.DateOfComment = DateTime.Now;
            db.Comments.Add(comment);
            db.SaveChanges();

            var queryComments = from b in db.Comments
                                orderby b.Id
                                select b;
            ViewBag.AllComments = queryComments;
            foreach (var item in queryComments)
            {
                db.Entry(item).Reference(p => p.User).Load();
            }
            ModelState.Clear();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "moderator")]
        public IActionResult HideComment(int id)
        {
            Comment comment = db.Comments.Find(id);
            if (comment != null)
            {
                comment.IsHidden = true;
                db.SaveChanges();
            }
            return RedirectToAction("Comments", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "moderator")]
        public IActionResult ShowComment(int id)
        {
            Comment comment = db.Comments.Find(id);
            if (comment != null)
            {
                comment.IsHidden = false;
                db.SaveChanges();
            }
            return RedirectToAction("Comments", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "moderator")]
        public IActionResult HideNews(int id)
        {
            News news = db.News.Find(id);
            if (news != null)
            {
                news.IsHidden = true;
                db.SaveChanges();
            }
            return RedirectToAction("News", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "moderator")]
        public IActionResult ShowNews(int id)
        {
            News news = db.News.Find(id);
            if (news != null)
            {
                news.IsHidden = false;
                db.SaveChanges();
            }
            return RedirectToAction("News", "Home");
        }

        [HttpGet]
        public IActionResult News()
        {
            var query = from b in db.News
                        orderby b.Id
                        select b;
            ViewBag.AllNews = query;
            foreach (var item in query)
            {
                db.Entry(item).Reference(p => p.User).Load();
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "moderator")]
        public IActionResult News(News news)
        {
            var query = from b in db.Users
                        where b.Username == User.Identity.Name
                        select b;
            news.IsHidden = false;
            news.User = query.First();
            news.DateOfNews = DateTime.Now;
            db.News.Add(news);
            db.SaveChanges();

            var queryNews = from b in db.News
                            orderby b.Id
                            select b;
            ViewBag.AllNews = queryNews;
            foreach (var item in queryNews)
            {
                db.Entry(item).Reference(p => p.User).Load();
            }
            ModelState.Clear();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public bool Authentication(string username, string password)
        {
            User user = db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            return user != null;
        }

        #region Роли
        public List<string> GetAllPossibleRoles()
        {
            return DeserializeRoles(db.AllRoles.FirstOrDefault(u => u.Id == 1).Roles);
        }

        public string AddPossibleRole(string role)
        {
            List<string> rolesList = GetAllPossibleRoles();
            if (!rolesList.Contains(role))
            {
                rolesList.Add(role);
                AllRoles roles = db.AllRoles.FirstOrDefault(u => u.Id == 1);
                roles.Roles = SerializeRoles(rolesList);
                db.SaveChanges();
                return "";
            }
            return "Роль уже существует!";
        }

        public string RemovePossibleRole(string role)
        {
            List<string> rolesList = GetAllPossibleRoles();
            if (rolesList.Contains(role))
            {
                rolesList.Remove(role);
                AllRoles roles = db.AllRoles.FirstOrDefault(u => u.Id == 1);
                roles.Roles = SerializeRoles(rolesList);
                db.SaveChanges();
                return "";
            }
            return "Роли не существует!";
        }

        public void AddRoleToUser(User user, string role)
        {
            if (GetAllPossibleRoles().Contains(role))
            {
                List<string> userRoles = GetAllUserRoles(user);
                userRoles.Add(role);
                user.Roles = SerializeRoles(userRoles);
            }
        }

        public void RemoveRoleFromUser(User user, string role)
        {
            List<string> userRoles = GetAllUserRoles(user);
            if (userRoles.Contains(role))
            {
                userRoles.Remove(role);
                user.Roles = SerializeRoles(userRoles);
            }
        }

        public List<string> GetAllUserRoles(User user)
        {
            return DeserializeRoles(user.Roles);
        }

        public void SetAllUserRoles(User user, List<string> roles)
        {
            user.Roles = SerializeRoles(roles);
        }

        private string SerializeRoles(List<string> roles)
        {
            return JsonConvert.SerializeObject(roles);
        }

        private List<string> DeserializeRoles(string roles)
        {
            return JsonConvert.DeserializeObject<List<string>>(roles);
        }
    }
    #endregion
}