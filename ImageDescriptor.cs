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
        
        [JsonPropertyName("GalleryName")]
        public string GalleryName { get => directoryName; }
        
        [JsonPropertyName("DirectoryName")]
        public string DirectoryName { get => "/" + directoryName; }
        protected string directoryName;
        
        [JsonPropertyName("FullDirectoryPath")]
        public string FullPath { get => "/" + fullPath.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/"); }
        private string fullPath;
        
        [JsonPropertyName("PhotoGalleries")] //JsonExtensionData
        public List<DirectoryDescriptor> Directories { get => directories; set => directories = value; }
        private System.Collections.Generic.List<DirectoryDescriptor> directories = new System.Collections.Generic.List<DirectoryDescriptor>();
        
        [JsonPropertyName("Images")]
        public List<ImageDescriptor> Images { get => images; set => images = value; }
        private System.Collections.Generic.List<ImageDescriptor> images = new System.Collections.Generic.List<ImageDescriptor>();

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
        public void SaveMDFiles(string Domain)
        {
            foreach(DirectoryDescriptor dd in directories)
            {
                dd.SaveMDFiles(Domain);
            }
            System.IO.FileInfo fi = new FileInfo(this.fullPath + "\\" + "Index.md");
            
            //we need to overwrite them even if they exist (but we need some fortmat to allow edits)
            {
                CreateIndexFile(fi, Domain);
                System.Console.WriteLine("Create : " + fi.FullName);
            }
        }


        public void CreateIndexFile(System.IO.FileInfo fi, string Domain)
        {
            System.IO.TextWriter tw = fi.CreateText();

            tw.WriteLine("# " + this.directoryName);
            tw.WriteLine();
            tw.WriteLine("----");
            foreach(ImageDescriptor i in images)
            {
                WriteImage(tw,i, Domain);
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

        public void WriteImage(System.IO.TextWriter textWriter, ImageDescriptor id, string Domain)
        {
            textWriter.WriteLine("[![" + id.Name  + "](" + Domain + "/" + id.ThumbNailFile.FullName.Replace(GitHubRepoRoot.FullName,"").Replace(@"\","/") + ")](" + id.ReSizedFileInfo.Name + ")" );
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
        
        
        private int width;
        public int ImageWidth { get => width; set => width = value; }
        private int height;
        public int ImageHeight { get => height; set => height = value; }
        private string folder;
        public string Folder { get => folder; set => folder = value; }
        
        [JsonIgnore]
        public FileInfo ImageFile { get => file; }
        [JsonPropertyName("OriginalFilePath")]
        public string JSON_ImageFIle { get => file.FullName.Replace(GitHubRepoRoot.FullName,"");}
        protected FileInfo file;  
        private string name;
        public string Name { get => name; set => name = value; }
        
        private string thumbnail_name;
        protected System.IO.FileInfo thumbNailFile;
        public string ThumbnailName { get => thumbnail_name; set => thumbnail_name = value; }
        [JsonIgnore]
        public FileInfo ThumbNailFile { get => thumbNailFile; set => thumbNailFile = value; }
        [JsonPropertyName("ThumbnailFilePath")]
        public string JSON_ThumbnailFile { get => thumbNailFile.FullName.Replace(GitHubRepoRoot.FullName,"");}

        protected FileInfo reSizedFileInfo; 
        [JsonIgnore]
        public string ReSizedFileName { get => reSizedFileName; set => reSizedFileName = value; }
        private string reSizedFileName;
        [JsonIgnore]
        public FileInfo ReSizedFileInfo { get => reSizedFileInfo; set => reSizedFileInfo = value; }
        [JsonPropertyName("ResizedFilePath")]
        public string JSON_ResizedImage { get => reSizedFileInfo.FullName.Replace(GitHubRepoRoot.FullName,"");}

        protected DirectoryInfo directory;
        private DirectoryInfo GitHubRepoRoot;

        public ImageDescriptor(System.IO.DirectoryInfo di, System.IO.FileInfo fi)
        {
            directory = di;
            file = fi;
            name = fi.Name;
            folder = di.FullName;

            thumbnail_name = ImageDescriptor.thumbnail + "-" + directory.Name + "-" + file.Name;
            ReSizedFileName = folder + "\\" +  ImageDescriptor.resized  + "-" + name;
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

            thumbnail_name = "/" + thumbnail_name.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/");
            ReSizedFileName = "/" + ReSizedFileName.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/");
            folder = "/" + folder.Replace(GitHubRepoRoot.FullName,"").Replace("\\","/");
        }
    }
}
