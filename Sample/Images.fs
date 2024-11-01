module Sample.Image

open System

open System.Collections.Concurrent
open Flurl.Http
open SkiaSharp
open System.IO
open Avalonia.Media.Imaging

let cache = ConcurrentDictionary<string, Bitmap>()

let getBitmapFromUri(url: Uri) = task {

  match cache.TryGetValue(url.ToString()) with
  | true, bitmap -> return bitmap
  | _ ->

    let! res = url.GetBytesAsync()

    let bitmap =
      new Bitmap(
        SKImage
          .FromBitmap(
            SKBitmap
              .Decode(new SKManagedStream(new MemoryStream(res), false))
              .Resize(SKImageInfo(128, 128), SKFilterQuality.High)
          )
          .Encode(SKEncodedImageFormat.Jpeg, 100)
          .AsStream()
      )

    cache.TryAdd(url.ToString(), bitmap) |> ignore
    return bitmap

}
