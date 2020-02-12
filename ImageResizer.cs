using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;

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

            public Box(int width,int height, bool preserveratio = true,bool biasforlongedge = true)
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

        public ImageResizer(System.IO.DirectoryInfo thumbnaildirectory, int ThumbNail_Width = 256, int ThumbNail_Height = 256, int Main_Width = 1024, int Main_Height = 1024, bool preserve_ratio = true, bool BiasForLongEdge = true)
        {
            this.Thumbnail = new Box(ThumbNail_Width,ThumbNail_Height,preserve_ratio,BiasForLongEdge);
            this.StandardImage = new Box(Main_Width,Main_Height,preserve_ratio,BiasForLongEdge);
            this.ThumbNailDirectory = thumbnaildirectory;
        }

        /// Thumbnail doesn't exists (we currently don't handle a thumbnail of the wrong size)
        public bool ThumbnailNeeded(ImageDescriptor id)
        {
            id.ThumbNailFile = new System.IO.FileInfo(ThumbNailDirectory.FullName + "\\" + id.ThumbnailName);
            if(id.ThumbNailFile.Exists)
                return false;
            else
                return true;
        }

        /// Image needs to be resized
        public bool NeedsResize(ImageDescriptor id)
        {
            id.ReSizedFileInfo = new System.IO.FileInfo(id.ReSizedFileName);
            if(id.ReSizedFileInfo.Exists)
                return false;
            else
                return true;
        }

        public void GenerateThumbnail(ImageDescriptor id)
        {
            var fs = id.ThumbNailFile.OpenWrite();
            var success = ReSizeToBox(id,Thumbnail,fs);
            if(success)  
                System.Console.WriteLine("Thumbnail Generated");
            else
                System.Console.WriteLine("Thumbnail generation failed");
        }

        public void ResizeImages(ImageDescriptor id)
        {
            var fs = id.ReSizedFileInfo.OpenWrite();

            var success = ReSizeToBox(id,StandardImage,fs);
            if(success)  
                System.Console.WriteLine("Image Resized");
            else
                System.Console.WriteLine("Image Resize Failed");
        }
        
        protected bool IsCorrectResolution(ImageDescriptor id, Box sizing )
        {
            var inStream = id.ImageFile.OpenRead();
            bool retval = false;
            using (Image image = Image.Load(inStream))
            {
                int height = image.Height;
                int width = image.Width;

                if(height == sizing.Height && width == sizing.Width)
                {
                    retval = true;
                }else if(height > width && sizing.Height == height && sizing.BiasForLongEdge && sizing.PreserveRatio)
                {
                    retval = true;
                }else if(height < width && sizing.Width == width && sizing.BiasForLongEdge && sizing.PreserveRatio)
                {
                    retval = true;
                }else{
                    System.Console.WriteLine("that else you never wrote, yeah.. you hit it");
                    //didn't finsih this logic
                }
            }
            inStream.Close();
            return retval;
        }

        protected bool ReSizeToBox(ImageDescriptor imageDescriptor, Box sizing, System.IO.FileStream outstream)
        {
            System.IO.FileInfo fi = imageDescriptor.ImageFile;
            {
                //https://docs.sixlabors.com/articles/ImageSharp/Resize.html
                var inStream = fi.OpenRead();
                using (Image image = Image.Load(inStream))
                {
                    int height = image.Height;
                    int width = image.Width;

                    double HeightWidthRatio = (double) height / (double) width;
                    double targetratio = (double) sizing.Height / (double) sizing.Width;

                    if(sizing.PreserveRatio && sizing.BiasForLongEdge )
                    {
                        if( HeightWidthRatio >= 1.0f ) // Height is greater than or equal to Width
                        {
                            image.Mutate(x => x.Resize( 0, sizing.Height, KnownResamplers.Lanczos3));
                        }else if(HeightWidthRatio < 1.0f){ // Width is greater than height
                            image.Mutate(x => x.Resize( sizing.Width, 0, KnownResamplers.Lanczos3));
                        }

                    }else if(sizing.PreserveRatio){                    
                        
                        if( targetratio >= 1.0f ) // Target Height is greater than or equal to Width
                        {
                            image.Mutate(x => x.Resize( 0, sizing.Height, KnownResamplers.Lanczos3));
                        }else if(targetratio < 1.0f){ // Target Width is greater than height
                            image.Mutate(x => x.Resize( sizing.Width, 0, KnownResamplers.Lanczos3));
                        }

                    }else{
                        image.Mutate(x => x.Resize(sizing.Width,sizing.Height, KnownResamplers.Lanczos3));
                    }
                    image.Save(outstream, new PngEncoder());
                    return true;
                }
            }
        }
    }
}