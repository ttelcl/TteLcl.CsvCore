using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TteLcl.Csv.Core;

namespace TteLcl.Csv;

/// <summary>
/// An object that buffers a line read from a CSV stream.
/// Used as-is while filling that buffer, but clients typically
/// consume it as <see cref="IReadOnlyList{String}"/>
/// </summary>
public class CsvReadBuffer: IReadOnlyList<string>
{
  private readonly List<string> _fieldBuffer;

  /// <summary>
  /// Create a new <see cref="CsvReadBuffer"/>
  /// </summary>
  public CsvReadBuffer()
  {
    _fieldBuffer = new List<string>();
  }

  /// <inheritdoc/>
  public string this[int index] => ((IReadOnlyList<string>)_fieldBuffer)[index];

  /// <inheritdoc/>
  public int Count => ((IReadOnlyCollection<string>)_fieldBuffer).Count;

  /// <inheritdoc/>
  public IEnumerator<string> GetEnumerator()
  {
    return _fieldBuffer.GetEnumerator();
  }

  /// <inheritdoc/>
  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  /// <summary>
  /// Fill the buffer with a full line using <see cref="CsvToken"/>s from
  /// <paramref name="source"/>.
  /// </summary>
  /// <param name="source">
  /// The token source, typically <see cref="CsvStreamParser.PlainTokenSource"/>.
  /// </param>
  /// <returns></returns>
  public bool FillLine(ITokenSource<CsvToken> source)
  {
    ClearLine();
    while(!Stopped)
    {
      PutToken(source.NextToken());
    }
    return HasFullLine;
  }

  /// <summary>
  /// Fill the buffer with a full line using <see cref="CsvParseToken"/>s from
  /// <paramref name="source"/>.
  /// </summary>
  /// <param name="source">
  /// The raw token source, typically <see cref="CsvStreamParser.ParseTokenSource"/>.
  /// </param>
  /// <returns></returns>
  public bool FillLine(ITokenSource<CsvParseToken> source)
  {
    ClearLine();
    while(!Stopped)
    {
      PutToken(source.NextToken());
    }
    return HasFullLine;
  }

  /// <summary>
  /// Repeatedly fill this buffer with a full line from <paramref name="source"/>
  /// (typically <see cref="CsvStreamParser.PlainTokenSource"/>) and yield this buffer
  /// after each line. BEWARE: the same buffer is returned each time, and its contents
  /// is invalidated the next round,
  /// </summary>
  /// <param name="source">
  /// The token source
  /// </param>
  /// <param name="exactLength">
  /// If positive: the exact expected field count for each line, throwing an exception
  /// if different.
  /// If negative: all lines must have the same field count as the first line
  /// If 0: no field count restrictions.
  /// </param>
  /// <param name="skipEmpty">
  /// If true (default): ignore empty lines
  /// </param>
  /// <returns></returns>
  public IEnumerable<IReadOnlyList<string>> EnumLines(
    ITokenSource<CsvToken> source,
    int exactLength = 0,
    bool skipEmpty = true)
  {
    while(FillLine(source))
    {
      if(!skipEmpty || Count > 0)
      {
        if(exactLength > 0 && Count != exactLength)
        {
          var sample = String.Join(",", _fieldBuffer.Take(5));
          throw new InvalidOperationException(
            $"Expecting exactly {exactLength} fields per line, but received {Count}. Line starting with {sample}");
        }
        if(exactLength < 0)
        {
          exactLength = Count;
        }
        yield return this;
      }
    }
  }

  /// <summary>
  /// Repeatedly fill this buffer with a full line from <paramref name="source"/>
  /// (typically <see cref="CsvStreamParser.ParseTokenSource"/>) and yield this buffer
  /// after each line. BEWARE: the same buffer is returned each time, and its contents
  /// is invalidated the next round,
  /// </summary>
  /// <param name="source">
  /// The token source
  /// </param>
  /// <param name="exactLength">
  /// If positive: the exact expected field count for each line, throwing an exception
  /// if different.
  /// If negative: all lines must have the same field count as the first line
  /// If 0: no field count restrictions.
  /// </param>
  /// <param name="skipEmpty">
  /// If true (default): ignore empty lines
  /// </param>
  /// <returns></returns>
  public IEnumerable<IReadOnlyList<string>> EnumLines(
    ITokenSource<CsvParseToken> source,
    int exactLength = 0,
    bool skipEmpty = true)
  {

    while(FillLine(source))
    {
      if(!skipEmpty || Count > 0)
      {
        if(exactLength > 0 && Count != exactLength)
        {
          var sample = String.Join(",", _fieldBuffer.Take(5));
          throw new InvalidOperationException(
            $"Expecting exactly {exactLength} fields per line, but received {Count}. Line starting with {sample}");
        }
        if(exactLength < 0)
        {
          exactLength = Count;
        }
        yield return this;
      }
    }
  }

  /// <summary>
  /// If true, the current line is full and no new fields can be added
  /// before calling <see cref="ClearLine"/> or <see cref="Reset"/>.
  /// </summary>
  public bool HasFullLine { get; private set; }

  /// <summary>
  /// If true, the end of the input stream was observed and no new fields
  /// or lines can be added before calling <see cref="Reset"/>.
  /// </summary>
  public bool EndOfFile { get; private set; }

