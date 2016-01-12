using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TarikGegic.Models;

namespace TarikGegic.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Picture picture)
        {
            //This is for the Authenticated users to upload picture
            if (User.Identity.IsAuthenticated)
            {
                    foreach (var file in picture.Files)
                    {
                        if (file.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/Image"), fileName);
                            file.SaveAs(path);
                        }
                    }
                }
            return RedirectToAction("Index");
        }
        //Left to do show image on the screen
       
        [HttpGet]
        public ActionResult LogIn()
        {
            return View();
        }

        //Log in system for registered users
        [HttpPost]
        public ActionResult LogIn(Models.UserModel user)
        {
            if (ModelState.IsValid)
            {
                if (IsValid(user.Email, user.Password))
                {
                    FormsAuthentication.SetAuthCookie(user.Email, false);
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    ModelState.AddModelError("","Login Data is Incorrect");
                }
            }
            return View(user);
        }

        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //registration for the users
        [HttpPost]
        public ActionResult Registration(Models.UserModel user)
        {
            if (ModelState.IsValid)
            {
                using (var db = new MainDbContent())
                {
                    var crypto = new SimpleCrypto.PBKDF2();

                    var encrpPass = crypto.Compute(user.Password);

                    var sysUser = db.SystemUsers.Create();

                    sysUser.Email = user.Email;
                    sysUser.Password = encrpPass;
                    sysUser.PasswordSalt = crypto.Salt;
                    sysUser.UserId = Guid.NewGuid();

                    db.SystemUsers.Add(sysUser);
                    db.SaveChanges();
                    SendMail(user);
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError("", "Login Data is incorrect");
            }

            return View(user);
        }

        //Log out 
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index","Home");
        }

        private bool IsValid(string email, string password)
        {
            var crypto = new SimpleCrypto.PBKDF2();
            bool isValid = false;

            using(var db = new MainDbContent())
            {
                var user = db.SystemUsers.FirstOrDefault(u => u.Email == email);

                if(user != null)
                {
                    if(user.Password == crypto.Compute(password, user.PasswordSalt))
                    {
                        isValid = true;
                    }
                }
            }
            return isValid;
        }

        //Send email after register
        protected void SendMail(UserModel user)
        {
            // Gmail Address from where you send the mail
            var fromAddress = "yourEmail@gmail.com";
            // any address where the email will be sending
            var toAddress = user.Email;
            //Password of your gmail address
            const string fromPassword = "yourPassword";
            // Passing the values and make a email formate to display
            string subject = "Register successful!";
            string body = "Dear " + user.Email + ",\n";
            body += " Now when you are registered can you enjoy\n";
            body += "Have your password secret for your safety.\n";

            // smtp settings
            var smtp = new System.Net.Mail.SmtpClient();
            {
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                smtp.Timeout = 20000;
            }
            // Passing values to smtp object
            smtp.Send(fromAddress, toAddress, subject, body);
        }
    }
}
