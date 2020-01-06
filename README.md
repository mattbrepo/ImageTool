# ImageTool
Command-line tool to compare, combine and view images. 

Available commands:
* -diff: compares two images or two directories containing images
* -fdiff: faster version of -diff
* -comb: places two images side by side
* -combdiff: places two images side by side only if they are different
* -view: shows in GUI an image and updates automatically if the image change

**Language: C#**

**Start: 2015**

## Why
I had two folders (_old_ and _new_) containing the same set of images. I needed to create an image every time a pair of images was different. The new image needed to be the combination of the pair. I couldn't achive that using [ImageMagick](https://imagemagick.org/index.php) so I decided to implement this tool.

## Example

Given 3 pairs in two folders:

.
+-- old
|   +-- a.jpg
|   +-- b.jpg
|   +-- c.jpg
+-- new
|   +-- a.jpg
|   +-- b.jpg
|   +-- c.jpg

where only _b.jpg_ differs.
Calling:

```
ImageTool -combdiff .\old .\new .
```

generates only one file:

![Example](/images/example.jpg)
