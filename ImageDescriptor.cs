using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Soap;
//using System.Runtime.Serialization.Formatters.Binary;

namespace CSharp_Image_Action
{
    [Serializable]
    public class DirectoryDescriptor
    {
        [JsonPropertyName("Images")]
        public List<ImageDescriptor> Images { get => images; set => images = value; }
        [JsonPropertyName("SubDirectories")] //JsonExtensionData
        public List<DirectoryDescriptor> Directories { get => directories; set => directories = value; }
        [JsonPropertyName("DirectoryName")]
        public string DirectoryName { get => directoryName; }
        [JsonPropertyName("FullDirectoryPath")]
        public string FullPath { get => fullPath.Replace(GitHubRepoRoot.FullName,""); }

        private System.Collections.Generic.List<ImageDescriptor> images = new System.Collections.Generic.List<ImageDescriptor>();
        private System.Collections.Generic.List<DirectoryDescriptor> directories = new System.Collections.Generic.List<DirectoryDescriptor>();
        protected string directoryName;
        private string fullPath;

        private DirectoryInfo GitHubRepoRoot;

        public DirectoryDescriptor(string directoryname, string fullpath)
        {
            this.directoryName = directoryname;
            this.fullPath = fullpath;
        }

        public void FixUpPaths(DirectoryInfo di)
        {
            GitHubRepoRoot = di;
            foreach(DirectoryDescriptor dd in directories)
            { 
                dd.FixUpPaths(di);
            }
            foreach(ImageDescriptor i in images)
            {
                i.FixUpPaths(di);
            }
        }
        public void SaveMDFiles()
        {
            foreach(DirectoryDescriptor dd in directories)
            {
                dd.SaveMDFiles();
            }
            System.IO.FileInfo fi = new FileInfo(this.fullPath + "\\" + "Index.md");
            
            //we need to overwrite them even if they exist (but we need some fortmat to allow edits)
            {
                CreateIndexFile(fi);
                System.Console.WriteLine("Create : " + fi.FullName);
            }
        }

        public void CreateIndexFile(System.IO.FileInfo fi)
        {
            System.IO.FileStream fs = fi.OpenWrite();
            System.IO.TextWriter tw = new System.IO.StreamWriter(fs);

            tw.WriteLine("# " + this.directoryName);
            tw.WriteLine();
            tw.WriteLine("----");
            foreach(ImageDescriptor i in images)
            {
                WriteImage(tw,i);
            }
            tw.WriteLine();
            tw.WriteLine("----");
            tw.WriteLine();
            foreach(DirectoryDescriptor d in Directories)
            {
                WriteDirectory(tw,d);
            }
            tw.WriteLine();
            tw.WriteLine("----");
            tw.Flush();
            tw.Close();
        }

        public void WriteImage(System.IO.TextWriter textWriter, ImageDescriptor id)
        {
            textWriter.WriteLine("[![" + id.Name  + "](" + id.ThumbNailFile.FullName.Replace(GitHubRepoRoot.FullName,"") + ")](" + id.ReSizedFileInfo.Name + ")" );
        }
        public void WriteDirectory(System.IO.TextWriter textWriter, DirectoryDescriptor dd)
        {
            textWriter.WriteLine("[" + dd.DirectoryName + "]( ./"+ dd.DirectoryName + "/Index.md )");
            textWriter.WriteLine();
        }
    }

    [Serializable()]
    public class ImageDescriptor
    {
        # region Static
        public static string thumbnail = "thumbnail";
        public static string resized = "resized";
        
        #endregion

        private string thumbnail_name;
        protected System.IO.FileInfo thumbNailFile;
        public string ThumbnailName { get => thumbnail_name; set => thumbnail_name = value; }
        [JsonIgnore]
        public FileInfo ThumbNailFile { get => thumbNailFile; set => thumbNailFile = value; }
        [JsonPropertyName("ThumbnailFilePath")]
        public string JSON_ThumbnailFile { get => thumbNailFile.FullName.Replace(GitHubRepoRoot.FullName,"");}


        protected FileInfo file;  
        private string name;
        public string Name { get => name; set => name = value; }
        [JsonIgnore]
        public FileInfo ImageFile { get => file; }
        [JsonPropertyName("OriginalFilePath")]
        public string JSON_ImageFIle { get => file.FullName.Replace(GitHubRepoRoot.FullName,"");}
              


        protected FileInfo reSizedFileInfo; 
        public string ReSizedFileName { get => reSizedFileName; set => reSizedFileName = value; }
        private string reSizedFileName;
        [JsonIgnore]
        public FileInfo ReSizedFileInfo { get => reSizedFileInfo; set => reSizedFileInfo = value; }
         [JsonPropertyName("ResizedFilePath")]
        public string JSON_ResizedImage { get => reSizedFileInfo.FullName.Replace(GitHubRepoRoot.FullName,"");}

        
        
        private string folder;
        public string Folder { get => folder; set => folder = value; }

        
        private int height;
        public int ImageHeight { get => height; set => height = value; }

        
        private int width;
        public int ImageWidth { get => width; set => width = value; }




        protected DirectoryInfo directory;
        private DirectoryInfo GitHubRepoRoot;

        public ImageDescriptor(System.IO.DirectoryInfo di, System.IO.FileInfo fi)
        {
            directory = di;
            file = fi;
            name = fi.Name;
            folder = di.FullName;

            thumbnail_name = ImageDescriptor.thumbnail + "-" + directory.Name + "-" + file.Name;
            ReSizedFileName = Folder + "\\" +  ImageDescriptor.resized  + "-" + Name;
        }

        public void FillBasicInfo()
        {
            var inStream = file.OpenRead();
            using (Image image = Image.Load(inStream))
            {
                height = image.Height;
                width = image.Width;
            }
            inStream.Close();
        }

        public void FixUpPaths(System.IO.DirectoryInfo di)
        {
            this.GitHubRepoRoot = di;

            thumbnail_name = thumbnail_name.Replace(GitHubRepoRoot.FullName,"");
            ReSizedFileName = ReSizedFileName.Replace(GitHubRepoRoot.FullName,"");
            folder = folder.Replace(GitHubRepoRoot.FullName,"");
        }
    }
}