using FormziApi.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace FormziApi.Helper
{
    public class ImageUpload
    {
        // set default size here
        public int Width { get; set; }

        public int Height { get; set; }

        // folder for the upload, you can put this in the web.config
        //private readonly string UploadPath = "~/Images/Items/";

        public ImageResult RenameUploadFile(HttpPostedFileBase file, string filePath, string filePrepend, string fileName, Int32 counter = 0)
        {
            //fileName = Guid.NewGuid().ToString().ToLower(); // Path.GetFileName(file.FileName);
            //var fileExtension = Path.GetExtension(file.FileName);
            
            //string prepend = filePrepend + "item_";
            //string finalFileName = prepend + ((counter).ToString()) + "_" + fileName;

            string finalFileName = fileName;

            if (System.IO.File.Exists(HttpContext.Current.Request.MapPath(filePath + finalFileName)))
            {
                //file exists => add country try again
                return RenameUploadFile(file, filePath, filePrepend, fileName, ++counter);
            }
            //file doesn't exist, upload item but validate first
            return UploadFile(file, filePath, finalFileName);
        }

        private ImageResult UploadFile(HttpPostedFileBase file, string filePath, string fileName)
        {
            ImageResult imageResult = new ImageResult { Success = true, ErrorMessage = null };
            bool exists = System.IO.Directory.Exists(filePath);

            if (!exists)
                System.IO.Directory.CreateDirectory(filePath);

            var path = Path.Combine(HttpContext.Current.Request.MapPath(filePath), fileName);
            string extension = Path.GetExtension(file.FileName);

            //make sure the file is valid
            if (!ValidateExtension(extension))
            {
                imageResult.Success = false;
                imageResult.ErrorMessage = "Invalid Extension";
                return imageResult;
            }

            try
            {
                file.SaveAs(path);

                Image imgOriginal = Image.FromFile(path);

                //pass in whatever value you want 
                Image imgActual = Scale(imgOriginal);
                imgOriginal.Dispose();
                imgActual.Save(path);
                imgActual.Dispose();

                imageResult.ImageName = fileName;

                return imageResult;
            }
            catch (Exception ex)
            {
                // you might NOT want to show the exception error for the user
                // this is generaly logging or testing

                imageResult.Success = false;
                imageResult.ErrorMessage = ex.Message;
                return imageResult;
            }
        }

        private bool ValidateExtension(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                case ".jpg":
                    return true;
                case ".png":
                    return true;
                case ".gif":
                    return true;
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
        }

        public Image Scale(Image imgPhoto)
        {
            Width = imgPhoto.Width > Width ? Width : imgPhoto.Width;

            float sourceWidth = imgPhoto.Width;
            float sourceHeight = imgPhoto.Height;
            float destHeight = 0;
            float destWidth = 0;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            // force resize, might distort image
            if (Width != 0 && Height != 0)
            {
                destWidth = Width;
                destHeight = Height;
            }
            // change size proportially depending on width or height
            else if (Height != 0)
            {
                destWidth = (float)(Height * sourceWidth) / sourceHeight;
                destHeight = Height;
            }
            else
            {
                destWidth = Width;
                destHeight = (float)(sourceHeight * Width / sourceWidth);
            }

            Bitmap bmPhoto = new Bitmap((int)destWidth, (int)destHeight,
                                        PixelFormat.Format32bppPArgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, (int)destWidth, (int)destHeight),
                new Rectangle(sourceX, sourceY, (int)sourceWidth, (int)sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();

            return bmPhoto;
        }

        public ImageResult OpenFileAndResize(string path)
        {
            ImageResult imageResult = new ImageResult { Success = true, ErrorMessage = null };

            Image imgOriginal = Image.FromFile(path);

            //pass in whatever value you want 
            Image imgActual = Scale(imgOriginal);
            imgOriginal.Dispose();
            imgActual.Save(path);
            imgActual.Dispose();

            //imageResult.ImageName = fileName;

            return imageResult;
        }
    }
}