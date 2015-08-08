#r "System.ComponentModel.Composition"
#if INTERACTIVE
#r "../Pretzel.exe"
#r "LibSass.x86.dll"
#r "libsassnet.dll"
#endif

open System.ComponentModel.Composition
open Pretzel.Logic.Extensibility
open Pretzel.Logic.Templating.Context
open System.IO.Abstractions
open LibSassNet
open Pretzel.Logic.Commands

type [<Export(typeof<ITransform>)>] SassTranformer
    [<ImportingConstructor>] (fileSystem: IFileSystem, commandParameters: CommandParameters) =

    let processSass paths path =
        let sassCompiler = SassCompiler()
        let result = sassCompiler.CompileFile(inputPath = path,
                                              outputStyle = OutputStyle.Compressed,
                                              includeSourceComments = false,
                                              additionalIncludePaths = paths)
        result.CSS

    let transformPage (page: Page) = do
        if fileSystem.File.Exists page.Filepath then
            let cssFilePath = fileSystem.Path.ChangeExtension(page.OutputFile, ".css")
            let paths = [| fileSystem.Path.Combine(commandParameters.Path, "_sass") |]
            do fileSystem.File.WriteAllText(cssFilePath, processSass paths page.Filepath)
            do fileSystem.File.Delete(page.OutputFile)

    let transform (siteContext: SiteContext) = do
        siteContext.Pages
        |> Seq.filter (fun p ->
            let ext = fileSystem.Path.GetExtension(p.File)
            ext = ".sass" || ext = ".scss")
        |> Seq.iter transformPage

    interface ITransform with
        member __.Transform(siteContext) =
            transform siteContext
