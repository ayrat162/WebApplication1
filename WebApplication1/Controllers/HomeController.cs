using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            string[] filePaths = Directory.GetFiles(Server.MapPath("~/App_Data/")).Select(p => Path.GetFileName(p)).ToArray();

            return View(new FileModel { FileNames = filePaths });
        }


        [Authorize]
        public ActionResult DownloadFile(string fileName)
        {
            //checking for secure filename should be added
            var path = Server.MapPath("~/App_Data/") + fileName;
            try
            {
                var bytes = System.IO.File.ReadAllBytes(path);
                return File(bytes, "application/octet-stream", fileName);
            }
            catch (Exception e)
            {
                return View(model: e.Message);
            }
        }
    }
}