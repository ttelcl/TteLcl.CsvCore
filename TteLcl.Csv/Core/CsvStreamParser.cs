using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv.Core;

/// <summary>
/// Wraps a <see cref="TextReader"/> with low level CSV reading and parsing logic,
/// allowing to read the CSV data field by field. While line breaks are recognized
/// and surfaced, this object does not construct records (CSV lines), it just
/// returns a sequence of CSV fields and line break markers.
/// </summary>
public class CsvStreamParser: IDisposable
{
  private readonly CsvParseStateMachine _stateMachine;
  private bool _disposedValue;
  private CsvToken? _pendingToken;
  private bool _baseEof;

  /// <summary>
  /// Create a new <see cref="CsvStreamParser"/>
  /// </summary>
  /// <param name="reader">
  /// The <see cref="TextReader"/> providing the text data source
  /// </param>
  /// <param name="separator">
  /// The CSV separator to use. Normally ',' or sometimes ';', but any character
  /// other than '"', '\r' and '\n' is allowed. Default ','.
  /// </param>
  /// <param name="leaveOpen">
  /// If true, disposing this reader does not dispose <paramref name="reader"/>.
  /// Otherwise (the normal case) it does.
  /// </param>
  public CsvStreamParser(
    TextReader reader,
    char separator = ',',
    bool leaveOpen = false)
  {
    BaseReader = reader;
    _stateMachine = new CsvParseStateMachine(separator);
    LeaveOpen = leaveOpen;
    _baseEof = false;
  }

  /// <summary>
  /// Create a new <see cref="CsvStreamParser"/>
  /// </summary>
  /// <param name="fileName">
  /// The name of the file to open
  /// </param>
  /// <param name="separator">
  /// The CSV separator to use. Normally ',' or sometimes ';', but any character
  /// other than '"', '\r' and '\n' is allowed. Default ','.
  /// </param>
  public CsvStreamParser(
    string fileName,
    char separator = ',')
    : this(File.OpenText(fileName), separator, false)
  {
  }

  /// <summary>
  /// The underlying text reader
  /// </summary>
  public TextReader BaseReader { get; }

  /// <summary>
  /// If true, disposing this <see cref="CsvStreamParser"/> does not
  /// dispose <see cref="BaseReader"/>
  /// </summary>
  public bool LeaveOpen { get; }

  /// <summary>
  /// The CSV separator character in use
  /// </summary>
  public char Separator => _stateMachine.Separator;

  /// <summary>
  /// Retrieve the next <see cref="CsvToken"/>
  /// </summary>
  /// <returns></returns>
  public CsvToken NextToken()
  {
    if(_pendingToken.HasValue)
    {
      var ret = _pendingToken.Value;
      _pendingToken = null;
      return ret;
    }
    var rawToken = NextRawToken();
    switch(rawToken.Kind)
    {
      case CsvParseTokenType.None:
        throw new InvalidOperationException(
          "Internal error - NextRawToken returned None");
      case CsvParseTokenType.IntermediateField:
        return CsvToken.Field(rawToken.Value);
      case CsvParseTokenType.EndOfLineField:
        _pendingToken = CsvToken.EndOfLine();
        return CsvToken.Field(rawToken.Value);
      case CsvParseTokenType.EndOfFileField:
        _pendingToken = CsvToken.EndOfFile();
        return CsvToken.Field(rawToken.Value);
      case CsvParseTokenType.EndOfLine:
        return CsvToken.EndOfLine();
      case CsvParseTokenType.EndOfFile:
        return CsvToken.EndOfFile();
      default:
        throw new InvalidOperationException(
          $"Internal error: unknown token type {rawToken.Kind}");
    }
  }

  private CsvParseToken NextRawToken()
  {
    CsvParseToken rawToken;
    do
    {
      if(_baseEof)
      {
        return CsvParseToken.EndOfFile();
      }
      char character;
      var ch = BaseReader.Read();
      if(ch == 0)
      {
        character = '\0';
        _baseEof = true;
      }
      else
      {
        character = (char)ch;
      }
      rawToken = _stateMachine.ParseCharacter(character);
    } while(rawToken.Kind == CsvParseTokenType.None);
    return rawToken;
  }

  /// <summary>
  /// Implement Dispose pattern
  /// </summary>
  protected virtual void Dispose(bool disposing)
  {
    if(!_disposedValue)
    {
      _disposedValue=true;
      if(disposing)
      {
        if(!LeaveOpen)
        {
          BaseReader.Dispose();
        }
      }
    }
  }

  /// <inheritdoc/>
  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
