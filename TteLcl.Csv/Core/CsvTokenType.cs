/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv.Core;

/// <summary>
/// The different tokens in a CSV stream
/// </summary>
public enum CsvTokenType
{

  /// <summary>
  /// End of the stream
  /// </summary>
  EndOfFile = 0,

  /// <summary>
  /// End of line token (no additional value). Indicates the end of a record.
  /// </summary>
  EndOfLine,

  /// <summary>
  /// A CSV field value (with a string value)
  /// </summary>
  Field,

}
