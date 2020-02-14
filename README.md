# CSharp-Image-Action
Working on an Action to resize images, make thumbnails, etc.. using .net core 3.1.100

[![Build .NET Core App](https://github.com/pauliver/CSharp-Image-Action/workflows/Build%20.NET%20Core%20App/badge.svg)](https://github.com/pauliver/CSharp-Image-Action/actions?query=workflow%3A%22Build+.NET+Core+App%22)

--------

*and now how you use it in your own repo*

#### Example Usage of the .json file 

*this output with works with Jekyll / github pages, to generate a programatic landing page (for hosting on GitHub Pages)*
> 
>  # Gallery
> {% for gallery in site.data.galleryjson.PhotoGalleries %}
> ## [{{gallery.GalleryName}}]({{gallery.FullDirectoryPath}})
> {% for images in gallery.Images %}
> ![{{images.ThumbnailName}}]({{images.ThumbnailFilePath}})
> {% endfor %}
> {% for subGalleries in gallery.PhotoGalleries %}
> ### [{{subGalleries.GalleryName}}]({{subGalleries.FullDirectoryPath}})
> {% for subimages in subGalleries.Images %}
> ![{{subimages.ThumbnailName}}]({{subimages.ThumbnailFilePath}})
> {% endfor %}     
> {% for ThreesubGalleries in subGalleries.PhotoGalleries %}     
> #### [{{ThreesubGalleries.GalleryName}}]({{ThreesubGalleries.FullDirectoryPath}})     
> {% for Threesubimages in ThreesubGalleries.Images %}
> ![{{Threesubimages.ThumbnailName}}]({{Threesubimages.ThumbnailFilePath}})
> {% endfor %}
> {% endfor %}
> {% endfor %}
> {% endfor %}


#### Example .yml file to build it

*no idea how i'm suposed to embed a copy/pasteable .yml file using markdown...*

> name: Create Thumbnails and compressed images
>
> on:
>   push:
>     branches: master
>
>   pull_request:
>     paths:
>       - '\*\*.jpg'
>       - '\*\*.png'
>       - '\*\*.jpeg'
>       - '\*\*.webp'
> 
> jobs:
>   build:
>     runs-on: windows-latest
>     strategy:
>       matrix:
>         dotnet: [ '3.1.100' ]
>     name: Dotnet ${{ matrix.dotnet }} ImageCompression
>     steps:
>       - name: Checkout
>         uses: actions/checkout@v2
>         with:
>           path: main 
>       - name: Checkout Image Tools
>         uses: actions/checkout@v2
>         with:
>           repository: pauliver/CSharp-Image-Action
>           path: ImgTools
>         
>       - name: Setup dotnet
>         uses: actions/setup-dotnet@v1
>         with:
>           dotnet-version: ${{ matrix.dotnet }}
>      
>       - name: Restore Dependancies
>         run: dotnet restore ImgTools/
> 
>       - name: Build Image Tools
>         run: dotnet build ImgTools/ --configuration Release
>       
>       - name: Run the Image Tools
>         run: dotnet <hardcode path to compiled ImagTools repo>  <hardcode path main repo>\<folder with images>\  <hardcode path main repo> <domain>
> 
>       - name: Commit files
>         run: |
>           cd <hardcode path main repo>
>           git config --local user.email "email@email.com"
>           git config --local user.name "GitHub Action"
>           git add *
>           git commit -m "Add resized images" -a
>           git push
>           cd <hardcode path main repo>
