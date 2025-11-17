module AppTokenize

open System
open System.IO
open System.Reflection

open TteLcl.Csv
open TteLcl.Csv.Core

open ColorPrint
open CommonTools

type private Options = {
  InputFile: string
  RawMode: bool
  Separator: char
  LineMode: bool
  IncludeEmptyLines: bool
  ExactFieldCount: int
}

let private runTokenize o =
  use streamParser = new CsvStreamParser(o.InputFile, o.Separator)
  if o.LineMode then
    let buffer = new CsvReadBuffer()
    let skipLines = o.IncludeEmptyLines |> not
    let lines () =
      if o.RawMode then
        buffer.EnumLines(streamParser.ParseTokenSource, o.ExactFieldCount, skipLines)
      else
        buffer.EnumLines(streamParser.PlainTokenSource, o.ExactFieldCount, skipLines)
    for line in lines() do
      cpx $"\fb{line.Count}\f0>"
      for field in line do
        cpx $"\fo[\fg{field}\fo]\f0"
      cp "\f0<"
    0
  else
    if o.RawMode then
      for t in streamParser.EnumerateAsRawTokenStream() do
        cpx $"\fc{t.Kind,-18}\f0"
        if t.HasValue then
          cpx $"  '\fg{t.Value}\f0'"
        cp ""
    else
      for t in streamParser.EnumerateAsTokenStream() do
        cpx $"\fc{t.TokenType,-10}\f0"
        if t.TokenType = CsvTokenType.Field then
          cpx $"  '\fg{t.FieldValue}\f0'"
        cp ""
    0

let run args =
  let rec parseMore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parseMore o
    | "--help" :: _
    | "-h" :: _ ->
      None
    | "-i" :: file :: rest ->
      rest |> parseMore {o with InputFile = file}
    | "-raw" :: rest ->
      rest |> parseMore {o with RawMode = true}
    | "-lines" :: rest ->
      rest |> parseMore {o with LineMode = true}
    | "-empty" :: rest ->
      rest |> parseMore {o with LineMode = true; IncludeEmptyLines = true}
    | "-n" :: count :: rest ->
      let exactCount = count |> Int32.Parse
      rest |> parseMore {o with ExactFieldCount = exactCount}
    | "-same" :: rest ->
      rest |> parseMore {o with ExactFieldCount = -1}
    | "-sep" :: ch :: rest ->
      if ch.Length <> 1 then
        cp "\frError: \foexpecting a single character as argument to \fg-sep\f0."
        None
      else
        rest |> parseMore {o with Separator = ch[0]}
    | [] ->
      if o.InputFile |> String.IsNullOrEmpty then
        cp "\foNo input file (\fg-i\fo) given\f0."
        None
      else
        o |> Some
    | x :: _ ->
      cp $"\frUnrecognized argument \f0'\fy{x}\f0'"
      None
  let oo = args |> parseMore {
    InputFile = null
    RawMode = false
    Separator = ','
    LineMode = false
    IncludeEmptyLines = false
    ExactFieldCount = 0
  }
  match oo with
  | Some(o) ->
    o |> runTokenize
  | None ->
    cp ""
    Usage.usage "tokenize"
    1




