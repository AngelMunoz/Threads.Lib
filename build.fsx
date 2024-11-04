#r "nuget: Fun.Build, 1.1.14"
#r "nuget: Fun.Result, 2.1.0"

open System.IO
open Fun.Build

let restore name = stage $"Restoring {name}" { run $"dotnet restore {name}" }

let build name = stage $"Building {name}" {
  run $"dotnet build {name} -f net8.0 --no-restore -c Release"
}

let test name = stage $"Testing {name}" {
  run $"dotnet test {name} -f net8.0 --no-restore"
}

let pack name = stage $"Packing {name}" {
  run $"dotnet pack {name} --no-build --no-restore -o dist"
}

let pushNugets = stage $"Push to NuGet" {

  run(fun ctx -> async {

    let nugetApiKey = ctx.GetEnvVar "NUGET_DEPLOY_KEY"
    let nugets = Directory.GetFiles(__SOURCE_DIRECTORY__ + "/dist", "*.nupkg")

    for nuget in nugets do
      printfn "Pushing %s" nuget

      let! res =
        ctx.RunSensitiveCommand
          $"dotnet nuget push {nuget} --skip-duplicate  -s https://api.nuget.org/v3/index.json -k {nugetApiKey}"

      match res with
      | Ok _ -> return ()
      | Error err -> failwith err
  })
}


pipeline "release" {
  let project = "Threads.Lib"
  restore project
  build project
  pack project
  pushNugets
  runIfOnlySpecified true
}

pipeline "release:local" {
  let project = "Threads.Lib"
  restore project
  build project
  pack project
  runIfOnlySpecified true
}

pipeline "ci:library" {
  let project = "Threads.Lib"
  restore project
  build project
  test project
  runIfOnlySpecified true
}

pipeline "ci:docs" {
  stage "Restoring Tools" { run "dotnet tool restore" }
  restore "Threads.Lib"
  build "Threads.Lib"

  stage "Fsdocs build" {
    run "dotnet fsdocs build --properties Condfiguration=Release"
  }

  runIfOnlySpecified true
}


pipeline "ci:sample" {
  let project = "Sample/Sample.fsproj"
  restore project
  build project

  runIfOnlySpecified true
}

pipeline "build" {
  stage "Restoring Tools" { run "dotnet tool restore" }
  stage "Restore Solution" { run "dotnet restore" }
  stage "Build Solution" { run "dotnet build -f net8.0 --no-restore -tl" }
  stage "Test Solution" { run "dotnet test -f net8.0 --no-restore" }

  runIfOnlySpecified true
}

tryPrintPipelineCommandHelp()
