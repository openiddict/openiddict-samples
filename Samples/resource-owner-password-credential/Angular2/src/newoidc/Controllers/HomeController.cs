
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using ImageProcessorCore;
using ImageProcessorCore.Samplers;
using newoidc.Models;
using newoidc.Data;


namespace newoidc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApplicationEnvironment _appEnvironment;
        private ApplicationDbContext _context;
        public HomeController(IApplicationEnvironment appEnvironment, ApplicationDbContext context)
        {
            _appEnvironment = appEnvironment;
            _context = context;
        }

        public ActionResult Index()
        {
            return View("index");
        }

       
        public IActionResult Error()
        {
            return View();
        }
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public ActionResult SaveUploadedFile()
        {

            string fName = "";
            string fname2 = "";
            string c = "";
            try
            {
                foreach (var file in Request.Form.Files)
                {
                    c = Request.Headers["X-Hello"].ToString();
                    // c = Request.Form.Keys.ToString();
                    var parsedContentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                    fName = parsedContentDisposition.FileName.Trim('"');
                    if (file != null && file.ContentDisposition.Length > 0)
                    {

                        var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\WallImages", _appEnvironment.ApplicationBasePath));

                        string pathString = System.IO.Path.Combine(originalDirectory.ToString(), "imagepath");

                        var fileName1 = Path.GetFileName(ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"'));

                        bool isExists = System.IO.Directory.Exists(pathString);

                        if (!isExists)
                            System.IO.Directory.CreateDirectory(pathString);

                        int i = 0;
                        string filemask = "scart{0}" + ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        fname2 = string.Format(filemask, i);
                        do
                        {
                            i = i + 1;
                            fname2 = String.Format(filemask, i);

                        } while (System.IO.File.Exists(pathString + "\\" + fname2));

                        string path = string.Format("{0}\\{1}", pathString, fname2);
                        string thumbpath = string.Format("{0}\\{1}", pathString, "thumb-" + fname2);
                        Size size = new Size(150, 0);
                        var fileStream = file.OpenReadStream();
                        Image image = new Image(fileStream);
                        Image image2 = new Image(fileStream);
                        FileStream file2 = new FileStream(path, FileMode.Create, System.IO.FileAccess.Write);
                        int Sizer = 500;
                        float width = image.Width;
                        float height = image.Height;
                        int x = 0;
                        int y = 0;
                        if (width > height)
                        {
                            width = (width / height) * Sizer;
                            height = Sizer;
                            x = Convert.ToInt32(Math.Ceiling((double)((width - height) / 2)));
                        }
                        else if (height > width)
                        {
                            height = (height / width) * Sizer;
                            width = Sizer;
                            y = Convert.ToInt32(Math.Ceiling((double)((height - width) / 2)));
                        }
                        else
                        {
                            width = Sizer;
                            height = Sizer;
                        }
                        int maxWidth = Sizer;
                        int maxHeight = Sizer;
                        int newWidth = image.Width;
                        int newHeight = image.Height;
                        double aspectRatio = (double)image.Width / (double)image.Height;
                        if (aspectRatio <= 1 && image.Width > maxWidth)
                        {
                            newWidth = maxWidth;
                            newHeight = (int)Math.Round(newWidth / aspectRatio);
                        }
                        else if (aspectRatio > 1 && image.Height > maxHeight)
                        {
                            newHeight = maxHeight;
                            newWidth = (int)Math.Round(newHeight * aspectRatio);
                        }
                        Point pt = new Point(x, y);
                        Size sz = new Size(newWidth, newHeight);
                        image.Resize(newWidth, newHeight).Save(file2);
                        FileStream file3 = new FileStream(thumbpath, FileMode.Create, System.IO.FileAccess.Write);
                        image2.Resize(0, 250 ,new NearestNeighborResampler(),new Rectangle(pt, sz)).Crop(400, 250).Save(file3);
                        image.Dispose();
                        fileStream.Dispose();
                        image2.Dispose();
                        file3.Dispose();
                        file2.Dispose();
                        var newpicture = new ProductPicture();
                        newpicture.pictureurl = fname2.ToString();
                        newpicture.ProductId = Convert.ToInt32(Request.Headers["X-Hello"]);
                        _context.ProductPicture.Add(newpicture);
                        _context.SaveChanges();

                        c = Request.Headers["X-Hello"].ToString() + "header," + x + "," + y;
                    }
                }

                return Json(new { Message = c + "," + fname2 });
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, status = "error", code = "403" });
            }

        }
    }

   
}
