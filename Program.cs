using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp_Image_Action
{
    class Program
    {
        public static string Jekyll_data_Folder = "_data";
        public static string Jekyll_data_File = "images.json";
        public static string THUMBNAILS = "\\Thumbnails";

        public static string GENERATED = "\\Generated";

        static void Main(string[] args)
        {
            var ImgDir = args[0];
            var jsonPath = args[1] + "\\" + Jekyll_data_Folder + "\\" + Jekyll_data_File;
            var repopath = args[1];
            System.IO.DirectoryInfo ImagesDirectory;
            System.IO.DirectoryInfo RepoDirectory; 
            System.IO.FileInfo fi;
            if(repopath is string || repopath as string != null)
            {
                RepoDirectory = new System.IO.DirectoryInfo(repopath as string);
            }
            else{
                Console.WriteLine("Second Arg must be a directory");
                return;
            }
            if (ImgDir is string)
            {
                ImagesDirectory = new System.IO.DirectoryInfo(ImgDir);
            }
            else if (ImgDir as String != null)
            {
                ImagesDirectory = new System.IO.DirectoryInfo(ImgDir as string);
            }
            else
            {
                Console.WriteLine("First Arg must be a directory");
                return;
            }
            if (jsonPath is string)
            {
                fi = new System.IO.FileInfo(jsonPath);
            }
            else if (jsonPath as String != null)
            {
                fi = new System.IO.FileInfo(jsonPath as string);
            }
            else
            {
                Console.WriteLine("Second Arg must be a directory that can lead to " + "\\" + Jekyll_data_Folder + "\\" + Jekyll_data_File);
                return;
            }
            if(!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            if (ImagesDirectory.Exists == false) // VSCode keeps offering to "fix this" for me... 
            {
                Console.WriteLine("Directory [" + ImgDir + "] Doesn't exist");
                return;
            }


            string[] extensionList = new string[]{".jpg",".png",".jpeg"};
            DirectoryDescriptor DD = new DirectoryDescriptor(ImagesDirectory.Name, ImagesDirectory.FullName);
            // Traverse Image Directory
            ImageHunter ih = new ImageHunter(ref DD,ImagesDirectory,extensionList);

            string v = ih.NumberImagesFound.ToString();
            Console.WriteLine(v + " Images Found") ;
            
            var ImagesList = ih.ImageList;

            System.IO.DirectoryInfo thumbnail = new System.IO.DirectoryInfo(ImgDir + THUMBNAILS);
            if(!thumbnail.Exists)
                thumbnail.Create();

            Console.WriteLine("Images to be resized");

            ImageResizer ir = new ImageResizer(thumbnail,256, 256, 1024, 1024, true, true);

            foreach(ImageDescriptor id in ImagesList)
            {
                id.FillBasicInfo();

                if(ir.ThumbnailNeeded(id))
                    ir.GenerateThumbnail(id);

                if(ir.NeedsResize(id)) // when our algorithm gets better, or or image sizes change
                    ir.ResizeImages(id);

            }
            Console.WriteLine("Images have been resized");

            Console.WriteLine("fixing up paths");
            DD.FixUpPaths(RepoDirectory);
            
            DD.SaveMDFiles();
            Console.WriteLine("Image indexes written");
            //https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
            // Generate 1 sets of json to save (1 deep tree?)
            // -> Gallery structure Gallery needs a thumbnail, and a name
            //   -> we probably need to generate a markdown page for it too... 
            //      -> Can hide more structure in the markdown page

            var encoderSettings = new TextEncoderSettings();
            encoderSettings.AllowCharacters('\u0436', '\u0430');
            encoderSettings.AllowRange(UnicodeRanges.BasicLatin);
            var options = new JsonSerializerOptions
            {
                IgnoreReadOnlyProperties = false,
                WriteIndented = true,
                IgnoreNullValues = false,
                //Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            };
            var jsonString = JsonSerializer.Serialize<DirectoryDescriptor>(DD, options);
            {
                var fs = fi.Create();
                System.IO.TextWriter tw = new System.IO.StreamWriter(fs);
                tw.Write(jsonString);
                tw.Close();
                //fs.Close();    
            }
            Console.WriteLine("Json written");
        }
    }
}
