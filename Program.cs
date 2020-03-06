using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL;
using GraphQLParser;
using Octokit;

namespace CSharp_Image_Action
{
    class Program
    {
        public static string Jekyll_data_Folder = "_data";
        public static string Jekyll_data_File = "galleryjson.json";
        public static string THUMBNAILS = "Thumbnails";
        public static string GENERATED = "\\Generated";

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            if(args.Length < 3)
            {
                Console.WriteLine("need atleast 3 args");
                return;
            }
            string[] extensionList = new string[]{".jpg",".png",".jpeg", ".JPG", ".PNG", ".JPEG", ".bmp", ".BMP"};
            System.IO.DirectoryInfo ImagesDirectory;
            System.IO.DirectoryInfo RepoDirectory; 
            System.IO.FileInfo fi;
            bool CleanlyLoggedIn = false;
            GitHubClient github = null;

           
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
            
            if(GitHubStuff)
            {
                try{
                    Console.WriteLine("Loading github...");
                    string secretkey = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                    github = new GitHubClient(new ProductHeaderValue("Pauliver-ImageTool"))
                    {
                        Credentials = new Credentials(secretkey)
                    };
                    CleanlyLoggedIn = true; // or maybe
                    Console.WriteLine("... Loaded");
                }catch(Exception ex){
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("... Loading Failed");
                    CleanlyLoggedIn = false;
                }
            }

            if(GitHubStuff)
            {
                Repo = args[4] as string;
                Owner = args[5] as string;
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
            }
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

            foreach(ImageDescriptor id in ImagesList)
            {
                try{
                    System.Console.WriteLine("Image: " +  id.Name);
                    id.FillBasicInfo();

                    if(ir.ThumbnailNeeded(id))
                        ir.GenerateThumbnail(id);

                    if(ir.NeedsResize(id)) // when our algorithm gets better, or or image sizes change
                        ir.ResizeImages(id);
                }catch(Exception ex){
                    Console.WriteLine(ex.ToString());   
                }
            }
            Console.WriteLine("Images have been resized");

            Console.WriteLine("fixing up paths");
            DD.FixUpPaths(RepoDirectory);
            
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

            /*if(false)
            {
                Console.WriteLine(" -- Merge new files -- ");

                try{
                    using (var repo = new LibGit2Sharp.Repository(RepoDirectory.FullName) )
                    {
                        //LibGit2Sharp.Commit commit = repo.Head.Tip;
                        LibGit2Sharp.Commands.Stage(repo, "*");
                        repo.Index.Write();
                        
                        LibGit2Sharp.Signature committer = new LibGit2Sharp.Signature("GitHub Action", "actions@users.noreply.github.com", DateTime.Now);

                        // Commit to the repository
                        LibGit2Sharp.Commit commit = repo.Commit("Add resized images", committer, committer, new LibGit2Sharp.CommitOptions());
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }*/

            Console.WriteLine(" --- ");

            if(GitHubStuff)
            {
                if(!CleanlyLoggedIn)
                {
                    Console.WriteLine("GitHub Login State unclear, Exiting");
                    return;
                }
                
                string PRname = "From " + CurrentBranch + " to " + GHPages;
                
                bool ShouldClose = false;

                var prs = await github.PullRequest.GetAllForRepository(Owner,Repo);
                
                foreach(PullRequest pr in prs)
                {
                    foreach(Label l in pr.Labels)
                    {
                        ShouldClose = false;
                        if(l.Name == AutoMergeLabel && pr.Title.Contains(PRname)) // I'm left over from a previous run
                        {
                            ShouldClose = true;
                        }
                        if(ShouldClose)
                        {
                            Console.WriteLine("It looks like you have an existing PR still open");
                            Console.WriteLine("This is likely to fail, unless you close : " + pr.Title);
                        }
                        ShouldClose = false;
                    }
                }
                
                
                Console.WriteLine("PR: " + PRname);
                Console.WriteLine("Owner: " + Owner);
                Console.WriteLine("CurrentBranch: " + CurrentBranch);
                Console.WriteLine("TargetBranch: " + GHPages);

                NewPullRequest newPr = new NewPullRequest(PRname + " : " + System.DateTime.UtcNow.ToString(),CurrentBranch,GHPages);
                PullRequest pullRequest = await github.PullRequest.Create(Owner,Repo,newPr);
                
                Console.WriteLine("PR Created # : " + pullRequest.Number);

                Console.WriteLine("PR Created: " + pullRequest.Title);

                //var prupdate = new PullRequestUpdate();
                //var newUpdate = await github.PullRequest.Update(Owner,Repo,pullRequest.Number,prupdate);

                try{

                    Console.WriteLine("Owner: " + PRname);        
                    Console.WriteLine("Repo: " + Repo);
                    Console.WriteLine("pullRequest.Number: " + pullRequest.Number);

                    if(github == null){  
                        Console.WriteLine("github == null");
                    }
                    if(github.Issue == null){
                        Console.WriteLine("github.Issue == null");
                    }

                    var issue = await github.Issue.Get(Owner, Repo, pullRequest.Number);
                    if(issue != null) //https://octokitnet.readthedocs.io/en/latest/issues/
                    {
                        var issueUpdate = issue.ToUpdate();
                        if(issueUpdate != null)
                        {
                            issueUpdate.AddLabel(AutoMergeLabel);
                            var labeladded = await github.Issue.Update(Owner, Repo, pullRequest.Number, issueUpdate);
                            Console.WriteLine("Label Added: " + AutoMergeLabel);
                        }
                    }

                }catch(Exception ex){
                    Console.WriteLine(ex.ToString());   
                }

                Console.WriteLine("Bailing Out...");
            }
        }
    }
}
