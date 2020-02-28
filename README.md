# CSharp-Image-Action
Working on an Action to resize images, make thumbnails, etc.. using .net core 3.1.100

[![Build .NET Core App](https://github.com/pauliver/CSharp-Image-Action/workflows/Build%20.NET%20Core%20App/badge.svg)](https://github.com/pauliver/CSharp-Image-Action/actions?query=workflow%3A%22Build+.NET+Core+App%22)


--------

Clone this [Template Repo](https://github.com/pauliver/Photo-Gallery-Template)


--------

```md
Example Usage of the .json file through Liquid and GitHub Markdown
 # Gallery
{% for gallery in site.data.galleryjson.PhotoGalleries %}
## [{{gallery.GalleryName}}]({{gallery.FullDirectoryPath}})
{% for images in gallery.Images %}
![{{images.ThumbnailName}}]({{images.ThumbnailFilePath}})
{% endfor %}
{% for subGalleries in gallery.PhotoGalleries %}
### [{{subGalleries.GalleryName}}]({{subGalleries.FullDirectoryPath}})
{% for subimages in subGalleries.Images %}
![{{subimages.ThumbnailName}}]({{subimages.ThumbnailFilePath}})
{% endfor %}     
{% for ThreesubGalleries in subGalleries.PhotoGalleries %}     
#### [{{ThreesubGalleries.GalleryName}}]({{ThreesubGalleries.FullDirectoryPath}})     
{% for Threesubimages in ThreesubGalleries.Images %}
![{{Threesubimages.ThumbnailName}}]({{Threesubimages.ThumbnailFilePath}})
{% endfor %}
{% endfor %}
{% endfor %}
{% endfor %}
```

Check out the lastest build script it uses: from [Here](http://github.com/pauliver/Photo-Gallery-Template/blob/master/.github/workflows/main.yml)
```yml
name: Create Thumbnails, Compressed images, Build Jekyll Site, Branch to Release on Success
env:
  URL: "pauliver.github.io/Photo-Gallery-Template/"
  AutoMergeLabel: "automerge"
  GHPages: "gh-pages"
  CurrentBranch: "master"
  Repo: "Photo-Gallery-Template"
  Owner: "pauliver"
  

on:
  push:
    paths-ignore:
      - '_data/galleryjson.json'
      - '_includes/gallery.json'
    branches:   
      - master
# page_build: # Pretty sure this was triggering endlessly
  pull_request:
    tags-ignore:
      - automerge
    paths:
      - '**.jpg'
      - '**.jpeg'
      - '**.png'
      - '**.webp'

jobs:
  process_images:
    name: Process Images with DotNet ${{ matrix.dotnet }}
    runs-on: [windows-latest]
    strategy:
      matrix:
        dotnet: [ '3.1.100' ]
    steps:
      - name: Checkout
        uses: actions/checkout@v2
        with:
          path: main 
      - name: Checkout Image Tools
        uses: actions/checkout@v2
        with:
          repository: pauliver/CSharp-Image-Action
          path: ImgTools
        
      - name: Setup dotnet
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: ${{ matrix.dotnet }}
     
      - name: Restore Dependancies
        run: dotnet restore ImgTools/

      - name: Build Image Tools
        run: dotnet build ImgTools/ --configuration Release
      
      - name: Run the Image Tools
        run: dotnet  ${{github.workspace}}\ImgTools\bin\Release\netcoreapp3.1\CSharp-Image-Action.dll  ${{github.workspace}}\main\gallery\ ${{github.workspace}}\main\ https://${{env.URL}} 

      - name: Commit the resized files and .json
        run: |
          git config --local user.email "actions@users.noreply.github.com"
          git config --local user.name "GitHub Action"
          cp ./_data/galleryjson.json ./_includes/gallery.json
          git add *
          git commit -m "Add resized images" -a
        working-directory: main 
        shell: powershell
      - name: Push changes
        uses: ad-m/github-push-action@master
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          directory: main   
      
      - name: Build the Pull Request (hopefully)
        run: dotnet  ${{github.workspace}}\ImgTools\bin\Release\netcoreapp3.1\CSharp-Image-Action.dll  ${{github.workspace}}\main\gallery\ ${{github.workspace}}\main\ https://${{env.URL}} True ${{env.Repo}} ${{env.Owner}} ${{env.CurrentBranch}} ${{env.GHPages}} ${{env.AutoMergeLabel}}
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          
  build_jekyll_site:
    name: Build the site in the jekyll/builder container
    needs: process_images
    runs-on: [ubuntu-latest]
    strategy:
      matrix:
        dotnet: [ '3.1.100' ]
    steps:
      - name: Checkout
        uses: actions/checkout@v2
      - run: |
          docker run \
          -v ${{ github.workspace }}:/srv/jekyll -v ${{ github.workspace }}/_site:/srv/jekyll/_site \
          jekyll/builder:latest /bin/bash -c "chmod 777 /srv/jekyll && jekyll build --future"
 
  merge_pr_if_sucess:
    name: Merge the PR we created
    needs: [process_images, build_jekyll_site]
    runs-on: [windows-latest]
    strategy:
      matrix:
        dotnet: [ '3.1.100' ]
    steps:
      - name: Setup dotnet
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: ${{ matrix.dotnet }}
      - name: Checkout Merge Tool
        uses: actions/checkout@v2
        with:
          repository: pauliver/Merge-Pull-Request-Csharp
          path: MergeTool

      - name: Build merge tool Tools
        run: dotnet build MergeTool/ --configuration Release
      
      - name: Run the MergeTool Tools
        run: dotnet  ${{github.workspace}}\MergeTool\bin\Release\netcoreapp3.1\Merge-Pull-Request.dll ${{env.Owner}} ${{env.Repo}} ${{env.AutoMergeLabel}}
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```
