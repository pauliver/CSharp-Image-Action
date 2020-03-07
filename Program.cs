using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.Json;
using System.Text.Json.Serialization;
using Octokit;

namespace CSharp_Image_Action
{
    class Program
    {
        public static string Jekyll_data_Folder = "_data";
        public static string Jekyll_data_File = "galleryjson.json";
        public static string THUMBNAILS = "Thumbnails";
        public static string GENERATED = "\\Generated";

        // Tweak this over time
        // - if we have 2 threads per processor
        // - and spend a lot of time in File IO
        // - we can accomidate for it here
        public static int ThreadsPerProcessor = 4;

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            PaulsOKWrapper github;
            // Processors not cores, probably need something else in the future
            int ProcessorCount = Environment.ProcessorCount;

            int NumThreadsToRun = ThreadsPerProcessor * ProcessorCount;

            if(args.Length < 3)
            {
                Console.WriteLine("need atleast 3 args");
                return;
            }
            string[] extensionList = new string[]{".jpg",".png",".jpeg", ".JPG", ".PNG", ".JPEG", ".bmp", ".BMP"};
            System.IO.DirectoryInfo ImagesDirectory;
            System.IO.FileInfo fi;

           
            var CurrentBranch = "master";
            var GHPages = "gh-pages";
            var AutoMergeLabel = "automerge";
            var Repo = "Repo";
            var Owner = "Owner";

            var ImgDir = args[0];
            var jsonPath = args[1] + "\\" + Jekyll_data_Folder + "\\" + Jekyll_data_File;
            var repopath = args[1];
            var domain = args[2];

            var GitHubStuff = false;
            if(args.Length >= 4 && args[3] is string && bool.Parse(args[3]))
            {
                GitHubStuff = bool.Parse(args[3]);
            }
            
            github = new PaulsOKWrapper(GitHubStuff);
            if(GitHubStuff)
            {
                github.AttemptLogin();
            }

            if(GitHubStuff && github.CleanlyLoggedIn)
            {
                {// Setup the Owner and the Repo
                    Repo = args[4] as string;
                    Owner = args[5] as string;
                    github.SetOwnerAndRepo(Owner,repopath);
                }
                {// Setup for merge between branches by label
                    if(args.Length >= 7 && args[6] is string)
                    {
                        CurrentBranch = args[6] as string;
                    }
                    if(args.Length >= 8 && args[7] is string)
                    {
                        GHPages = args[7] as string;
                    }
                    if(args.Length >= 9 && args[8] is string)
                    {
                        AutoMergeLabel = args[8] as string;
                    }
                    github.SetupForMergesByLabel(AutoMergeLabel,CurrentBranch,GHPages);
                }
            }
            if(repopath is string || repopath as string != null)
            {
                github.RepoDirectory = new System.IO.DirectoryInfo(repopath as string);
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
            if(!(domain is string))
            {
                Console.WriteLine("arg 3 needs to be your domain");
            }


            DirectoryDescriptor DD = new DirectoryDescriptor(ImagesDirectory.Name, ImagesDirectory.FullName);

            // Traverse Image Directory
            ImageHunter ih = new ImageHunter(ref DD,ImagesDirectory,extensionList);

            string v = ih.NumberImagesFound.ToString();
            Console.WriteLine(v + " Images Found") ;
            
            var ImagesList = ih.ImageList;

            System.IO.DirectoryInfo thumbnail = new System.IO.DirectoryInfo(ImgDir + "\\" + THUMBNAILS);

            if(!thumbnail.Exists)
                thumbnail.Create();

            Console.WriteLine("Images to be resized");

            ImageResizer ir = new ImageResizer(thumbnail,256, 256, 1024, 1024, true, true);

            //@@ Add Multithreading via thread pool here
            foreach(ImageDescriptor id in ImagesList)
            {
                try{
                    System.Console.WriteLine("Image: " +  id.Name);
                    id.FillBasicInfo();

                    if(ir.ThumbnailNeeded(id))
                    {
                        ir.GenerateThumbnail(id,github);
                    }

                    if(ir.NeedsResize(id))
                    { // when our algorithm gets better, or or image sizes change
                        ir.ResizeImages(id,github);
                    }

                }catch(Exception ex){
                    Console.WriteLine(ex.ToString());   
                }
            }
            Console.WriteLine("Images have been resized");

            Console.WriteLine("fixing up paths");
            DD.FixUpPaths(github.RepoDirectory);
            
            DD.SaveMDFiles(domain, ImagesDirectory);
            Console.WriteLine("Image indexes written");


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

            {
                Console.WriteLine("Committing Json files");

                github.AddorUpdateFile(fi);

                System.IO.FileInfo NewPath = new System.IO.FileInfo(args[1] + "\\_includes\\gallery.json");
                if(NewPath.Exists)
                {
                    NewPath.Delete();
                }
                System.IO.File.Copy( fi.FullName, NewPath.FullName);
                
                github.AddorUpdateFile(NewPath);

                Console.WriteLine("Json files Committed");
                Console.WriteLine(" --- ");
            }
            
            {
                Console.WriteLine("now to push to master");

                Console.WriteLine(" --- ");
            }

            if(github.DoGitHubStuff)
            {
                if(!github.CleanlyLoggedIn)
                {
                    Console.WriteLine("GitHub Login State unclear, Exiting");
                    return;
                }
                   
                string PRname = "From " + CurrentBranch + " to " + GHPages;

                bool StalePRSuccess = await github.FindStalePullRequests(PRname);

                bool CreatePRSuccess = await github.CreateAndLabelPullRequest(PRname);
                

                Console.WriteLine("Bailing Out...");
            }
        }
    }
}
