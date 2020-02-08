using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace CSharp_Image_Action
{
    
    //https://devblogs.microsoft.com/dotnet/net-core-image-processing/
    public class ImageResizer
    {
        //https://github.com/SixLabors/ImageSharp

        public ImageResizer(int ThumbNail_Width = 256, int ThumbNail_Height = 256, int Main_Width = 1024, int Main_Height = 1024, bool BiasForLongEdge = true, bool preserve_ratio = true)
        {

        }

        /// Thumbnail doesn't exists, or needs updates
        public bool ThumbnailNeeded(ImageDescriptor id)
        {
            return true;
        }

        /// Image needs to be resized
        public bool NeedsResize(ImageDescriptor id)
        {
            return true;
        }

        public void GenerateThumbnail(ImageDescriptor id)
        {

        }

        public void ResizeImages(ImageDescriptor id)
        {

        }

    }
}