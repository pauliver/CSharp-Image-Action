using System.IO;

namespace CSharp_Image_Action
{
    public class ImageHunter
    {
        //some structure to hold images & directories
        // should it be a tree or just a list?

        int ImagesFound = 0;
        int MaxImages = 32768;

        int MaxDirectoryDepth = 4;

        string[] ValidImageExtensions;

        public int NumberImagesFound
        {get{ return ImagesFound;}}

        public ImageHunter(System.IO.DirectoryInfo directory, System.String[] Extensions, int Max_Images = 32768, int Directory_Depth = 4)
        {
            MaxDirectoryDepth = Directory_Depth;
            MaxImages = Max_Images;
            ValidImageExtensions = Extensions;
            RecurseDirectory(directory,0);
        }

        protected void MatchImages(System.IO.FileInfo fi)
        {
            ++ImagesFound;
            //@@ ADD TO IMAGE LIST HERE
        }
        protected void RecurseDirectory(System.IO.DirectoryInfo directory, int CurrentDepth)
        {
            if(ImagesFound > MaxImages)
            {
                //would be good to log an error
                return;
            }    

            if(CurrentDepth == MaxDirectoryDepth)
            {
                //would be good to log an error
                
            }else{
                // Get all the sub directories, and dive into them
                foreach(System.IO.DirectoryInfo di in directory.GetDirectories())
                {
                    RecurseDirectory(di, CurrentDepth + 1);
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
                    MatchImages(fi);
                }else{                
                    //would be good to log an "info" "filename" with "extension" invalid
                }
            }
        }

    }
}