using System;

namespace CSharp_Image_Action
{
    class Program
    {
        static void Main(string[] args)
        {
            var ImgDir = args[0];
            System.IO.DirectoryInfo di;
            if (ImgDir is string)
            {
                di = new System.IO.DirectoryInfo(ImgDir);
            }
            else if (ImgDir as String != null)
            {
                di = new System.IO.DirectoryInfo(ImgDir as string);
            }
            else
            {
                Console.WriteLine("First Arg must be a directory");
                return;
            }

            if (di.Exists == false) // VSCode keeps offering to "fix this" for me... 
            {
                Console.WriteLine("Directory [" + ImgDir + "] Doesn't exist");
                return;
            }

            // Traverse Image Directory
            // Build tree 
        }
    }
}
