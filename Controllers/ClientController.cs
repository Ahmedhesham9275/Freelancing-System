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

namespace Client.Controllers
{
    public class ClientController : Controller
    {

        private SystemDBEntities db = new SystemDBEntities();
        private DataBaseConnection dbc = new DataBaseConnection();
        public static int cid = 0;
        public static string ctype = "";
        // GET: Client
        public ActionResult CLogIn(string id)
        {

            DataBaseConnection data = new DataBaseConnection();
            cid = data.GetUserID(id);
            ctype = data.GetUserType(id);

            return RedirectToAction("Index", "Client");
        }
        public ActionResult CLogOut()
        {
            cid = 0;
            ctype = "";
            return RedirectToAction("LogOut", "User");
        }

        [Authorize]
        public ActionResult Index()
        {
            if (cid != 0 && ctype == "Client")
            {
                var data = db.JobPosts.Where(u => u.ClientId == cid);
                if (data.Count() == 0)
                {
                    ViewBag.ClientPostsMsg = "There Are No Posts";
                }

                var posts = data.ToList();
                var HomeViewModel = new HomeViewModel { posts = posts };
                return View(HomeViewModel);

            }

            return RedirectToAction("CurrentUser", "User");
        }
        [Authorize]
        public ActionResult Create()
        {
            if (cid != 0 && ctype == "Client")

            {
                var HomeViewModel = new HomeViewModel { post = new JobPost() };
                return View(HomeViewModel);//

            }
            return RedirectToAction("CurrentUser", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ClinetId,JobType,JobTitle,JobBudget,Date,JobDes,PropNum,Rate,Approved,Taken")] JobPost post)
        {

            post.ClientId = cid;
            post.JobDate = DateTime.Now.ToString("MM/dd/yyyy");
            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("valid");
                db.JobPosts.Add(post);
                db.SaveChanges();
            }
            return RedirectToAction("Index");//
        }

        [Authorize]
        public ActionResult Edit(int id)
        {

            if (cid != 0 && ctype == "Client")

            {

                var post = db.JobPosts.Where(u => u.Id == id).FirstOrDefault();
                var HomeViewModel = new HomeViewModel { post = post };
                return View(HomeViewModel);//
            }
            return RedirectToAction("CurrentUser", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ClinetId,JobType,JobTitle,JobBudget,Date,JobDes,PropNum,Rate,Approved,Taken")] JobPost post)
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int JobId)
        {
            var posts = db.JobPosts.Where(u => u.Id == JobId).FirstOrDefault();
            db.JobPosts.Remove(posts);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Proposals(string id)
        {
            if (cid != 0 && ctype == "Client")
            {
                if (id == null)
                {
                    return RedirectToAction("CurrentUser", "User");
                }

                if (User.Identity.Name.ToLower().ToString() == id.ToLower().ToString())
                {

                    int userId = dbc.GetUserID(User.Identity.Name);

                    var PorposalsForJob = (from pl in db.Proposals.Where(u => u.Approved == 0)
                                           join ps in db.JobPosts
                                           on pl.JobId equals ps.Id
                                           join u in db.Users
                                           on pl.FreelancerId equals u.Id
                                           select new ViewModel { ps = ps, ur = u }).ToList();
                    if (PorposalsForJob.Count() == 0)
                    {
                        ViewBag.PorposalsForJobStatus = "No One Applied For Job";
                    }
                    HomeViewModel model = new HomeViewModel { testModel = PorposalsForJob };
                    return View(model);
                }
            }
            return RedirectToAction("CurrentUser", "User");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AcceptJob([Bind(Include = "Id,JobId,FreelancerId,Approved")] Proposal proposal)
        {
            if (ModelState.IsValid)
            {
                
                var UpdateJobPost = db.JobPosts.Where(u => u.Id == proposal.JobId).UpdateFromQuery(u => new JobPost { Taken = 1 });
                var UpdateProposal = db.Proposals.Where(u => u.Id == proposal.JobId).UpdateFromQuery(u => new Proposal { Approved = 1 });
                //var removeProposal = db.Proposals.SqlQuery(" DELETE  FROM [Proposal] WHERE [Proposal].JobId = '" + proposal.JobId +"' and Approved = 0");
                

                db.SaveChanges();
                db.Proposals.RemoveRange(db.Proposals.Where(u => u.JobId == proposal.JobId && u.Approved == 0));
                db.SaveChanges();
                return RedirectToAction("Proposals", "Client");
            }
            return RedirectToAction("Proposals", "Client");
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RefuseJob([Bind(Include = "Id,JobId,FreelancerId,Approved")] Proposal proposal)
        {
            if (ModelState.IsValid)
            {
                if (db.Proposals.Where(u => u.JobId == proposal.JobId && u.FreelancerId == proposal.FreelancerId).Count() == 1)
                {
                    //  Proposal removeProposal = db.Proposals.SqlQuery(" DELETE  FROM[Proposal] WHERE JobId = proposal.JobId and FreelancerId = proposal.FreelancerId ").FirstOrDefault();
                    Proposal removeProposal = db.Proposals.Where(u => u.JobId == proposal.JobId && u.FreelancerId == proposal.FreelancerId).FirstOrDefault();
                    db.Proposals.Remove(removeProposal);
                    db.SaveChanges();
                    return RedirectToAction("Proposals", "Client", new { User.Identity.Name });
                }
            }
            return RedirectToAction("Proposals");
        }
    }
}