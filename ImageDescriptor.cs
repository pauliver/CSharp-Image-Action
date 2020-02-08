using System.IO;

namespace CSharp_Image_Action
{
    public class ImageDescriptor
    {
        public string ThumbnailPath { get => thumbnail_path; set => thumbnail_path = value; }
        public string Name { get => name; set => name = value; }
        public string Folder { get => folder; set => folder = value; }
        public string ImageHeight { get => height; set => height = value; }
        public string ImageWidth { get => width; set => width = value; }

        private string thumbnail_path;
        private string name;
        private string folder;
        private string height;
        private string width;

        protected DirectoryInfo directory;
        protected FileInfo file;
        public ImageDescriptor(System.IO.DirectoryInfo di, System.IO.FileInfo fi)
        {
            directory = di;
            file = fi;
            name = fi.Name;
            folder = di.FullName;
        }
    }
}