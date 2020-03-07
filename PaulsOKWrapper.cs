using System;
using System.IO;
using Octokit;
using System.Threading.Tasks;

namespace CSharp_Image_Action
{
    // It's just an OK wrapper, not a great wrapper
    // we are wrappign OctoKit
    public class PaulsOKWrapper
    {
        protected string repo = "Repo";
        protected string owner = "Owner";

        bool gitHubStuff = false;
        private bool cleanlyLoggedIn = false;
        private GitHubClient github = null;
        System.IO.DirectoryInfo repoDirectory; 

        public GitHubClient GithubClient { get => github; set => github = value; }
        public bool CleanlyLoggedIn { get => cleanlyLoggedIn; }
        public DirectoryInfo RepoDirectory { get => repoDirectory; set => repoDirectory = value; }
        public bool DoGitHubStuff { get => gitHubStuff; set => gitHubStuff = value; }

        protected string email;
        protected string username;

        public PaulsOKWrapper(bool gitHubStuff)
        {
            this.gitHubStuff = gitHubStuff;
            this.username = "GitHub Action";
            this.email = "actions@users.noreply.github.com";
        }

        public bool SetOwnerAndRepo(string p_Owner, string p_Repo)
        {
            this.owner = p_Owner;
            this.repo = p_Repo;
            Console.WriteLine("Owner Set to : " + this.owner);
            Console.WriteLine("Repo set to : " + this.repo);
            return true;
        }

        public bool AttemptLogin()
        {
            try{
                Console.WriteLine("Loading github...");
                string secretkey = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                github = new GitHubClient(new ProductHeaderValue("Pauliver-ImageTool"))
                {
                    Credentials = new Credentials(secretkey)
                };
                cleanlyLoggedIn = true; // or maybe
                Console.WriteLine("... Loaded");
                return true;
            }catch(Exception ex){
                Console.WriteLine(ex.ToString());
                Console.WriteLine("... Loading Failed");
                cleanlyLoggedIn = false;
                return false;
            }
        }

        protected string CurrentBranchName;
        protected string TargetBranchName;
        protected string AutoMergeLabel;
        bool SetupForMergByLabel = false;
        public bool SetupForMergesByLabel(string p_autoMergeLabel = "automerge", string currentBranch = "master", string targetBranch = "gh-pages")
        {
            this.CurrentBranchName = currentBranch;
            this.TargetBranchName = targetBranch;
            this.AutoMergeLabel = p_autoMergeLabel;

            SetupForMergByLabel = true;
            return true;
        } 

        public bool AddorUpdateFile(System.IO.FileInfo fi)
        {
            // Is the file in the repo?
            // - if not add it
            // - if it is update it
            return true;
        }

        public bool SomethingAboutCommittingAnImage(System.IO.FileInfo fi)
        {
            return AddorUpdateFile(fi);
        }

        public async ValueTasks<Boolean> FindStalePullRequests(string PRname)
        {
            bool ShouldClose = false;

            var prs = await github.PullRequest.GetAllForRepository(owner,repo);
            
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
            return true;
        }

        async public ValueTasks<bool> CreateAndLabelPullRequest(string PRname)
        {
            Console.WriteLine("PR: " + PRname);
            Console.WriteLine("Owner: " + owner);
            Console.WriteLine("CurrentBranch: " + CurrentBranchName);
            Console.WriteLine("TargetBranch: " + TargetBranchName);

            NewPullRequest newPr = new NewPullRequest(PRname + " : " + System.DateTime.UtcNow.ToString(),CurrentBranchName,TargetBranchName);
            PullRequest pullRequest = await github.PullRequest.Create(owner,repo,newPr);
            
            Console.WriteLine("PR Created # : " + pullRequest.Number);

            Console.WriteLine("PR Created: " + pullRequest.Title);

            //var prupdate = new PullRequestUpdate();
            //var newUpdate = await github.PullRequest.Update(Owner,Repo,pullRequest.Number,prupdate);

            try{

                Console.WriteLine("Owner: " + PRname);        
                Console.WriteLine("Repo: " + repo);
                Console.WriteLine("pullRequest.Number: " + pullRequest.Number);

                if(github == null){  
                    Console.WriteLine("github == null");
                }
                if(github.Issue == null){
                    Console.WriteLine("github.Issue == null");
                }

                var issue = await github.Issue.Get(owner, repo, pullRequest.Number);
                if(issue != null) //https://octokitnet.readthedocs.io/en/latest/issues/
                {
                    var issueUpdate = issue.ToUpdate();
                    if(issueUpdate != null)
                    {
                        issueUpdate.AddLabel(AutoMergeLabel);
                        var labeladded = await github.Issue.Update(owner, repo, pullRequest.Number, issueUpdate);
                        Console.WriteLine("Label Added: " + AutoMergeLabel);
                    }
                }

            }catch(Exception ex){
                Console.WriteLine(ex.ToString());  
                return false; 
            }
        return true;
        }
        
    }

    public class PaulsOKSIngleton
    {

    }
}