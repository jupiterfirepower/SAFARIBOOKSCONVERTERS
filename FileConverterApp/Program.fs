// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Text
open System.Net
open FSharp.Data
open System.Text.RegularExpressions
open System.Drawing
 
let rec allFiles dirs =
    if Seq.isEmpty dirs then Seq.empty else
        seq { yield! dirs |> Seq.collect Directory.EnumerateFiles
              yield! dirs |> Seq.collect Directory.EnumerateDirectories |> allFiles }

type StringBuilder = System.Text.StringBuilder

let inline (|?) (a: int) b = if a <> -1 then a else b

let inline htmlDecode(text:string) = 
                WebUtility.HtmlDecode(text.Trim()).Replace("=\r\n", "").
                                        Replace("<br>", "\n").
                                        Replace("=20","").
                                        Replace("=3D>","=>").
                                        Replace("3D"," ").
                                        Replace("<strong>", "").
                                        Replace("</strong>", "").
                                        Replace("<=\nstrong>","").
                                        Replace("<em>","").
                                        Replace("</em>","").
                                        Replace("<pre>","").
                                        Replace("</pre>","").
                                        Replace("<span>","").
                                        Replace("</span>","").
                                        Replace("<p>","").
                                        Replace("</p>","").
                                        Replace("<kbd>","").
                                        Replace("</kbd>","").
                                        Replace("&gt;",">")

let inline htmlFix(text:string) = 
                WebUtility.HtmlDecode(text.Trim()).Replace("=\r\n", "")
                                        .Replace("=20","")
                                        .Replace(" =3D> "," =&gt; ")
                                        .Replace(" =3D "," = ")
                                        .Replace("=3D&gt;","=&gt;")
                                        .Replace(" :=3D "," := ")
                                        .Replace(" !=3D "," != ")
                                        .Replace(" >=3D "," >= ")
                                        .Replace(" <=3D "," <= ")
                                        .Replace(" +=3D "," += ")
                                        .Replace(" =3D=3D "," == ")
                                        .Replace("<U>","")
                                        .Replace("<S>","")
                                        .Replace("&#x2013;&#xA0;","—")
                                        .Replace("&#x2013;","—")
                                        .Replace("=E2=80=93","-")
                                        .Replace("=E2=80=94","—")

let inline htmlSpecFix(text:string) = 
                WebUtility.HtmlDecode(text.Trim()).Replace("=\r\n", "")
                                        .Replace("=20","")
                                        .Replace("3D","")
                                         
                                        
let inline getHtmlBeetwenTag(htmlText:string, tagName:string) = 
                let tagIndexBottom = htmlText.LastIndexOf("</"+tagName+">")
                match tagIndexBottom with
                | -1 -> String.Empty
                | x ->
                    let tagIndexTop = htmlText.LastIndexOf("<"+tagName+">", x)
                    if x <> -1 && tagIndexTop <> -1 then
                        htmlText.Substring(tagIndexTop + tagName.Length + 2, x - tagIndexTop - (tagName.Length + 2))
                    else
                        String.Empty

let inline getHtmlWithTag(htmlText:string, tagName:string) = 
                let tagIndexBottom = htmlText.LastIndexOf("</"+tagName+">")
                match tagIndexBottom with
                | -1 -> String.Empty
                | x ->
                    let tagIndexTop = htmlText.LastIndexOf("<"+tagName+">", x)
                    if x <> -1 && tagIndexTop <> -1 then
                        htmlText.Substring(tagIndexTop, x + (tagName.Length + 3) - tagIndexTop)
                    else
                        String.Empty

let inline getInnerTextByHtmlTag(htmlPart:string, tagName:string) =
                       let regex = new Regex("<"+tagName+"(.*?)>(.*?)</"+tagName+">")
                       let m = regex.Match(htmlPart)
                       m.Groups.[2].Value

let inline getFileNameFromUrl(url:string) = System.IO.Path.GetFileName(url.Trim [|'3';'D';'"'|]) 

let getImageSize(imgBase64:string) =
                                    try
                                        let imageData = Convert.FromBase64String(imgBase64)
                                        use ms = new MemoryStream(imageData)
                                        let img = System.Drawing.Image.FromStream(ms)
                                        (img.Width, img.Height)
                                    with | _ -> (0,0)

let getImageFromBase64(imgBase64:string) = let imageData = Convert.FromBase64String(imgBase64)
                                           use ms = new MemoryStream(imageData)
                                           System.Drawing.Image.FromStream(ms)

let inline IsBase64String(data:string) = let s = data.Trim()
                                         (s.Length % 4 = 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None)


