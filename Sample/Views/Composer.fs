namespace Sample.Views

open Avalonia.Controls
open Avalonia.Controls.Templates

open NXUI
open NXUI.FSharp.Extensions
open Threads.Lib

open FSharp.Data.Adaptive
open Navs.Avalonia
open Sample

module Composer =

  let replyAudienceFnTpl: FuncDataTemplate<Posts.ReplyAudience> =
    FuncDataTemplate<Posts.ReplyAudience>(fun audience _ ->
      ComboBoxItem()
        .content(
          match audience with
          | Posts.ReplyAudience.Everyone -> "Everyone"
          | Posts.ReplyAudience.AccountsYouFollow -> "Accounts you follow"
          | Posts.ReplyAudience.MentionedOnly -> "Mentioned only"
        ))

  let view(onPost: PostParameters -> Async<Result<unit, string>>) : Control =
    let text = cval("")
    let selectedAudience = cval(Posts.ReplyAudience.Everyone)
    let mediaType = cval(Posts.Text)
    let mediaUrl = cval(None)
    let enableControls = cval true
    let characterCount = text |> AVal.map _.Length

    let enableButton =
      (characterCount, enableControls)
      ||> AVal.map2(fun length enableControls ->
        length <= 500 && enableControls)


    let resetParams() =
      text.setValue("")
      selectedAudience.setValue(Posts.ReplyAudience.Everyone)
      mediaType.setValue(Posts.Text)
      mediaUrl.setValue(None)

    DockPanel()
      .lastChildFill(true)
      .HorizontalAlignmentCenter()
      .margin(4, 8)
      .minWidth(320)
      .children(
        TextBox()
          .HorizontalAlignmentStretch()
          .VerticalAlignmentStretch()
          .watermark("What's on your mind?")
          .minHeight(124)
          .maxWidth(320)
          .maxHeight(280)
          .TextWrappingWrap()
          .DockTop()
          .acceptsReturn(true)
          .margin(0, 0, 0, 8)
          .isEnabled(enableControls |> AVal.toBinding)
          .text(text |> CVal.toBinding),
        DockPanel()
          .HorizontalAlignmentStretch()
          .DockBottom()
          .lastChildFill(true)
          .children(
            Button()
              .content("Post")
              .DockRight()
              .height(46)
              .VerticalContentAlignmentCenter()
              .HorizontalContentAlignmentCenter()
              .width(64)
              .isEnabled(enableButton |> AVal.toBinding)
              .OnClickHandler(fun _ _ ->
                async {
                  enableControls.setValue false

                  let result =
                    onPost(
                      {
                        text = text |> AVal.force
                        mediaUrl = mediaUrl |> AVal.force
                        mediaType = mediaType |> AVal.force
                        audience = selectedAudience |> AVal.force
                      }
                    )

                  match! result with
                  | Ok _ -> resetParams()
                  | Error e -> printfn $"%A{e}"

                  enableControls.setValue true
                }
                |> Async.StartImmediate),
            TextBlock()
              .DockRight()
              .foreground(
                characterCount
                |> AVal.map(fun count ->
                  match count with
                  | count when count > 500 -> Avalonia.Media.Brushes.Red
                  | count when count > 400 -> Avalonia.Media.Brushes.Orange
                  | _ -> Avalonia.Media.Brushes.Green)
                |> AVal.toBinding
              )
              .text(
                characterCount
                |> AVal.map(fun count -> $"%i{count}/500")
                |> AVal.toBinding
              ),
            ComboBox()
              .DockLeft()
              .isEnabled(enableControls |> AVal.toBinding)
              .itemsSource(
                [
                  Posts.ReplyAudience.Everyone
                  Posts.ReplyAudience.AccountsYouFollow
                  Posts.ReplyAudience.MentionedOnly
                ]
              )
              .itemTemplate(replyAudienceFnTpl)
              .selectedItem(selectedAudience |> CVal.toBinding)
          )
      )
