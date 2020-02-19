# CSharp-Image-Action
Working on an Action to resize images, make thumbnails, etc.. using .net core 3.1.100

[![Build .NET Core App](https://github.com/pauliver/CSharp-Image-Action/workflows/Build%20.NET%20Core%20App/badge.svg)](https://github.com/pauliver/CSharp-Image-Action/actions?query=workflow%3A%22Build+.NET+Core+App%22)


--------

Clone this [Template Repo](https://github.com/pauliver/Photo-Gallery-Template)

**below here is outdated info**
--------

*OutDated as of 2/22/20 - need to protect branch, when they match the script nukes it.. not helpful*

[This folder](https://github.com/pauliver/CSharp-Image-Action/tree/master/SampleWebsite) has a complete copy of everything you need to use this elsewhere 


#### Example Usage of the .json file through Liquid and GitHub Markdown

```markdown

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

#### Example .yml file to build it

```yml
name: Create Thumbnails, Compressed images, Build Jekyll Site, Branch to Release on Success
env:
  URL: "example.com"
  AutoMergeLabel: "automerge"
  GHPages: "gh-pages"
  CurrentBranch: "master"
  Repo: "CSharp-Image-Action"
  Owner: "pauliver"
  # https://help.github.com/en/actions/configuring-and-managing-workflows/using-environment-variables#default-environment-variables

on:
  push:
    branches: [master]
# page_build: # Pretty sure this was triggering endlessly
  pull_request:
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
          git add *
          git commit -m "Add resized images" -a
        working-directory: main 
        shell: powershell
      - name: Push changes
        uses: ad-m/github-push-action@master
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          directory: main   
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
  create_pr_if_success:
    name: Create PR to gh-pages if everything works
    needs: [process_images, build_jekyll_site]
    runs-on: [ubuntu-latest]
    strategy:
      matrix:
        dotnet: [ '3.1.100' ]
    steps:
      - name: Checkout
        uses: actions/checkout@v2
        with:
          path: main
      - name: Create Pull Request
        uses: peter-evans/create-pull-request@v2.4.1 #https://github.com/marketplace/actions/create-pull-request
        with:
          path: main
          base: ${{env.CurrentBranch}}
          branch: ${{env.GHPages}}
          token: ${{ secrets.GITHUB_TOKEN }}
          commit-message: "tests all passed, creating PR for ${{env.GHPages}}"
          labels: ${{env.AutoMergeLabel}}
  merge_pr_if_sucess:
    name: Merge the PR we created
    needs: [create_pr_if_success]
    runs-on: [ubuntu-latest]
    strategy:
      matrix:
        dotnet: [ '3.1.100' ]
    steps:
      - name: automerge
        uses: "pascalgn/automerge-action@ecb16453ce68e85b1e23596c8caa7e7499698a84"
        env:
          GITHUB_TOKEN: "${{ secrets.GITHUB_TOKEN }}"
          MERGE_LABELS: ${{env.AutoMergeLabel}},"!work in progress"
          MERGE_REMOVE_LABELS: ${{env.AutoMergeLabel}}
          MERGE_DELETE_BRANCH: false	
          UPDATE_METHOD: "merge"
```
