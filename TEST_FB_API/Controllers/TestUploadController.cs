using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace TEST_FB_API.Controllers
{
    public class TestUploadController : Controller
    {
        // GET: TestUpload
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase uploadFile)
        {
            JSONReturnObject rtObj = new JSONReturnObject();
            
            try
            {
                var webDirectoryPath = "/UploadedFiles/";
                var fileName = uploadFile.FileName;
                var path = CheckDirectory(webDirectoryPath, fileName);
                uploadFile.SaveAs(path);

                string webRelativePath = webDirectoryPath + Path.GetFileName(path);

                rtObj.filePath = webRelativePath;
                rtObj.isSuccess = true;
            }
            catch
            {
                rtObj.isSuccess = false;
            }

            return Json(rtObj, "application/json");
        }

        public string CheckDirectory(string webDirectoryPath, string fileName)
        {
            var directoryPath = Server.MapPath(webDirectoryPath);
            var fullPath = Path.Combine(directoryPath, fileName);

            if (Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }
            if (System.IO.File.Exists(fullPath))
            {
                var fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName);
                var fileExtension = Path.GetExtension(fileName);
                var newFileName = fileNameNoExtension + "-" + DateTime.Now.ToString("ddMMyyyy-HHmmss") + fileExtension;
                

                fullPath = Path.Combine(directoryPath, newFileName);
            }

            return fullPath;
        }

        private class JSONReturnObject
        {
            public string filePath { get; set; }

            public bool isSuccess { get; set; }
        }
    }
}