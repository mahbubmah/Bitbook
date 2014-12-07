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
    public class CommentController : ApiController
    {
        private string imgFolder = "/Photos/";
        private string defaultAvatar = "user.png";
        private Entities db = new Entities();

        // GET: api/Comment
        public IQueryable<Comment> GetComments()
        {
            return db.Comments;
        }

        // GET: api/Comment/5
        [ResponseType(typeof(Comment))]
        public IHttpActionResult GetComment(string id)
        {
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        // PUT: api/Comment/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutComment(string id, Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != comment.CommentId)
            {
                return BadRequest();
            }

            db.Entry(comment).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        // POST: api/Comment
        
        public HttpResponseMessage PostComment(Comment comment)
        {
            comment.UserId = User.Identity.GetUserId().ToString();
            comment.UpdateTime = DateTime.UtcNow;
            ModelState.Remove("comment.UserId");
            ModelState.Remove("comment.UpdateTime");

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                var usr = db.AspNetUsers.FirstOrDefault(x => x.Id == comment.UserId);
                var ret = new
                {
                    CommentedBy = comment.UserId,
                    CommentedByName = usr.UserName,
                    CommentedByAvatar = imgFolder + (String.IsNullOrEmpty(comment.AspNetUser.ImageName) ? defaultAvatar : comment.AspNetUser.ImageName),
                    CommentedDate = comment.UpdateTime,
                    CommentId = comment.CommentId,
                    Message = comment.CommentText,
                    PostId = comment.PostId
                };

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ret);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = comment.CommentId }));
                return response;
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            //postcomment.CommentedBy = WebSecurity.CurrentUserId;
            //postcomment.CommentedDate = DateTime.UtcNow;
            //ModelState.Remove("postcomment.CommentedBy");
            //ModelState.Remove("postcomment.CommentedDate");
            //if (ModelState.IsValid)
            //{
            //    db.PostComments.Add(postcomment);
            //    db.SaveChanges();
            //    var usr = db.UserProfiles.FirstOrDefault(x => x.UserId == postcomment.CommentedBy);
            //    var ret = new
            //    {
            //        CommentedBy = postcomment.CommentedBy,
            //        CommentedByName = usr.UserName,
            //        CommentedByAvatar = imgFolder + (String.IsNullOrEmpty(usr.AvatarExt) ? defaultAvatar : postcomment.CommentedBy + "." + postcomment.UserProfile.AvatarExt),
            //        CommentedDate = postcomment.CommentedDate,
            //        CommentId = postcomment.CommentId,
            //        Message = postcomment.Message,
            //        PostId = postcomment.PostId
            //    };

            //    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ret);
            //    response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = postcomment.CommentId }));
            //    return response;
            //}
            //else
            //{
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            //}
        }

        // DELETE: api/Comment/5
        [ResponseType(typeof(Comment))]
        public IHttpActionResult DeleteComment(string id)
        {
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return NotFound();
            }

            db.Comments.Remove(comment);
            db.SaveChanges();

            return Ok(comment);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CommentExists(string id)
        {
            return db.Comments.Count(e => e.CommentId == id) > 0;
        }
    }
}