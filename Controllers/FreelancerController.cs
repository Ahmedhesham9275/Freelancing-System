using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
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
    public class FreelancerController : Controller
    {
        private SystemDBEntities db = new SystemDBEntities();
        private DataBaseConnection data = new DataBaseConnection();
        public ActionResult Index()
        {
            var posts = db.JobPosts.Where(u=>u.Approved==1&&u.Taken==0).ToList();
            var user = new User();
            var savedPosts = db.SavedJobs.ToList();
            var HomeViewModel = new HomeViewModel { posts = posts, user = user, savedPosts = savedPosts  };
            return View(HomeViewModel);//
        }
        [Authorize]
        public ActionResult Saved(string id)
        {
            if (id == null)
            {
                return RedirectToAction("CurrentUser", "User");
            }

            if (User.Identity.Name.ToLower().ToString() == id.ToLower().ToString())
            {
                int userId = data.GetUserID(User.Identity.Name);
                var SavedJobsForUser = db.JobPosts.SqlQuery("select * from JobPost JOIN SavedJob on JobPost.Id = SavedJob.JobId and SavedJob.FreelancerId='" + userId + "'");
                if (SavedJobsForUser.Count() == 0)
                {
                    ViewBag.SavedPostsStatus = "There Are No saved Jobs For You";
                }
                HomeViewModel model = new HomeViewModel() { posts = SavedJobsForUser.ToList() };
                return View(model);
            }
            else
            {
                return RedirectToAction("Details", "User", new { id });
            }
        }

        [Authorize]
        public ActionResult CurrentJobs(string id)
        {
            if (id == null)
            {
                return RedirectToAction("CurrentUser", "User");
            }

            if (User.Identity.Name.ToLower().ToString() == id.ToLower().ToString())
            {
                int userId = data.GetUserID(User.Identity.Name);
                var CurrentJobsForUser = db.JobPosts.SqlQuery("select * from JobPost JOIN Proposal on JobPost.Id = Proposal.JobId and Proposal.Approved = '1' where Proposal.FreelancerId='" + userId + "'");
                if (CurrentJobsForUser.Count() == 0)
                {
                    ViewBag.CurrentJobsStatus = "There Are No Jobs For You";
                }
                HomeViewModel model = new HomeViewModel() { posts = CurrentJobsForUser.ToList() };
                return View(model);
            }
            else
            {
                return RedirectToAction("Details", "User", new { id });
            }
        }

        [HttpGet]
        public ActionResult SearchForJob(string SearchByJobName, string SearchByJobDate)
        {
            var SearchResult = from m in db.JobPosts.Where(u => u.Approved == 1 && u.Taken == 0) select m;
            if (SearchByJobName != "")
            {
                SearchResult = SearchResult.Where(u => u.JobTitle.ToLower() == SearchByJobName.ToLower());
            }
            if (SearchByJobDate != "")
            {
                try
                {
                    var myDate = DateTime.ParseExact(SearchByJobDate, "yyyy-MM-dd", null);
                    string TargetDay = myDate.ToString("MM/dd/yyyy");
                    System.Diagnostics.Debug.WriteLine(TargetDay);
                    SearchResult = SearchResult.Where(u => u.JobDate == TargetDay);
                }
                catch (System.FormatException)
                {

                }
            }


            HomeViewModel model = new HomeViewModel { posts = SearchResult.ToList() };

            return View("Index", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyForJob([Bind(Include = "Id,JobId,FreelancerId,Approved")] Proposal proposal)
        {
            if (ModelState.IsValid)
            {
                if (db.Proposals.Where(u => u.JobId == proposal.JobId && u.FreelancerId == proposal.FreelancerId).Count() == 0)
                {
                    db.Proposals.Add(proposal);
                    var data = db.JobPosts.Where(u => u.Id == proposal.JobId);
                    int CurrentProp = data.FirstOrDefault().PropNum + 1;
                    data.UpdateFromQuery(u => new JobPost { PropNum = CurrentProp });
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Freelancer");
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveJob([Bind(Include = "Id,JobId,FreelancerId")] SavedJob savedJob)
        {
            if (ModelState.IsValid)
            {
                if (db.SavedJobs.Where(u => u.JobId == savedJob.JobId && u.FreelancerId == savedJob.FreelancerId).Count() == 0)
                {
                    db.SavedJobs.Add(savedJob);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index","Freelancer");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteJob([Bind(Include = "Id,JobId,UserId")] SavedJob savedJob)
        {
            if (ModelState.IsValid)
            {
                SavedJob removeSavedJob = db.SavedJobs.Where(u => u.JobId == savedJob.JobId && u.FreelancerId == savedJob.FreelancerId).FirstOrDefault();
                db.SavedJobs.Remove(removeSavedJob);
                db.SaveChanges();
                return RedirectToAction("Saved", "Freelancer", new { User.Identity.Name });
            }
            return RedirectToAction("Index");
        }
    }
}