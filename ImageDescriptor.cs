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
        public string DirectoryName { get => directoryName; set => directoryName = value; }
        [JsonPropertyName("FullDirectoryPath")]
        public string FullPath { get => fullPath; set => fullPath = value; }

        private System.Collections.Generic.List<ImageDescriptor> images = new System.Collections.Generic.List<ImageDescriptor>();
        private System.Collections.Generic.List<DirectoryDescriptor> directories = new System.Collections.Generic.List<DirectoryDescriptor>();
        protected string directoryName;
        private string fullPath;


        public DirectoryDescriptor(string directoryname, string fullpath)
        {
            this.directoryName = directoryname;
            this.fullPath = fullpath;
        }

        public void SaveMDFiles()
        {
            foreach(DirectoryDescriptor dd in directories)
            {
                dd.SaveMDFiles();
            }
            System.IO.FileInfo fi = new FileInfo(this.fullPath + "\\" + directoryName + "Index.md");
            
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
            tw.WriteLine("----");
            foreach(DirectoryDescriptor d in Directories)
            {
                WriteDirectory(tw,d);
            }
            tw.WriteLine("----");
        }

        public void WriteImage(System.IO.TextWriter textWriter, ImageDescriptor id)
        {
            textWriter.Write("[![" + id.Name  + "](" + id.ThumbNailFile + ")](" + id.ReSizedFileName + ")");
        }
        public void WriteDirectory(System.IO.TextWriter textWriter, DirectoryDescriptor dd)
        {
            textWriter.Write("[" + dd.DirectoryName + "](" + dd.fullPath + ")");
        }
    }

    [Serializable()]
    public class ImageDescriptor
    {

        [JsonIgnore]
        public FileInfo ThumbNailFile { get => thumbNailFile; set => thumbNailFile = value; }
        [JsonIgnore]
        public FileInfo ImageFile { get => file; }
        [JsonIgnore]
        public FileInfo ReSizedFileInfo { get => reSizedFileInfo; set => reSizedFileInfo = value; }
        public string Name { get => name; set => name = value; }
        public string ThumbnailName { get => thumbnail_name; set => thumbnail_name = value; }
        public string ReSizedFileName { get => reSizedFileName; set => reSizedFileName = value; }
        public string Folder { get => folder; set => folder = value; }
        public int ImageHeight { get => height; set => height = value; }
        public int ImageWidth { get => width; set => width = value; }

        [JsonPropertyName("OriginalFilePath")]
        public string JSON_ImageFIle { get => ImageFile.FullName;}
        [JsonPropertyName("ThumbnailFilePath")]
        public string JSON_ThumbnailFile { get => thumbNailFile.FullName;}
        [JsonPropertyName("ResizedFilePath")]
        public string JSON_ResizedImage { get => ReSizedFileInfo.FullName;}

        private string thumbnail_name;
        private string name;
        private string folder;
        private int height;
        private int width;
        private string reSizedFileName;
        protected FileInfo reSizedFileInfo;      
        protected FileInfo file;  
        protected System.IO.FileInfo thumbNailFile;

        protected DirectoryInfo directory;
        public ImageDescriptor(System.IO.DirectoryInfo di, System.IO.FileInfo fi)
        {
            directory = di;
            file = fi;
            name = fi.Name;
            folder = di.FullName;
        }

        public void FillBasicInfo()
        {
            thumbnail_name = directory.Name + "_" + file.Name;
            
            {
                var inStream = file.OpenRead();
                using (Image image = Image.Load(inStream))
                {
                    height = image.Height;
                    width = image.Width;
                }
                inStream.Close();
            }
        }
    }
}