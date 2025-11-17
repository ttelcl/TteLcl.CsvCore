using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv.Core;

/// <summary>
/// Implements <see cref="ICsvWriter"/> (and thus <see cref="ICsvWriterBase"/>) for writing CSV
/// to a <see cref="TextWriter"/> or file. Also exposes static helper methods to encode or
/// quote fields, useful for other implementations of this interface.
/// </summary>
public class CsvWriter: ICsvWriter
{
  private bool _disposed;

  /// <summary>
  /// Create a new <see cref="CsvWriter"/> instance
  /// </summary>
  /// <param name="baseWriter">
  /// The underlying text writer to write to
  /// </param>
  /// <param name="separator">
  /// The CSV separator character to use (default: ',')
  /// </param>
  /// <param name="leaveOpen">
  /// If true, disposing this <see cref="CsvWriter"/> does NOT dispose
  /// <paramref name="baseWriter"/>. If false (default), it does.
  /// </param>
  public CsvWriter(
    TextWriter baseWriter,
    char separator = ',',
    bool leaveOpen = false)
  {
    BaseWriter = baseWriter;
    LeaveOpen = leaveOpen;
    Separator = separator;
    if(separator == '"' || separator == '\r' || separator == '\n' || separator == '\0')
    {
      throw new ArgumentOutOfRangeException(
        nameof(separator), "Unsupported separator character: Double quotes and line breaks can not be used as CSV separator");
    }
  }

  /// <summary>
  /// Create a new <see cref="CsvWriter"/> instance for writing to the specified file
  /// (creating the file if necessary)
  /// </summary>
  /// <param name="filename">
  /// The name of the file to write.
  /// </param>
  /// <param name="separator">
  /// The CSV separator character to use (default ',')
  /// </param>
  public CsvWriter(
    string filename,
    char separator = ',')
    : this(File.CreateText(filename), separator)
  {
  }

  /// <summary>
  /// The underlying <see cref="TextWriter"/> to write to
  /// </summary>
  public TextWriter BaseWriter { get; }

  /// <summary>
  /// If true, disposing this <see cref="CsvWriter"/> does not
  /// dispose <see cref="BaseWriter"/>
  /// </summary>
  public bool LeaveOpen { get; }

  /// <summary>
  /// The separator character to use
  /// </summary>
  public char Separator { get; }

  /// <summary>
  /// The number of fields that have been written this line so far
  /// </summary>
  public int FieldsWrittenThisLine { get; private set; }

  /// <inheritdoc/>
  public void WriteField(string field)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if(FieldsWrittenThisLine > 0)
    {
      BaseWriter.Write(Separator);
    }
    BaseWriter.Write(EncodeField(field));
    FieldsWrittenThisLine++;
  }

  /// <inheritdoc/>
  public void WriteLine()
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    BaseWriter.WriteLine();
    FieldsWrittenThisLine = 0;
  }

  /// <summary>
  /// Encode <paramref name="field"/> as a CSV field considering the current <see cref="Separator"/>.
  /// If quoting is required this will return the quoted value, otherwise it returns the field itself.
  /// </summary>
  /// <param name="field">
  /// The field to encode
  /// </param>
  /// <returns>
  /// Either <paramref name="field"/> unmodified, or its quoted value.
  /// </returns>
  public string EncodeField(string field)
  {
    return EncodeField(field, Separator);
  }

  /// <summary>
  /// Encode <paramref name="field"/> as a CSV field considering the given <paramref name="separator"/>.
  /// If quoting is required this will return the quoted value, otherwise it returns the field itself.
  /// </summary>
  /// <param name="field">
  /// The field to encode
  /// </param>
  /// <param name="separator">
  /// The separator in use. This character appearing in <paramref name="field"/> is one of the possible
  /// reasons that make quoting necessary
  /// </param>
  /// <returns>
  /// Either <paramref name="field"/> unmodified, or its quoted value.
  /// </returns>
  public static string EncodeField(string field, char separator)
  {
    if(field.Contains(separator)
      || field.IndexOfAny(__charactersThatRequireQuoting)>=0)
    {
      return QuoteField(field);
    }
    if(field.Length>0 && (Char.IsWhiteSpace(field[0]) || Char.IsWhiteSpace(field[^1])))
    {
      return QuoteField(field);
    }
    return field;
  }

  private static readonly char[] __charactersThatRequireQuoting = ['\r', '\n', '"'];

  /// <summary>
  /// Return the field in quoted form (even if there is no need to quote it)
  /// </summary>
  /// <param name="field">
  /// The field to quote
  /// </param>
  /// <returns>
  /// The quoted field
  /// </returns>
  public static string QuoteField(string field)
  {
    var sb = new StringBuilder();
    sb.Append('"');
    foreach(var ch in field)
    {
      sb.Append(ch);
      if(ch == '"')
      {
        // Duplicate the embedded '"' character to escape it.
        sb.Append('"');
      }
    }
    sb.Append('"');
    return sb.ToString();
  }

  /// <summary>
  /// Implements the Dispose Pattern
  /// </summary>
  /// <param name="disposing"></param>
  protected virtual void Dispose(bool disposing)
  {
    if(!_disposed)
    {
      _disposed=true;
      if(disposing && !LeaveOpen)
      {
        BaseWriter.Dispose();
      }
    }
  }

  /// <inheritdoc/>
  public void Dispose()
  {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