let ResizeImage(image:System.Drawing.Image, height:int, width:int) =
                                let destRect = new Rectangle(0, 0, width, height)
                                let destImage = new Bitmap(width, height)
                                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution)
                                use graphics = Graphics.FromImage(destImage)
                                graphics.CompositingMode <- System.Drawing.Drawing2D.CompositingMode.SourceCopy
                                graphics.CompositingQuality <- System.Drawing.Drawing2D.CompositingQuality.HighQuality
                                graphics.InterpolationMode <- System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
                                graphics.SmoothingMode <- System.Drawing.Drawing2D.SmoothingMode.HighQuality
                                graphics.PixelOffsetMode <- System.Drawing.Drawing2D.PixelOffsetMode.HighQuality
                                use wrapMode = new System.Drawing.Imaging.ImageAttributes()
                                wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY)
                                graphics.DrawImage(image, destRect, 0, 0, image.Width,image.Height, GraphicsUnit.Pixel, wrapMode)
                                destImage

let ImageToBase64(image:Bitmap, format:System.Drawing.Imaging.ImageFormat) =
                                  use ms = new MemoryStream()
                                  // Convert Image to byte[]
                                  image.Save(ms, format)
                                  let imageBytes = ms.ToArray()
                                  // Convert byte[] to Base64 String
                                  Convert.ToBase64String(imageBytes)

let Base64ToImage(base64String:string) = let imageBytes = Convert.FromBase64String(base64String) // Convert Base64 String to byte[]              
                                         use ms = new MemoryStream(imageBytes, 0, imageBytes.Length)
                                         // Convert byte[] to Image
                                         ms.Write(imageBytes, 0, imageBytes.Length)
                                         Image.FromStream(ms, true)

let getImageFormatByExt = function
                          | "png" -> System.Drawing.Imaging.ImageFormat.Png
                          | "jpeg" -> System.Drawing.Imaging.ImageFormat.Jpeg
                          | "gif" -> System.Drawing.Imaging.ImageFormat.Gif
                          | "tiff" -> System.Drawing.Imaging.ImageFormat.Tiff
                          | "wmf" -> System.Drawing.Imaging.ImageFormat.Wmf
                          | "emf" -> System.Drawing.Imaging.ImageFormat.Emf
                          | "exif" -> System.Drawing.Imaging.ImageFormat.Exif
                          | "bmp" -> System.Drawing.Imaging.ImageFormat.Bmp
                          | _ -> System.Drawing.Imaging.ImageFormat.Png

let Replace3D(article:string) = let add = ref 0
                                let mutable endi:int = 0
                                let mutable articleResult = article
                                while articleResult.Length > endi + !add && articleResult.IndexOf("</pre>", endi + !add) <> -1 do
                                      let starti = articleResult.IndexOf("<pre>", endi + !add)
                                      endi <- articleResult.IndexOf("</pre>", endi + !add)
                                      if starti > -1 && articleResult.Length > starti && endi-(starti+5) > 0  then
                                         let pretext = articleResult.Substring(starti, endi-(starti))
                                         let resultNo3D = pretext.Replace("3D", "")
                                         articleResult <- articleResult.Replace(pretext, resultNo3D)
                                      add := 7
                                articleResult