  /// <summary>
  /// True if this buffer does not accept new fields. Equivalent to
  /// <see cref="HasFullLine"/> <c>||</c> <see cref="EndOfFile"/>.
  /// </summary>
  public bool Stopped => EndOfFile || HasFullLine;

  /// <summary>
  /// Clear the current line and start a new one, allowing addition of new fields.
  /// (unless <see cref="EndOfFile"/> is true, in which case the line is cleared, 
  /// but field addition is still blocked).
  /// Clears the <see cref="HasFullLine"/> flag.
  /// </summary>
  public void ClearLine()
  {
    _fieldBuffer.Clear();
    HasFullLine = false;
  }

  /// <summary>
  /// Reset this buffer, readying it for use with a new source of fields.
  /// This clears the line buffer and clears the <see cref="HasFullLine"/> and
  /// <see cref="EndOfFile"/> flags
  /// </summary>
  public void Reset()
  {
    ClearLine();
    EndOfFile = false;
  }

  /// <summary>
  /// Put the next field into the buffer. Fails if <see cref="EndOfFile"/>
  /// or <see cref="HasFullLine"/> are set.
  /// Intended for manual use when <see cref="PutToken(CsvToken)"/> or
  /// <see cref="PutToken(CsvParseToken)"/> are not applicable.
  /// </summary>
  /// <param name="field"></param>
  /// <exception cref="InvalidOperationException"></exception>
  public void PutField(string field)
  {
    if(EndOfFile)
    {
      throw new InvalidOperationException(
        "Cannot add new fields after EOF. Call Reset() first.");
    }
    if(HasFullLine)
    {
      throw new InvalidOperationException(
        "Cannot add more fields to this line. Call ClearLine() first to start a new line");
    }
    _fieldBuffer.Add(field);
  }

  /// <summary>
  /// Mark this buffer as containing a full line.
  /// Intended for manual use when <see cref="PutToken(CsvToken)"/> or
  /// <see cref="PutToken(CsvParseToken)"/> are not applicable.
  /// </summary>
  public void PutEndOfLine()
  {
    HasFullLine = true;
  }

  /// <summary>
  /// Set <see cref="EndOfFile"/> to true. Does not automatically set <see cref="HasFullLine"/>.
  /// Intended for manual use when <see cref="PutToken(CsvToken)"/> or
  /// <see cref="PutToken(CsvParseToken)"/> are not applicable.
  /// </summary>
  public void PutEndOfFile()
  {
    EndOfFile = true;
  }

  /// <summary>
  /// Put field / eoln / eof into this buffer based on the given <see cref="CsvToken"/>
  /// <paramref name="token"/>
  /// </summary>
  /// <param name="token">
  /// The token to insert
  /// </param>
  /// <returns>
  /// True if more tokens can be inserted before having to handle the buffer content.
  /// False to indicate a full line and/or end of file state (inverse of <see cref="Stopped"/>)
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if <see cref="Stopped"/> is true, and a call to <see cref="ClearLine"/>
  /// and / or <see cref="Reset"/> is expected first
  /// </exception>
  public bool PutToken(CsvToken token)
  {
    if(Stopped)
    {
      throw new InvalidOperationException(
        "Incorrect use: this buffer is stopped. Expecting a call to ClearLine() and / or Reset() first.");
    }
    switch(token.TokenType)
    {
      case CsvTokenType.EndOfFile:
        PutEndOfFile();
        break;
      case CsvTokenType.EndOfLine:
        PutEndOfLine();
        break;
      case CsvTokenType.Field:
        PutField(token.FieldValue);
        break;
      default:
        throw new InvalidOperationException(
          $"Unknown token type '{token.TokenType}'");
    }
    return !Stopped;
  }

  /// <summary>
  /// Put field / eoln / eof into this buffer based on the given raw <see cref="CsvParseToken"/>
  /// <paramref name="token"/>
  /// </summary>
  /// <param name="token">
  /// The token to insert
  /// </param>
  /// <returns>
  /// True if more tokens can be inserted before having to handle the buffer content.
  /// False to indicate a full line and/or end of file state (inverse of <see cref="Stopped"/>)
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if <see cref="Stopped"/> is true, and a call to <see cref="ClearLine"/>
  /// and / or <see cref="Reset"/> is expected first
  /// </exception>
  public bool PutToken(CsvParseToken token)
  {
    if(Stopped)
    {
      throw new InvalidOperationException(
        "Incorrect use: this buffer is stopped. Expecting a call to ClearLine() and / or Reset() first.");
    }
    switch(token.Kind)
    {
      case CsvParseTokenType.EndOfFile:
        PutEndOfFile();
        break;
      case CsvParseTokenType.EndOfFileField:
        PutField(token.Value);
        PutEndOfLine();
        PutEndOfFile();
        break;
      case CsvParseTokenType.EndOfLine:
        PutEndOfLine();
        break;
      case CsvParseTokenType.EndOfLineField:
        PutField(token.Value);
        PutEndOfLine();
        break;
      case CsvParseTokenType.IntermediateField:
        PutField(token.Value);
        break;
      case CsvParseTokenType.None:
        throw new InvalidOperationException(
          "Not expecting tokens of type CsvParseTokenType.None");
      default:
        throw new InvalidOperationException(
          $"Unknown token type '{token.Kind}'");
    }
    return !Stopped;
  }
}
