[<AutoOpen>]
module Extensions

open Avalonia.Controls
open AsyncImageLoader
open Avalonia.Data


type Image with
  member this.asyncSource(src: string) =
    this[ImageLoader.SourceProperty] <- src
    this

  member this.asyncSource(src: IBinding, ?mode, ?priority) =
    let mode = defaultArg mode BindingMode.Default
    let priority = defaultArg priority BindingPriority.LocalValue

    let descriptor =
      ImageLoader.SourceProperty.Bind().WithMode(mode).WithPriority(priority)

    this[descriptor] <- src
    this


  member this.isLoading(value: bool) =
    this[ImageLoader.IsLoadingProperty] <- value
    this

  member this.isLoading(value: IBinding, ?mode, ?priority) =
    let mode = defaultArg mode BindingMode.Default
    let priority = defaultArg priority BindingPriority.LocalValue

    let descriptor =
      ImageLoader.IsLoadingProperty.Bind().WithMode(mode).WithPriority(priority)

    this[descriptor] <- value
    this
