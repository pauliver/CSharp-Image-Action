using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace CSharp_Image_Action
{
    public class ImageDescriptor
    {
        private System.IO.FileInfo thumbNailFile;
        public string ThumbnailName { get => thumbnail_name; set => thumbnail_name = value; }
        public string Name { get => name; set => name = value; }
        public string Folder { get => folder; set => folder = value; }
        public int ImageHeight { get => height; set => height = value; }
        public int ImageWidth { get => width; set => width = value; }
        public FileInfo ThumbNailFile { get => thumbNailFile; set => thumbNailFile = value; }
        public FileInfo ImageFile { get => file; }
        private string thumbnail_name;
        private string name;
        private string folder;
        private int height;
        private int width;

        protected DirectoryInfo directory;
        protected FileInfo file;
        public ImageDescriptor(System.IO.DirectoryInfo di, System.IO.FileInfo fi)
        {
            directory = di;
            file = fi;
            name = fi.Name;
            folder = di.FullName;
        }

        public void FillBasicInfo()
        {
            thumbnail_name = directory.Name + "_" + file.Name + "_" + file.CreationTimeUtc.ToString();
            
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