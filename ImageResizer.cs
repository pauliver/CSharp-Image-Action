using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace CSharp_Image_Action
{
    
    //https://devblogs.microsoft.com/dotnet/net-core-image-processing/
    public class ImageResizer
    {
        //https://github.com/SixLabors/ImageSharp

        public struct Box{
            public int Width;
            public int Height;

            public bool BiasForLongEdge;
            public bool PreserveRatio;

            public Box(int width,int height,bool biasforlongedge, bool preserveratio)
            {
                this.Width = width;
                this.Height = height;
                this.BiasForLongEdge = biasforlongedge;
                this.PreserveRatio = preserveratio;
            }
        }

        Box Thumbnail;
        Box StandardImage;

        public Box ThumbnailSizing { get => Thumbnail; }
        public Box StandardImageSizing { get => StandardImage;  }

        System.IO.DirectoryInfo ThumbNailDirectory;

        public ImageResizer(System.IO.DirectoryInfo thumbnaildirectory, int ThumbNail_Width = 256, int ThumbNail_Height = 256, int Main_Width = 1024, int Main_Height = 1024, bool BiasForLongEdge = true, bool preserve_ratio = true)
        {
            this.Thumbnail = new Box(ThumbNail_Width,ThumbNail_Height,BiasForLongEdge,preserve_ratio);
            this.StandardImage = new Box(Main_Width,Main_Height,BiasForLongEdge,preserve_ratio);
            this.ThumbNailDirectory = thumbnaildirectory;
        }

        /// Thumbnail doesn't exists, or needs updates
        public bool ThumbnailNeeded(ImageDescriptor id)
        {
            id.ThumbNailFile = new System.IO.FileInfo(ThumbNailDirectory.FullName + id.ThumbnailName);
            if(id.ThumbNailFile.Exists)
                return false;
            else
                return true;
        }

        /// Image needs to be resized
        public bool NeedsResize(ImageDescriptor id)
        {
            return true;
        }

        public void GenerateThumbnail(ImageDescriptor id)
        {
            var fs = id.ThumbNailFile.OpenWrite();
        }

        public void ResizeImages(ImageDescriptor id)
        {
            //https://docs.sixlabors.com/articles/ImageSharp/Resize.html
        }
        
        protected bool IsCorrectResolution(ImageDescriptor id, Box sizing )
        {
            return false;
        }

        protected Box ReSizeToBox(ImageDescriptor imageDescriptor, Box sizing, System.IO.FileStream outstream)
        {
            System.IO.FileInfo fi = imageDescriptor.ImageFile;
            return new Box();   
        }
    }
}