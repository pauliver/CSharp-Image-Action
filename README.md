# CSharp-Image-Action
Working on an Action to resize images, make thumbnails, etc.. using .net core 3.1.100

[![.NET Core](https://github.com/pauliver/CSharp-Image-Action/workflows/.NET%20Core/badge.svg)](https://github.com/pauliver/CSharp-Image-Action/actions?query=workflow%3A%22.NET+Core%22)

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
