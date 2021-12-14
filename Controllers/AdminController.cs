using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Net;
using System.Web.Security;
using Freelance.Models;
using Freelance.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace Freelance.Controllers
{
    public class AdminController : Controller
    {
        private SystemDBEntities db = new SystemDBEntities();
        private DataBaseConnection dbc = new DataBaseConnection();

        public static bool bool_Admin = false;

        public ActionResult IsAdmin(string id)
        {

            DataBaseConnection data = new DataBaseConnection();
            var s = data.GetUserType(id);
            if (s == "Admin") bool_Admin = true;

            return RedirectToAction("Index", "Admin");
        }
        public ActionResult AdminLogOut()
        {

            bool_Admin = false;
            return RedirectToAction("LogOut", "User");
        }

        [Authorize]
        public ActionResult Index()
        {

            if (bool_Admin)
            {
                var jobPosts = db.JobPosts.Where(u => u.Approved == 1 && u.Taken == 0);
                return View(jobPosts.ToList());
            }
            return RedirectToAction("CurrentUser", "User");
        }


        [Authorize]
        public ActionResult AddUser()
        {
            if (bool_Admin)
            {
                var user = new User();

                return View(user);
            }
            return RedirectToAction("CurrentUser", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser([Bind(Include = "Id,Email,UserName,Password,FirstName,LastName,PhoneNumber,Photo,UserType")] User user, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("valid");
                if (ImageFile == null)
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("UserPage", "Admin");
                }
                else
                {
                    string ImageName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                    ImageName = ImageName + "-" + user.UserName + Path.GetExtension(ImageFile.FileName);
                    user.Photo = Path.Combine(@"/Content/Images", ImageName);

                    ImageFile.SaveAs(Path.Combine(Server.MapPath(@"~/Content/Images"), ImageName));
                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("UserPage", "Admin");

                }
            }
            return RedirectToAction("CurrentUser", "User");

        }




        [Authorize]
        public ActionResult UserPage()
        {
            if (bool_Admin)
            {
                return View(db.Users.Where(u=>u.UserType!="Admin").ToList());
            }
            return RedirectToAction("CurrentUser", "User");
        }
        [Authorize]

        public ActionResult PostsRequests()
        {
            if (bool_Admin)
            {
                var jobPostsRequests = db.JobPosts.Where(u => u.Approved == 0 && u.Taken == 0);
                return View(jobPostsRequests.ToList());
            }
            return RedirectToAction("CurrentUser", "User");
        }

        


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(int Id)
        {
            var user = db.Users.Find(Id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("UserPage");
            //return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AcceptJob(int JobId)
        {
            var data = db.JobPosts.Where(u => u.Id == JobId);

            data.UpdateFromQuery(u => new JobPost { Approved = 1 });
            db.SaveChanges();
            return RedirectToAction("PostsRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteJob(int jobID)
        {
            var job = db.JobPosts.Find(jobID);
            if (job == null)
            {
                return RedirectToAction("Index", "Admin");
            }
            db.JobPosts.Remove(job);
            db.SaveChanges();
            return RedirectToAction("Index", "Admin");
        }

        [Authorize]
        public ActionResult EditPost(int JobId)
        {
           
            if (bool_Admin)

            {
                var post = db.JobPosts.Where(u => u.Id == JobId).FirstOrDefault();
                var HomeViewModel = new HomeViewModel { post = post };
                return View(HomeViewModel);//
            }
            return RedirectToAction("CurrentUser", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost([Bind(Include = "Id,ClinetId,JobType,JobTitle,JobBudget,Date,JobDes,PropNum,Rate,Approved,Taken")] JobPost post)
        {
            if (ModelState.IsValid)
            {
                var posts = db.JobPosts.Where(u => u.Id == post.Id).ToList();
                var op = posts[0];
                op.JobType = post.JobType;
                op.JobTitle = post.JobTitle;
                op.JobDes = post.JobDes;
                op.JobBudget = post.JobBudget;
                post = op;
                System.Diagnostics.Debug.WriteLine("here");
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
            }
            HomeViewModel model = new HomeViewModel() { post = post };
            return View(model);
        }



    }
}

