using System;

namespace CSharp_Image_Action
{
    class Program
    {
        public static string Jekyll_data_Folder = "_data";
        public static string Jekyll_data_File = "images.json";
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

            ImageResizer ir = new ImageResizer(/* set options here */);

            foreach(ImageDescriptor id in ImagesList)
            {
                if(ir.ThumbnailNeeded(id))
                    ir.GenerateThumbnail(id);

                if(ir.NeedsResize(id)) // when our algorithm gets better, or or image sizes change
                    ir.ResizeImages(id);
            }

        }
    }
}
