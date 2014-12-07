using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using BitbookFinal.Models;
using Microsoft.AspNet.Identity;

namespace BitbookFinal.Controllers
{
    public class PostController : ApiController
    {
        private string imgFolder = "/Photos/";
        private string defaultAvatar = "user.png";
        private Entities db = new Entities();

        // GET: api/Post
        public dynamic GetPosts()
        {
            var ret = (from post in db.Posts.ToList()
                orderby post.UpdateTime descending
                select new
                {
                    Messege=post.PostText,
                    PostedBy=post.UserId,
                    PostedByName=post.AspNetUser.FirstName,
                    PostedByAvatar=imgFolder+(string.IsNullOrEmpty(post.AspNetUser.ImageName) ? defaultAvatar: post.AspNetUser.ImageName),
                    PostedDate=post.UpdateTime,
                    PostId=post.PostId,
                    PostComments = from comment in post.Comments.ToList() orderby post.UpdateTime select new
                    {
                        CommentedBy = comment.UserId,
                        CommentedByName = comment.AspNetUser.FirstName,
                        CommentedByAvatar = imgFolder + (String.IsNullOrEmpty(comment.AspNetUser.ImageName) ? defaultAvatar : comment.AspNetUser.ImageName),
                        CommentedDate = comment.UpdateTime,
                        CommentId = comment.CommentId,
                        Message = comment.CommentText,
                        PostId = comment.PostId
                    }
                }).AsEnumerable();
            return ret;
            //var ret = (from post in db.Posts.ToList()
            //           orderby post.PostedDate descending
            //           select new
            //           {
            //               Message = post.Message,
            //               PostedBy = post.PostedBy,
            //               PostedByName = post.UserProfile.UserName,
            //               PostedByAvatar = imgFolder + (String.IsNullOrEmpty(post.UserProfile.AvatarExt) ? defaultAvatar : post.PostedBy + "." + post.UserProfile.AvatarExt),
            //               PostedDate = post.PostedDate,
            //               PostId = post.PostId,
            //               PostComments = from comment in post.PostComments.ToList()
            //                              orderby comment.CommentedDate
            //                              select new
            //                              {
            //                                  CommentedBy = comment.CommentedBy,
            //                                  CommentedByName = comment.UserProfile.UserName,
            //                                  CommentedByAvatar = imgFolder + (String.IsNullOrEmpty(comment.UserProfile.AvatarExt) ? defaultAvatar : comment.CommentedBy + "." + comment.UserProfile.AvatarExt),
            //                                  CommentedDate = comment.CommentedDate,
            //                                  CommentId = comment.CommentId,
            //                                  Message = comment.Message,
            //                                  PostId = comment.PostId
            //                              }
            //           }).AsEnumerable();
            //return ret;
        }


        // GET: api/Post/5
        [ResponseType(typeof(Post))]
        public IHttpActionResult GetPost(string id)
        {
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }

        // PUT: api/Post/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPost(string id, Post post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != post.PostId)
            {
                return BadRequest();
            }

            db.Entry(post).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Post
        [ResponseType(typeof(Post))]
        public HttpResponseMessage PostPost(Post post)
        {
            post.UserId = User.Identity.GetUserId().ToString();
            post.UpdateTime = DateTime.UtcNow;
            ModelState.Remove("post.UserId");
            ModelState.Remove("post.UpdateTime");

            if (ModelState.IsValid)
            {
                db.Posts.Add(post);
                db.SaveChanges();
                var usr = db.AspNetUsers.FirstOrDefault(x => x.Id == post.UserId);
                var ret = new
                {
                    Message = post.PostText,
                    PostedBy = post.UserId,
                    PostedByName = usr.FirstName,
                    PostedByAvatar = imgFolder + (string.IsNullOrEmpty(post.AspNetUser.ImageName) ? defaultAvatar : post.AspNetUser.ImageName),
                    PostedDate = post.UpdateTime,
                    PostId = post.PostId
                };
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ret);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = post.PostId }));
                return response;
            }

            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            
            //post.PostedBy = WebSecurity.CurrentUserId;
            //post.PostedDate = DateTime.UtcNow;
            //ModelState.Remove("post.PostedBy");
            //ModelState.Remove("post.PostedDate");

            //if (ModelState.IsValid)
            //{
            //    db.Posts.Add(post);
            //    db.SaveChanges();
            //    var usr = db.UserProfiles.FirstOrDefault(x => x.UserId == post.PostedBy);
            //    var ret = new
            //    {
            //        Message = post.Message,
            //        PostedBy = post.PostedBy,
            //        PostedByName = usr.UserName,
            //        PostedByAvatar = imgFolder + (String.IsNullOrEmpty(usr.AvatarExt) ? defaultAvatar : post.PostedBy + "." + post.UserProfile.AvatarExt),
            //        PostedDate = post.PostedDate,
            //        PostId = post.PostId
            //    };
            //    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ret);
            //    response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = post.PostId }));
            //    return response;
            //}
            //else
            //{
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            //}
        }

        // DELETE: api/Post/5
        [ResponseType(typeof(Post))]
        public IHttpActionResult DeletePost(string id)
        {
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return NotFound();
            }

            db.Posts.Remove(post);
            db.SaveChanges();

            return Ok(post);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PostExists(string id)
        {
            return db.Posts.Count(e => e.PostId == id) > 0;
        }
    }
}