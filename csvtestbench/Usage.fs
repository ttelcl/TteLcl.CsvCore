// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage focus =
  let showSection section =
    focus = "" || focus = "all" || focus = section
  let showDetails section =
    focus = "all" || focus = section
  if showSection "" || showSection "all" then
    cp "\fyUtility for testing TteLcl.Csv functionality\f0"
    cp ""
  if showSection "tokenize" then
    cp "\focsvtestbench tokenize \fg-i \fcfile.csv\f0 [\fg-raw\f0] [\fg-sep \fcseparator\f0]"
    cp "  Report CSV tokens in the file."
  if showDetails "tokenize" then
    cp "  \fg-raw\f0\fx             Report raw tokens"
    cp "  \fg-sep \fcseparator\f0   Separator character to use. Default is '\fo,\f0'"
    cp ""
  if true then
    cp "Common options:"
    cp "  \fg-h             \f0Show help"
    cp "  \fg-v             \f0Verbose mode"



