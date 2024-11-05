[<AutoOpen>]
module Sample.Extensions

open Avalonia.Controls
open AsyncImageLoader
open Avalonia.Data



type Image with

  member this.asyncSource
    (binding: IBinding, ?mode: BindingMode, ?priority: BindingPriority)
    =
    let mode = defaultArg mode BindingMode.TwoWay
    let priority = defaultArg priority BindingPriority.LocalValue

    let descriptor =
      ImageLoader.SourceProperty.Bind().WithMode(mode).WithPriority(priority)

    this[descriptor] <- binding
    this

  member this.isLoading
    (binding: IBinding, ?mode: BindingMode, ?priority: BindingPriority)
    =
    let mode = defaultArg mode BindingMode.OneWayToSource
    let priority = defaultArg priority BindingPriority.LocalValue

    let descriptor =
      ImageLoader.IsLoadingProperty.Bind().WithMode(mode).WithPriority(priority)

    this[descriptor] <- binding
    this
