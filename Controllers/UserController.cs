using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Freelance.Models;
using Freelance.Models.ViewModels;

namespace Freelance.Controllers
{
    public class UserController : Controller
    {
        private SystemDBEntities db = new SystemDBEntities();
        private DataBaseConnection dbc = new DataBaseConnection();
      
        public ActionResult Index(string id)

        {

            if (id == null)
            {
                return RedirectToAction("CurrentUser","User");
            }

            if (User.Identity.Name.ToLower().ToString() == id.ToLower().ToString())
            {

                User user = db.Users.Where(u => u.UserName.ToLower().ToString() == id.ToLower().ToString()).FirstOrDefault();
                if (user == null)
                {
                    return HttpNotFound();
                }
                return RedirectToAction("Edit", new { id });

            }
            else
            {
                return RedirectToAction("Details", "User", new { id });
            }

        }

        public ActionResult CurrentUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                string UsrType = dbc.GetUserType(User.Identity.Name);
                if (UsrType == "Freelancer")
                {
                    return RedirectToAction("Index", "Freelancer");
                }
                else if (UsrType == "Client")
                {
                    return RedirectToAction("CLogIn", "Client", new { id = User.Identity.Name });
                }
                else { return RedirectToAction("IsAdmin", "Admin", new { id = User.Identity.Name }); } //for admin
            }
            else
            {
                return RedirectToAction("Index", "Freelancer");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(HomeViewModel user)
        {
            
            
            if (ModelState.IsValid)
            {
                var data = db.Users.Where(u => u.Email.ToLower() == user.userLogin.Email.ToLower() && u.Password == user.userLogin.Password);
                if (data.Count() == 1)
                {
                    FormsAuthentication.SetAuthCookie(data.SingleOrDefault().UserName.ToString(), true);
                    return RedirectToAction("CurrentUser");
                }
            }
            ModelState.AddModelError("", "invalid Username or Password");
            return RedirectToAction("Index", "Freelancer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "Id,Email,UserName,Password,FirstName,LastName,PhoneNumber,Photo,UserType")] User user, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {

                if (ImageFile == null)
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    FormsAuthentication.SetAuthCookie(user.UserName, true);
                    return RedirectToAction("CurrentUser");
                }
                else
                {
                    string ImageName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                    ImageName = ImageName + "-" + user.UserName + Path.GetExtension(ImageFile.FileName);
                    user.Photo = Path.Combine(@"/Content/Images", ImageName);

                    ImageFile.SaveAs(Path.Combine(Server.MapPath(@"~/Content/Images"), ImageName));
                    db.Users.Add(user);
                    db.SaveChanges();
                    FormsAuthentication.SetAuthCookie(user.UserName, true);
                    return RedirectToAction("CurrentUser");

                }
            }
            return RedirectToAction("Index", "Freelancer");

        }

        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Freelancer");
        }


        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return RedirectToAction("CurrentUser");
            }
            else
            {
                if (User.Identity.Name.ToLower().ToString() == id.ToLower().ToString())
                {

                    return RedirectToAction("Edit", "User", new { id });
                }
                else
                {
                    User user = db.Users.Where(u => u.UserName.ToLower().ToString() == id.ToLower().ToString()).FirstOrDefault();

                    if (user == null)
                    {
                        return HttpNotFound();
                    }
                    var HomeViewModel = new HomeViewModel { user = user };
                    return View(HomeViewModel);
                }


            }
        }


        [Authorize]
        public ActionResult Edit(String id)
        {
            if (id == null)
            {
                return RedirectToAction("CurrentUser", "User");
            }
            if (User.Identity.Name.ToLower().ToString() == id.ToLower().ToString())
            {

                User user = db.Users.Where(u => u.UserName.ToLower().ToString() == id.ToLower().ToString()).FirstOrDefault();
                if (user == null)
                {
                    return HttpNotFound();
                }
                var HomeViewModel = new HomeViewModel { user = user };
                return View(HomeViewModel);


            }
            else
            {
                return RedirectToAction("Details", "User", new { id });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,UserName,Password,FirstName,LastName,PhoneNumber,Photo,UserType")] User user, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile == null)
                {
                    System.Diagnostics.Debug.WriteLine("here");
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                }
                else
                {
                    string ImageName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                    ImageName = ImageName + "-" + user.UserName + Path.GetExtension(ImageFile.FileName);
                    user.Photo = Path.Combine(@"/Content/Images", ImageName);

                    ImageFile.SaveAs(Path.Combine(Server.MapPath(@"~/Content/Images"), ImageName));
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            HomeViewModel model = new HomeViewModel() { user = user };
            return RedirectToAction("CurrentUser");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
