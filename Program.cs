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

        static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            PaulsOKWrapper github;
            // Processors not cores, probably need something else in the future
            int ProcessorCount = Environment.ProcessorCount;

            int NumThreadsToRun = ThreadsPerProcessor * ProcessorCount;

            if(args.Length < 3)
            {
                Console.WriteLine("need atleast 3 args");
                return 1;
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
                return 1;
            }

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

            bool CommitContainedImages = true;

            if(GitHubStuff && github.CleanlyLoggedIn)
            {
                {// Setup the Owner and the Repo
                    Repo = args[4] as string;
                    Owner = args[5] as string;
                    github.SetOwnerAndRepo(Owner,Repo);
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
                    
                    bool commitsetup = await github.SetupCommit();
                    Console.WriteLine("Setup to commit changes: " + commitsetup.ToString());

                    CommitContainedImages = await github.CommitContainedImages(ImagesDirectory);

                }
            }
            if(repopath is string || repopath as string != null)
            {
                github.RepoDirectory = new System.IO.DirectoryInfo(repopath as string);
            }
            else{
                Console.WriteLine("Second Arg must be a directory");
                return 1;
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
                return 1;
            }
            if(!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            if (ImagesDirectory.Exists == false) // VSCode keeps offering to "fix this" for me... 
            {
                Console.WriteLine("Directory [" + ImgDir + "] Doesn't exist");
                return 1;
            }
            if(!(domain is string))
            {
                Console.WriteLine("arg 3 needs to be your domain");
            }

            int successfull = 0;

            if(CommitContainedImages)
            {            
                int NumCommitted = 0;
             
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
                            var increase_count = ir.GenerateThumbnail(id,github);
                            if(github.DoGitHubStuff && increase_count)
                            {
                                ++NumCommitted;
                            }
                        }

                        if(ir.NeedsResize(id))
                        { // when our algorithm gets better, or or image sizes change
                            var increase_count = ir.ResizeImages(id,github);
                            if(github.DoGitHubStuff && increase_count)
                            {
                                ++NumCommitted;
                            }
                        }

                    }catch(Exception ex){
                        Console.WriteLine(ex.ToString());   
                    }
                }
                Console.WriteLine("Images have been resized");

                Console.WriteLine("fixing up paths");
                DD.FixUpPaths(github.RepoDirectory);


                github.SetNumberImagesCommitted(NumCommitted);

                if(github.DoGitHubStuff && NumCommitted >= 1) // Need more than 1 changed file to try and commit things
                {
                    Console.WriteLine(NumCommitted + " Images were generated, now to push to push them to master");

                    int RateLimitCallsLeft = github.DecrementAPICallsBy(0);
                    if( (NumCommitted * 2) > RateLimitCallsLeft )
                    {
                        Console.WriteLine();
                        Console.WriteLine("WARNING!!!!");
                        Console.WriteLine();
                        Console.WriteLine("WARNING!!!!");
                        Console.WriteLine();
                        Console.WriteLine("WARNING!!!!");
                        Console.WriteLine();
                        Console.WriteLine("GitHub API Will rate limit to 1000 calls an hour. you are adding " + NumCommitted + " files and you have " + RateLimitCallsLeft + " API calls left");
                        Console.WriteLine();
                        Console.WriteLine("WARNING!!!!");
                        Console.WriteLine();
                        Console.WriteLine("WARNING!!!!");
                        Console.WriteLine();
                        Console.WriteLine("WARNING!!!!");
                        Console.WriteLine();
                    }

                    //https://laedit.net/2016/11/12/GitHub-commit-with-Octokit-net.html
                    if(!await github.CommitAndPush())
                    {
                        successfull = 2;
                    }

                    Console.WriteLine(" --- ");
                }else{
                    Console.WriteLine("No images were altered, nothing to push to main");
                }
                
                Console.WriteLine("Domain Set to: " + domain );

                DD.SaveMDFiles(domain, ImagesDirectory, github); //This follows another path...
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

                if(github.DoGitHubStuff && NumCommitted >= 1)
                {
                    Console.WriteLine("Committing Json files");

                    await github.ImmediatlyAddorUpdateTextFile(fi);

                    System.IO.FileInfo NewPath = new System.IO.FileInfo(args[1] + "\\_includes\\gallery.json");
                    if(NewPath.Exists)
                    {
                        NewPath.Delete();
                    }
                    System.IO.File.Copy( fi.FullName, NewPath.FullName);
                    
                    await github.ImmediatlyAddorUpdateTextFile(NewPath);

                    Console.WriteLine("Json files Committed");
                    Console.WriteLine(" --- ");
                }else{
                    Console.WriteLine("No Images Altered, No Json files Committed");
                    Console.WriteLine(" --- ");
                }

                // do we need a synronication point here? lots of things could be going on in parallel
                // DO NOT TRY AND GET CLEVER AND || your github api usage. it will end in tears
                // if you are looking at this call, you are about to do something stupid, don't do it.
                await github.SyncPoint(true);

            }

            bool APICALLSAVING = true; //code that we are only doing because we have 1k API calls an hour

            if(github.DoGitHubStuff)
            {
                if(!github.CleanlyLoggedIn)
                {
                    Console.WriteLine("GitHub Login State unclear, bad things may happen");
                }
                   
                string PRname = "From " + CurrentBranch + " to " + GHPages;

                bool StalePRSuccess = await github.FindStalePullRequests(PRname);
                
                
                if(!APICALLSAVING && StalePRSuccess)
                {
                    //would prefer to close out and re-open, but need to save API Calls
                    Console.WriteLine("add Attempt to close stale pull request here");
                    StalePRSuccess = await github.CloseStalePullRequests(PRname); 
                }

                if(!StalePRSuccess && successfull == 0)
                {
                    successfull = 3;
                } 

                if(APICALLSAVING)
                {
                    // would prefer to close PR"s and re-open, but gotta save them API calls, 
                    //    only get 1k an hour
                    if(!StalePRSuccess)
                    {
                        bool CreatePRSuccess = await github.CreateAndLabelPullRequest(PRname);
                        if(!CreatePRSuccess && successfull == 0)
                        {
                            successfull = 4;
                        } 

                    }else{
                        Console.WriteLine("PR Already exists, so not creating one - saving API calls");
                    }

                }else{
                    
                    bool CreatePRSuccess = await github.CreateAndLabelPullRequest(PRname);
                    if(!CreatePRSuccess && successfull == 0)
                    {
                        successfull = 4;
                    } 
                }

                github.RefreshRateLimits();
                Console.WriteLine("Run has finished Exiting...");
                Console.WriteLine("Current Code is : " + successfull.ToString());
            }
            return successfull;
        }
    }
}
