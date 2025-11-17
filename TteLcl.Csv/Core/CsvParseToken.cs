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
/// Description of CsvParseToken
/// </summary>
public readonly struct CsvParseToken
{
  private readonly string? _value;

  /// <summary>
  /// Create a new CsvParseToken
  /// </summary>
  private CsvParseToken(CsvParseTokenType kind, string? value = null)
  {
    Kind = kind;
    _value = value;
  }

  /// <summary>
  /// Create a new <see cref="CsvParseToken"/> with type <see cref="CsvParseTokenType.None"/>
  /// </summary>
  /// <returns></returns>
  public static CsvParseToken None { get; } =
    new CsvParseToken(CsvParseTokenType.None, null);

  /// <summary>
  /// Create a new <see cref="CsvParseToken"/> with type <see cref="CsvParseTokenType.IntermediateField"/>
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
  public static CsvParseToken Field(string value) =>
    new CsvParseToken(CsvParseTokenType.IntermediateField, value);

  /// <summary>
  /// Create a new <see cref="CsvParseToken"/> with type <see cref="CsvParseTokenType.EndOfLineField"/>
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
  public static CsvParseToken EndOfLine(string value) =>
    new CsvParseToken(CsvParseTokenType.EndOfLineField, value);

  /// <summary>
  /// Create a new <see cref="CsvParseToken"/> with type <see cref="CsvParseTokenType.EndOfLine"/>
  /// </summary>
  /// <returns></returns>
  public static CsvParseToken EndOfLine() =>
    new CsvParseToken(CsvParseTokenType.EndOfLine);

  /// <summary>
  /// Create a new <see cref="CsvParseToken"/> with type <see cref="CsvParseTokenType.EndOfFileField"/>
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
  public static CsvParseToken EndOfFile(string value) =>
    new CsvParseToken(CsvParseTokenType.EndOfFileField, value);

  /// <summary>
  /// Create a new <see cref="CsvParseToken"/> with type <see cref="CsvParseTokenType.EndOfFile"/>
  /// </summary>
  /// <returns></returns>
  public static CsvParseToken EndOfFile() =>
    new CsvParseToken(CsvParseTokenType.EndOfFile);

  /// <summary>
  /// The value of the field. Only valid when <see cref="Kind"/> is one of
  /// <see cref="CsvParseTokenType.IntermediateField"/>, <see cref="CsvParseTokenType.EndOfLineField"/>,
  /// or <see cref="CsvParseTokenType.EndOfFileField"/>.
  /// </summary>
  public string Value =>
    Kind switch {
      CsvParseTokenType.None or
      CsvParseTokenType.EndOfLine or
      CsvParseTokenType.EndOfFile =>
        throw new InvalidOperationException("This token type has no value"),
      _ => _value ?? throw new InvalidOperationException(
        "Internal error: not expecting a null value"),
    };

  /// <summary>
  /// The kind of CSV parse token
  /// </summary>
  public CsvParseTokenType Kind { get; }

  /// <summary>
  /// True if there is a field value included in this token
  /// </summary>
  public bool HasValue =>
    Kind switch {
      CsvParseTokenType.None or
      CsvParseTokenType.EndOfLine or
      CsvParseTokenType.EndOfFile => false,
      _ => _value != null, // expected to be true, but err on the safe side
    };

  /// <summary>
  /// True if there is an End of Line. Note! This will be false if there is an End of File,
  /// even there logically should have been an End of Line first.
  /// </summary>
  public bool HasEoln => Kind == CsvParseTokenType.EndOfLine || Kind == CsvParseTokenType.EndOfLineField;

  /// <summary>
  /// True if there is an End of File.
  /// </summary>
  public bool HasEof => Kind == CsvParseTokenType.EndOfFile || Kind == CsvParseTokenType.EndOfFileField;
}

/// <summary>
/// The types of tokens coming from the low level parser
/// (some of these correspond to multiple higher level <see cref="CsvTokenType"/> tokens)
/// </summary>
public enum CsvParseTokenType
{
  /// <summary>
  /// Parsing in progress, nothing to report
  /// </summary>
  None,

  /// <summary>
  /// A field followed by a separator
  /// </summary>
  IntermediateField,

  /// <summary>
  /// A field followed by an end-of-record (end of line)
  /// </summary>
  EndOfLineField,

  /// <summary>
  /// A field followed by the end of file
  /// </summary>
  EndOfFileField,

  /// <summary>
  /// A lone end-of-record (empty record)
  /// </summary>
  EndOfLine,

  /// <summary>
  /// A lone end of file
  /// </summary>
  EndOfFile,
}
