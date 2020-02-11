using System.Collections;
using System.IO;

namespace CSharp_Image_Action
{
    public class ImageHunter
    {
        public int NumberImagesFound
        {get{ return ImagesFound;}}

        public ArrayList ImageList { get => imageList;}

        protected System.Collections.ArrayList imageList = new System.Collections.ArrayList();

        protected int ImagesFound = 0;
        protected int MaxImages = 32768;

        protected int MaxDirectoryDepth = 4;

        protected string[] ValidImageExtensions;

        public ImageHunter(ref DirectoryDescriptor dd, System.IO.DirectoryInfo directory, System.String[] Extensions, int Max_Images = 32768, int Directory_Depth = 4)
        {
            MaxDirectoryDepth = Directory_Depth;
            MaxImages = Max_Images;
            ValidImageExtensions = Extensions;
            RecurseDirectory(ref dd, directory,0);
        }

        protected void MatchImages(ref DirectoryDescriptor dd, System.IO.DirectoryInfo directory, System.IO.FileInfo fi)
        {
            ++ImagesFound;
            var p = new ImageDescriptor(directory,fi);
            imageList.Add(p);
            dd.Images.Add(p);
        }
        protected void RecurseDirectory(ref DirectoryDescriptor dd, System.IO.DirectoryInfo directory, int CurrentDepth)
        {
            if(ImagesFound > MaxImages)
            {
                System.Console.WriteLine("//would be good to log an error");
                return;
            }    

            if(CurrentDepth == MaxDirectoryDepth)
            {
                System.Console.WriteLine("//would be good to log an error");
            }else{
                // Get all the sub directories, and dive into them
                foreach(System.IO.DirectoryInfo di in directory.GetDirectories())
                {
                    DirectoryDescriptor dd2 = new DirectoryDescriptor(di.Name,di.FullName);
                    dd.Directories.Add(dd2);
                    RecurseDirectory(ref dd2,di, CurrentDepth + 1);
                }
            }
            foreach(System.IO.FileInfo fi in directory.GetFiles())
            {
                //optimize this later, it's slow and ugly
                bool ValidExtension = false;
                // Get all the images if a valid extension
                foreach(string s in ValidImageExtensions)
                {
                    if(fi.Extension == s)
                        ValidExtension = true;
                }

                if(ValidExtension)
                {
                    MatchImages(ref dd, directory,fi);
                }else{
                    System.Console.WriteLine("Info: " + fi.Name  + " with " + fi.Extension + " invalid");              
                }
            }
        }

    }
}