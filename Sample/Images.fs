module Sample.Image

open System

open FsHttp
open SkiaSharp
open System.IO
open Avalonia.Media.Imaging

// TODO: Cache these things later by given URL
let getBitmapFromUri(url: Uri) = async {
  let! req = http { GET(url.ToString()) } |> Request.sendAsync

  let! res = Response.toBytesAsync req

  return
    new Bitmap(
      SKImage
        .FromBitmap(
          SKBitmap
            .Decode(new SKManagedStream(new MemoryStream(res), false))
            .Resize(new SKImageInfo(128, 128), SKFilterQuality.High)
        )
        .Encode(SKEncodedImageFormat.Jpeg, 100)
        .AsStream()
    )
}
