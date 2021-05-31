
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Test.ViewModels;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace Test.Controllers
{
    public class ContactSurfaceController : SurfaceController
    {
        private readonly ILogger logger;


        public ContactSurfaceController(ILogger logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult RenderForm()
        {
            var model = new ContactViewModel()
            {
                ContactFromId = CurrentPage.Id
            };


            return PartialView("~/Views/Partials/Contact/contactForm.cshtml", model);
        }

        [HttpPost]
        public ActionResult RenderForm(ContactViewModel model)
        {
            return PartialView("~/Views/Partials/Contact/contactForm.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(ContactViewModel model)
        {
            bool success = false;

            if (ModelState.IsValid)
            {
                success = SendEmail(model);
            }

            var contactPage = UmbracoContext.Content.GetById(false, model.ContactFromId);
            var successMessage = contactPage.Value<IHtmlString>("successMessage");
            var errorMessage = contactPage.Value<IHtmlString>("errorMessage");

            TempData["InfoMessageHtml"] = success ? successMessage : errorMessage;
            return CurrentUmbracoPage();
        }

        private bool SendEmail(ContactViewModel model)
        {
            try
            {
                var options = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

                // отправитель - устанавливаем адрес и отображаемое в письме имя
                ////MailAddress from = new MailAddress("baranovskynet@gmail.com", "Admin");
                MailAddress from = new MailAddress(options.From, "Admin");
                // кому отправляем
                MailAddress to = new MailAddress(model.Email, model.Name);
                // создаем объект сообщения
                MailMessage m = new MailMessage(from, to);
                // тема письма
                m.Subject = $"Question - {model.Name}";
                // текст письма
                m.Body = 
                    "<div>" +
                    "<h1>Hello bro!</h1>" +
                    $"<p style=\"color:blue\">{model.Message}</p>" +
                    "</div>";
                // письмо представляет код html
                m.IsBodyHtml = true;
                // адрес smtp-сервера и порт, с которого будем отправлять письмо

                SmtpClient smtp = new SmtpClient(options.Network.Host, options.Network.Port);
                // логин и пароль
                //zodtymiuhgwoulqp
                smtp.Credentials = new NetworkCredential(options.Network.UserName, options.Network.Password);
                smtp.EnableSsl = options.Network.EnableSsl;
                smtp.Send(m);
                return true;

            }
            catch (Exception ex)
            {
                logger.Error(typeof(ContactSurfaceController), ex, "Error sending contact form.");
                return false;
            }
        }
    }
}