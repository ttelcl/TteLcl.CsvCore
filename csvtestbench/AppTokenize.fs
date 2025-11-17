module AppTokenize

open System
open System.IO
open System.Reflection

open TteLcl.Csv.Core

open ColorPrint
open CommonTools

type private Options = {
  InputFile: string
  RawMode: bool
  Separator: char
}

let private runTokenize o =
  use streamParser = new CsvStreamParser(o.InputFile, o.Separator)
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
  }
  match oo with
  | Some(o) ->
    o |> runTokenize
  | None ->
    cp ""
    Usage.usage "tokenize"
    1




