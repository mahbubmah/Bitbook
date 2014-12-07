using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BitbookFinal.Models;
using Microsoft.AspNet.Identity;

namespace BitbookFinal.Controllers
{
     [Authorize]
    [RequireHttps]
    public class HomeController : Controller
    {
        private Entities db=new Entities();

         [HttpPost]
        public RedirectToRouteResult FriendRequest(string submitButton)
         {
            
                 FriendRelation aFriendRelation = new FriendRelation();
                 aFriendRelation.UserId1 = User.Identity.GetUserId();
                 aFriendRelation.UserId2 = submitButton;
                 aFriendRelation.AreFriend = false;
                 
            try
             {
                db.FriendRelations.Add(aFriendRelation);
                db.SaveChanges();
                return RedirectToAction("Index");
             }
             catch (Exception exception)
             {
                 return RedirectToAction("Index");
             }
         }

         [HttpGet]
         public PartialViewResult ConfirmFriendRequest()
         {
             var tuple = new Tuple<List<AspNetUser>, List<FriendRelation>>(new List<AspNetUser>(db.AspNetUsers.ToList()), new List<FriendRelation>(db.FriendRelations.ToList()));
             return PartialView("_ConfirmFriendPartial",tuple);
         }

         [HttpPost]
         public ActionResult ConfirmFriendRequest(string submitButton)
         {
             string id = User.Identity.GetUserId().ToString();
             var frendReq =db.FriendRelations.SingleOrDefault(x => (x.UserId1 == id || x.UserId2 == id)&&(x.UserId1==submitButton ||x.UserId2==submitButton));
             frendReq.AreFriend = true;
             db.SaveChanges();
             var tuple = new Tuple<List<AspNetUser>, List<FriendRelation>>(new List<AspNetUser>(db.AspNetUsers.ToList()), new List<FriendRelation>(db.FriendRelations.ToList()));
             return RedirectToAction("Index");
         }


         public ActionResult Index()
        {
             HomeViewModel aHomeViewModel=new HomeViewModel();
             var userid = User.Identity.GetUserId().ToString();
             aHomeViewModel.AspNetUsers = db.AspNetUsers.ToList();
             aHomeViewModel.Comments = db.Comments.ToList();
             aHomeViewModel.Likes = db.Likes.ToList();
             aHomeViewModel.Posts = db.Posts.ToList();
             aHomeViewModel.FriendRelations = db.FriendRelations.ToList();
             return View(aHomeViewModel);
        }
       
        public ActionResult About()
        {
            var userid = User.Identity.GetUserId().ToString();
            var user = db.AspNetUsers.Single(x => x.Id == userid);

            return View(user);
        }


        public ActionResult AboutProfile(string id)
        {
            
            var user = db.AspNetUsers.Single(x => x.Id == id);
            return View(user);
        }

         [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public PartialViewResult _Search()
        {
            return PartialView();
        }

         public ActionResult SearchedUser()
         {
             var tuple = new Tuple<List<AspNetUser>, List<FriendRelation>>(new List<AspNetUser>(db.AspNetUsers.ToList()), new List<FriendRelation>(db.FriendRelations.ToList()));
             return View(tuple);
         }

        [HttpPost]
       public ActionResult SearchedUser(string search)
        {
           
           if (string.IsNullOrEmpty(search))
           {
               var tuple = new Tuple<List<AspNetUser>, List<FriendRelation>>(new List<AspNetUser>(db.AspNetUsers.ToList()), new List<FriendRelation>(db.FriendRelations.ToList()));
               return View(tuple);
           }

           List<AspNetUser> users = db.AspNetUsers.Where(x => x.FirstName.StartsWith(search)).ToList();
           var tuple1 = new Tuple<List<AspNetUser>, List<FriendRelation>>(new List<AspNetUser>(users), new List<FriendRelation>(db.FriendRelations.ToList()));

           return View(tuple1);
        }
    }
}