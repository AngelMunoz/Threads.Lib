namespace Sample.Views


open System
open System.Diagnostics

open Avalonia.Controls.Templates
open Avalonia.Media
open IcedTasks
open IcedTasks.Polyfill.Async

open FSharp.Data.Adaptive

open Avalonia
open Avalonia.Controls
open Avalonia.Data

open NXUI.Desktop
open NXUI.FSharp.Extensions

open Navs
open Navs.Avalonia

open ScottPlot
open ScottPlot.Avalonia

open Threads.Lib

open Sample
open Sample.Services


module Metrics =

  type DataRange with

    member this.AsString =
      match this with
      | Week -> "week"
      | Month -> "month"
      | Year -> "year"

  let private totalValueMetricCard(metric: Metric) =

    let totalValue =
      metric.totalValue
      |> ValueOption.map(fun v -> $"%i{v}")
      |> ValueOption.defaultValue "N/A"

    let title =
      match metric.name with
      | Insights.Views -> "Views"
      | Insights.Likes -> "Likes"
      | Insights.Replies -> "Replies"
      | Insights.Reposts -> "Reposts"
      | Insights.Quotes -> "Quotes"
      | Insights.Shares -> "Shares"
      | Insights.FollowerCount -> "Followers"
      | Insights.FollowerDemographics -> "Follower Demographics"

    let title = TextBlock().text(title).fontSize(16).margin(8)
    let value = TextBlock().text(totalValue).fontSize(24).margin(8)

    Border()
      .cornerRadius(5)
      .margin(8.)
      .borderBrush(Brushes.Gray)
      .borderThickness(Thickness(1, 1, 1.5, 1.5))
      .boxShadow(
        BoxShadows(
          BoxShadow(
            OffsetX = 5,
            OffsetY = 2,
            Blur = 5,
            Color = Color.FromRgb(0uy, 0uy, 0uy)
          )
        )
      )
      .padding(2.)
      .child(
        StackPanel()
          .HorizontalAlignmentCenter()
          .VerticalAlignmentCenter()
          .margin(8)
          .children(title, value)
      )

  let private metricsPanel(metricsList: aval<Metric list>) =
    ItemsControl()
      .HorizontalAlignmentCenter()
      .itemsPanel(FuncTemplate<Panel>(fun () -> WrapPanel()))
      .itemTemplate(
        FuncDataTemplate<_>(fun metric _ -> totalValueMetricCard metric)
      )
      .itemsSource(metricsList |> AVal.toBinding)

  let private viewsPlot(metricsList: aval<Metric list>) : aval<Control> =
    let plot = AvaPlot() |> AVal.constant

    let data =
      metricsList
      |> AVal.map(fun metrics ->
        metrics |> List.fold (fun acc m -> m.data @ acc) [])

    let xData =
      data
      |> AVal.map(fun data ->
        data |> List.map(fun d -> d.endTime.Value.DateTime))

    let yData = data |> AVal.map(fun data -> data |> List.map(fun d -> d.value))

    adaptive {
      let! plot = plot
      let! x = xData
      let! y = yData

      plot.Plot.Clear()

      plot
        .height(200)
        .minWidth(400)
        .Plot.Add.ScatterLine(x |> List.toArray, y |> List.toArray)
      |> ignore

      plot.Plot.Axes.DateTimeTicksBottom() |> ignore

      return plot
    }

  let dataRangeSelect(range, onRangeChanged) =
    ComboBox()
      .selectedItem(range |> AVal.toBinding)
      .itemsSource([ Week; Month; Year ])
      .itemTemplate(
        FuncDataTemplate<DataRange>(fun r _ -> TextBlock().text($"{r}"))
      )
      .OnSelectionChangedHandler(fun sender args ->
        sender.SelectedItem |> unbox<DataRange> |> onRangeChanged)

  let private viewsControl(metricsList: aval<Metric list>) =
    UserControl()
      .OnSizeChangedHandler(fun sender _ ->

        match sender.Content with
        | :? AvaPlot as plot ->
          let width =
            if sender.Width <= 0 || Double.IsNaN sender.Width then
              400
            else
              int sender.Width

          plot.Plot.RenderInMemory(width, 200)
        | null
        | _ -> ())
      .content(
        adaptive {
          let! length = metricsList |> AVal.map _.Length

          if length > 0 then
            return! viewsPlot metricsList
          else
            return TextBlock().text("No data")
        }
        |> AVal.toBinding
      )

  let page (metricsStore: MetricsStore) ctx _ : Async<Control> = async {
    let! token = Async.CancellationToken

    let viewsData =
      metricsStore.metrics
      |> AVal.map(fun metrics ->
        metrics |> List.filter(fun m -> m.name = Insights.Views))

    Async.StartImmediate(metricsStore.loadMetrics(), token)

    let nonViewsData =
      metricsStore.metrics
      |> AVal.map(fun m -> m |> List.filter(fun m -> m.name <> Insights.Views))

    let viewsTitle =
      metricsStore.range
      |> AVal.map(fun r -> $"Views for the last %s{r.AsString}")

    let metricsTitle =
      metricsStore.range
      |> AVal.map(fun r -> $"Metrics for the last %s{r.AsString}")

    let onDataRangeChanged value =
      Async.StartImmediate(metricsStore.updateRange value)

    return
      UserControl()
        .content(
          StackPanel()
            .children(
              DockPanel()
                .children(
                  TextBlock()
                    .DockLeft()
                    .fontSize(24)
                    .margin(8)
                    .text(viewsTitle |> AVal.toBinding),
                  dataRangeSelect(metricsStore.range, onDataRangeChanged)
                    .DockRight()
                    .margin(8)
                ),
              viewsControl(viewsData),
              TextBlock()
                .text(metricsTitle |> AVal.toBinding)
                .fontSize(24)
                .margin(8)
                .HorizontalAlignmentCenter(),
              metricsPanel(nonViewsData)
            )
        )
  }
