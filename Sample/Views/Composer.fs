namespace Sample.Views

open System
open Avalonia.Controls
open Avalonia.Controls.Templates

open NXUI
open NXUI.FSharp.Extensions
open FsToolkit.ErrorHandling

open FSharp.Data.Adaptive

open Threads.Lib
open Navs.Avalonia
open Sample


[<AutoOpen>]
module private ComposerStyles =

  type DockPanel with

    member this.StyleAsComposer() =
      this.HorizontalAlignmentCenter().margin(4, 8)


  type TextBox with

    member this.StyleAsComposerTextBox() =
      this
        .HorizontalAlignmentStretch()
        .VerticalAlignmentCenter()
        .TextWrappingWrap()
        .acceptsReturn(true)
        .minHeight(124)
        .maxHeight(320)
        .width(320)
        .margin(0, 0, 0, 8)

  type TextBlock with

    member this.StyleAsComposerCounter() = this.margin(0, 0, 8, 0)


  type Button with

    member this.StyleAsComposerButton() =
      this
        .HorizontalContentAlignmentCenter()
        .VerticalContentAlignmentCenter()
        .height(46)
        .width(64)

type private AdaptiveParams = {
  audience: cval<Posts.ReplyAudience>
  mediaType: cval<Posts.MediaType>
  mediaUrl: cval<Uri option>
  text: cval<string>
}

module private AdaptiveParams =
  let toPostParams adaptiveParams : PostParameters = {
    audience = adaptiveParams.audience |> AVal.force
    text = adaptiveParams.text |> AVal.force
    mediaUrl = adaptiveParams.mediaUrl |> AVal.force
    mediaType = adaptiveParams.mediaType |> AVal.force
  }

  let resetParams adaptiveParams =
    adaptiveParams.audience.setValue Posts.ReplyAudience.Everyone
    adaptiveParams.text.setValue ""
    adaptiveParams.mediaUrl.setValue None
    adaptiveParams.mediaType.setValue Posts.Text

module Composer =

  let private onPublisPost (enableControls: cval<bool>) onPost postParams = async {
    enableControls.setValue false

    do!
      onPost(AdaptiveParams.toPostParams postParams)
      |> AsyncResult.map(fun () -> AdaptiveParams.resetParams postParams)
      |> AsyncResult.teeError(fun err -> printfn "Error: %s" err)
      |> AsyncResult.ignoreError

    enableControls.setValue true
  }

  let private replyAudienceFnTpl: FuncDataTemplate<Posts.ReplyAudience> =
    FuncDataTemplate<Posts.ReplyAudience>(fun audience _ ->
      ComboBoxItem()
        .content(
          match audience with
          | Posts.ReplyAudience.Everyone -> "Everyone"
          | Posts.ReplyAudience.AccountsYouFollow -> "Accounts you follow"
          | Posts.ReplyAudience.MentionedOnly -> "Mentioned only"
        ))

  let replyAudienceSelect(enableControls, audience) =
    ComboBox()
      .isEnabled(enableControls |> AVal.toBinding)
      .itemsSource(
        [
          Posts.ReplyAudience.Everyone
          Posts.ReplyAudience.AccountsYouFollow
          Posts.ReplyAudience.MentionedOnly
        ]
      )
      .itemTemplate(replyAudienceFnTpl)
      .selectedItem(audience |> CVal.toBinding)

  let private composerTextBox
    (enableControls: aval<bool>, text: cval<string>)
    : Control =
    TextBox()
      .StyleAsComposerTextBox()
      .watermark("What's on your mind?")
      .isEnabled(enableControls |> AVal.toBinding)
      .text(text |> CVal.toBinding)

  let private publishPostButton(enableButton: aval<bool>, onClick) =
    Button()
      .StyleAsComposerButton()
      .content("Post")
      .isEnabled(enableButton |> AVal.toBinding)
      .OnClickHandler(fun _ _ -> onClick())

  let composerCharacterCounter(characterCount) =

    let foreground = adaptive {
      let! count = characterCount

      return
        match count with
        | count when count > 500 -> Avalonia.Media.Brushes.Red
        | count when count > 400 -> Avalonia.Media.Brushes.Orange
        | _ -> Avalonia.Media.Brushes.Green
    }

    TextBlock()
      .StyleAsComposerCounter()
      .foreground(foreground |> AVal.toBinding)
      .text(
        characterCount
        |> AVal.map(fun count -> $"%i{count}/500")
        |> AVal.toBinding
      )

  let view(onPost: PostParameters -> Async<Result<unit, string>>) : Control =
    let postParams = {
      text = cval ""
      mediaUrl = cval None
      mediaType = cval Posts.Text
      audience = cval Posts.ReplyAudience.Everyone
    }

    let characterCount = postParams.text |> AVal.map _.Length
    let enableControls = cval true

    let enableButton =
      (characterCount, enableControls)
      ||> AVal.map2(fun length enableControls ->
        length <= 500 && enableControls)

    let onPublisPost = onPublisPost enableControls onPost

    DockPanel()
      .StyleAsComposer()
      .children(
        composerTextBox(enableControls, postParams.text).DockTop(),
        DockPanel()
          .DockBottom()
          .HorizontalAlignmentStretch()
          .children(
            publishPostButton(
              enableButton,
              fun () -> Async.StartImmediate(onPublisPost postParams)
            )
              .DockRight(),
            composerCharacterCounter(characterCount).DockRight(),
            replyAudienceSelect(enableControls, postParams.audience).DockLeft()
          )
      )