[<EntryPoint>]
let main argv =
    printfn "Start parsing safaribooks!"
    let count = ref 0
    //if (Array.length argv) = 0 then
    //  printfn "usage [] - optional parameter(default 1000) : dotnet FileCoverterApp.dll directory_path_to_book [imageWidthInPixels]"
    //  Environment.Exit(-1)

    let imageWidth = match (Array.length argv) with   
                        | 2 -> argv.[1] |> int
                        | _ -> 1000

    let builderFirst = StringBuilder()
    let builderHtml = StringBuilder()
    

    builderHtml.Append("<html><body>") |> ignore

    let files = allFiles (seq { yield """C:\PROJECT\Converters\FileConverterApp\FileConverterApp\bin\Debug\netcoreapp2.1\network_programming_with_rust"""  }) //argv.[0] implementing_azure_solutions___second_edition
    let result = Seq.sortBy (fun (x : string) -> FileInfo(x).CreationTime) files
    printfn "%A" result
    for filePath in files do
        printfn "File parsed %s" (System.IO.Path.GetFileName(filePath))
        count := !count + 1
        let readText = File.ReadAllText(filePath)
        let header = (getHtmlBeetwenTag(readText, "header").Trim [|'\r';'\n';' '|]).Replace("=\r\n", String.Empty)
        let headerWithTag = getHtmlWithTag(readText, "header").Trim [|'\r';'\n';' '|]
        let article = getHtmlBeetwenTag(readText, "article").Trim [|'\r';'\n';' ';'='|]
        let articleWithTag = getHtmlWithTag(readText, "article").Trim [|'\r';'\n';' ';'=';'2';'0'|]

        let headerText = getInnerTextByHtmlTag(header, "h1")

        let mutable imageBase64:string = String.Empty

        if articleWithTag <> String.Empty then
            let articleDoc = HtmlDocument.Parse(articleWithTag.Replace("=\r\n", String.Empty))

            let mutable writed = false

            articleDoc.Descendants ["pre"] |> Seq.map (fun x -> x.InnerText()) 
            |> Seq.iter (fun x -> if headerText.Length > 0 && not writed then 
                                     builderFirst.Append(headerText.Trim()+"\n\n") |> ignore
                                     writed <- true
                                  let outText = htmlDecode(WebUtility.HtmlDecode(x))
                                  builderFirst.Append(outText.Trim()) |> ignore
                                  builderFirst.Append("\n\n") |> ignore )

            builderHtml.Append(headerWithTag.Replace("=\r\n","")+"<br>") |> ignore

            articleDoc.Descendants ["img"] |> Seq.map (fun x -> getFileNameFromUrl(x.AttributeValue "src")) 
            |> Seq.iter (fun x -> printfn "Image File Name: %s" x
                                  let startImage = readText.LastIndexOf(x)
                                  let endImage = readText.IndexOf("------MultipartBoundary", startImage) 
                                  let ext = Path.GetExtension(x).Trim [|'.'|]
                                  if endImage <> -1 then
                                    imageBase64 <- (readText.Substring(startImage+x.Length, endImage-(startImage+x.Length)).Trim())
                                    let (h,w) = getImageSize(imageBase64)
                                    let (hs,ws) = match (h,w) with
                                                    | (hx, wx) when wx > imageWidth ->  //int width = img.Width * (percent * .01);
                                                                                        //int height = img.Height * (percent * .01);
                                                                                        (int (Math.Round((float (float hx) * (float imageWidth/(float wx))), 0)), int (Math.Round((float (float wx) * (float imageWidth/(float wx))), 0)) )
                                                    | (height, width) -> (height, width)

                                    if w > imageWidth then 
                                        let imageCurrent = getImageFromBase64(imageBase64) 
                                        let bitmapResized = ResizeImage(imageCurrent, hs, ws)
                                        imageBase64 <- ImageToBase64(bitmapResized, getImageFormatByExt(ext.ToLower()))
                                        printfn "Image Height : %d  %d" h hs
                                        printfn "Image Width : %d %d" w ws

                                    let insImage = "<img src=\"data:image/"+ext+";base64,"+imageBase64+"\" alt=\""+x+"\" scale=\"0\">"
                                    let startImageIndex = articleWithTag.IndexOf("<img")
                                    let endImageIndex = articleWithTag.IndexOf(">", startImageIndex + 4) 
                                    let imgOrig=articleWithTag.Substring(startImageIndex, endImageIndex+1-startImageIndex).Trim()

                                    let articleWithTagResult = articleWithTag.Replace(imgOrig, "{{{1}}}")
                                    let fixedHtmlResult = htmlFix(articleWithTagResult)
                                    let replaced3dHtmlResult = Replace3D(fixedHtmlResult)
                                    let articleWithTagHtmlResult = replaced3dHtmlResult.Replace("{{{1}}}", insImage)

                                    
                                    //let fixedHtmlResult = htmlSpecFix(articleWithTagResult)
                                    //let articleWithTagHtmlResult = articleWithTagResult.Replace("{{{1}}}", insImage)

                                    builderHtml.Append(articleWithTagHtmlResult) |> ignore )
                                    //builderHtml.Append(articleWithTagResult) |> ignore )
                                  
                                  
            if Seq.length (articleDoc.Descendants ["img"]) = 0 then
               let fixedHtmlResult = htmlFix(articleWithTag)
               let replaced3dHtmlResult = Replace3D(fixedHtmlResult)
               builderHtml.Append(replaced3dHtmlResult) |> ignore
               //builderHtml.Append(articleWithTag) |> ignore

    
    builderHtml.Append("</body></html>") |> ignore                       

    File.WriteAllText("resultbook.html", builderHtml.ToString().Trim(), Encoding.UTF8)
    File.WriteAllText("shortbookcode.txt", builderFirst.ToString().Trim(), Encoding.UTF8)

    printfn "%d files Parsing finished." !count 
    Console.ReadLine() |> ignore
    0 // return an integer exit code
