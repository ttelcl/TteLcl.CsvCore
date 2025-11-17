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
    cp "\focsvtestbench tokenize \fg-i \fcfile.csv\f0 [\fg-raw\f0] [\fg-lines\f0] [\fg-sep \fcseparator\f0] [\fg-empty\f0] [\fg-n \fcfieldcount\f0|\fg-same\f0]"
    cp "  Report CSV tokens in the file."
  if showDetails "tokenize" then
    cp "  \fg-raw\f0\fx             Report raw tokens"
    cp "  \fg-lines\f0\fx           Instead of reporting individual tokens, report lines."
    cp "  \fg-empty\f0\fx           Include empty lines. Implies \fg-lines\f0."
    cp "  \fg-n \fcfieldcount\f0    The exact number of fields per line to expect, or 0 to ignore. Ignored unless \fg-lines\f0 or \fg-empty\f0 is given."
    cp "  \fg-same\f0\fx            All lines must have the same field count. Equivalent to \fg-n \fc-1\f0."
    cp "  \fg-sep \fcseparator\f0   Separator character to use. Default is '\fo,\f0'"
    cp ""
  if true then
    cp "Common options:"
    cp "  \fg-h             \f0Show help"
    cp "  \fg-v             \f0Verbose mode"



