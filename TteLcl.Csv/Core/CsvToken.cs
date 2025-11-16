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
/// A token in a CSV stream: a field, end-of-line, end-of-file
/// </summary>
public readonly struct CsvToken
{
  private readonly string? _fieldValue;

  /// <summary>
  /// Create a new CsvToken
  /// </summary>
  private CsvToken(CsvTokenType tokenType, string? value)
  {
    _fieldValue = value;
    TokenType = tokenType;
  }

  /// <summary>
  /// Create an End Of File token
  /// </summary>
  /// <returns></returns>
  public static CsvToken EndOfFile() => new(CsvTokenType.EndOfFile, null);

  /// <summary>
  /// Create an End Of Line token
  /// </summary>
  /// <returns></returns>
  public static CsvToken EndOfLine() => new(CsvTokenType.EndOfLine, null);

  /// <summary>
  /// Create a Field token
  /// </summary>
  /// <param name="fieldValue">
  /// The value of the field
  /// </param>
  /// <returns></returns>
  public static CsvToken Field(string fieldValue) => new(CsvTokenType.Field, fieldValue);


  /// <summary>
  /// The kind of token
  /// </summary>
  public CsvTokenType TokenType { get; }

  /// <summary>
  /// The field value in case <see cref="TokenType"/> is <see cref="CsvTokenType.Field"/>
  /// (throwing an exception otherwise)
  /// </summary>
  public string FieldValue {
    get =>
      TokenType switch {
        CsvTokenType.EndOfFile => throw new InvalidOperationException("EOF has no field value"),
        CsvTokenType.EndOfLine => throw new InvalidOperationException("EOLN has no field value"),
        CsvTokenType.Field =>
          _fieldValue ?? throw new InvalidOperationException("Not expecting a null field value (internal error)"),
        _ => throw new InvalidOperationException($"Unrecognized CSV token type: {TokenType}"),
      };
  }

}
