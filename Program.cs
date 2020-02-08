using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp_Image_Action
{
    class Program
    {
        public static string Jekyll_data_Folder = "_data";
        public static string Jekyll_data_File = "images.json";
        public static string THUMBNAILS = "\\Thumbnails";

        static void Main(string[] args)
        {
            var ImgDir = args[0];
            System.IO.DirectoryInfo di;
            if (ImgDir is string)
            {
                di = new System.IO.DirectoryInfo(ImgDir);
            }
            else if (ImgDir as String != null)
            {
                di = new System.IO.DirectoryInfo(ImgDir as string);
            }
            else
            {
                Console.WriteLine("First Arg must be a directory");
                return;
            }

            if (di.Exists == false) // VSCode keeps offering to "fix this" for me... 
            {
                Console.WriteLine("Directory [" + ImgDir + "] Doesn't exist");
                return;
            }

            string[] extensionList = new string[]{".jpg",".png",".jpeg"};

            // Traverse Image Directory
            ImageHunter ih = new ImageHunter(di,extensionList);

            string v = ih.NumberImagesFound.ToString();
            Console.WriteLine(v + " Images Found") ;
            
            var ImagesList = ih.ImageList;

            System.IO.DirectoryInfo thumbnaildi = new System.IO.DirectoryInfo(ImgDir + THUMBNAILS);

            ImageResizer ir = new ImageResizer(thumbnaildi,256, 256, 1024, 1024, true, true);

            foreach(ImageDescriptor id in ImagesList)
            {
                id.FillBasicInfo();

                if(ir.ThumbnailNeeded(id))
                    ir.GenerateThumbnail(id);

                if(ir.NeedsResize(id)) // when our algorithm gets better, or or image sizes change
                    ir.ResizeImages(id);
            }

            // Generate 1 sets of json to save (1 deep tree?)
            // -> Gallery structure Gallery needs a thumbnail, and a name
            //   -> we probably need to generate a markdown page for it too... 
            //      -> Can hide more structure in the markdown page

            //https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
            //jsonString = JsonSerializer.Serialize(weatherForecast);
            //File.WriteAllText(fileName, jsonString);
        }
    }
}
