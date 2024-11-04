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

  let view(onPost: PostParameters -> unit) : Control =
    let text = cval("")
    let selectedAudience = cval(Posts.ReplyAudience.Everyone)
    let mediaType = cval(Posts.Text)
    let mediaUrl = cval(None)

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
          .DockTop()
          .acceptsReturn(true)
          .minHeight(124)
          .margin(0, 0, 0, 8)
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
              .OnClickHandler(fun _ _ ->
                onPost(
                  {
                    text = text |> AVal.force
                    mediaUrl = mediaUrl |> AVal.force
                    mediaType = mediaType |> AVal.force
                    audience = selectedAudience |> AVal.force
                  }
                )),
            ComboBox()
              .itemsSource(
                [
                  Posts.ReplyAudience.Everyone
                  Posts.ReplyAudience.AccountsYouFollow
                  Posts.ReplyAudience.MentionedOnly
                ]
              )
              .itemTemplate(replyAudienceFnTpl)
              .DockLeft()
              .selectedItem(selectedAudience |> CVal.toBinding)
          )
      )
