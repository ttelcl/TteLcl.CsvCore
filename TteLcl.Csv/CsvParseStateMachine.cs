using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TteLcl.Csv.Core;

namespace TteLcl.Csv;

/// <summary>
/// The states of the CSV parser
/// </summary>
internal enum CsvParseState
{
  /// <summary>
  /// Something went wrong
  /// </summary>
  Error,

  /// <summary>
  /// Initial state. Also the state at the start of any line
  /// </summary>
  Start,

  /// <summary>
  /// Parsing complete
  /// </summary>
  Done,

  /// <summary>
  /// We just got a CR ('\r') character and now expect a LF ('\n').
  /// We did not see any field content in the current line yet,
  /// so this line will be empty.
  /// </summary>
  ExpectLineFeed1,

  /// <summary>
  /// We just got a CR ('\r') character and now expect a LF ('\n').
  /// We saw at least one field character in the current line.
  /// If the character before that CR was a separator, this line ends
  /// with an empty field.
  /// </summary>
  ExpectLineFeed2,

  /// <summary>
  /// We are at the start of a new field, but not the start of a new line.
  /// </summary>
  NextField,

  /// <summary>
  /// We are parsing a plain, unquoted, field
  /// </summary>
  PlainField,

  /// <summary>
  /// We are parsing a quoted field
  /// </summary>
  QuotedField,

  /// <summary>
  /// We found a quote character ('"') in a quoted field but don't know yet
  /// if that is the end of the quoted field or an embedded literal '"'
  /// character.
  /// </summary>
  QuoteQuote,
}

internal class CsvParseStateMachine
{
  private readonly StringBuilder _fieldBuilder;

  public CsvParseStateMachine(char separator = ',')
  {
    _fieldBuilder = new StringBuilder(1024); // increased initial capacity
    State = CsvParseState.Start;
    Separator = separator;
    if(separator == QUOTE || separator == CR || separator == LF || separator == EOF)
    {
      throw new ArgumentOutOfRangeException(
        nameof(separator), "Unsupported separator character: Double quotes and line breaks can not be used as CSV separator");
    }
  }

  public CsvParseState State { get; private set; }

  public char Separator { get; }

  /// <summary>
  /// Parse the next character
  /// </summary>
  /// <param name="ch">
  /// The character to parse or '\0' to parse End Of File (and complete the parsing process)
  /// </param>
  /// <returns>
  /// The emission of the parse update (containing field result, if any, as well as end of line and
  /// end of file status)
  /// </returns>
  /// <exception cref="NotImplementedException"></exception>
  public CsvParseToken ParseCharacter(char ch)
  {
    return State switch {
      CsvParseState.Error => ParseError(ch),
      CsvParseState.Start => ParseStart(ch),
      CsvParseState.Done => ParseDone(ch),
      CsvParseState.ExpectLineFeed1 => ParseExpectLineFeed1(ch),
      CsvParseState.ExpectLineFeed2 => ParseExpectLineFeed2(ch),
      CsvParseState.NextField => ParseNextField(ch),
      CsvParseState.PlainField => ParsePlainField(ch),
      CsvParseState.QuotedField => ParseQuotedField(ch),
      CsvParseState.QuoteQuote => ParseQuoteQuote(ch),
      _ => throw new InvalidOperationException(
        $"Invalid state '{State}'"),
    };
  }

  private CsvParseToken ParseError(char ch)
  {
    throw new InvalidOperationException(
      $"CSV parsing already has failed fatally, cannot parse further");
  }

  private CsvParseToken ParseStart(char ch)
  {
    switch(ch)
    {
      case EOF:
        State = CsvParseState.Done;
        return CsvParseToken.EndOfFile();
      case CR:
        State = CsvParseState.ExpectLineFeed1;
        return CsvParseToken.None;
      case LF:
        State = CsvParseState.Start;
        return CsvParseToken.EndOfLine();
      case QUOTE:
        State = CsvParseState.QuotedField;
        return CsvParseToken.None;
      default:
        if(ch == Separator)
        {
          State = CsvParseState.NextField;
          return CsvParseToken.Field("");
        }
        else
        {
          State = CsvParseState.PlainField;
          return Store(ch);
        }
    }
  }

  private CsvParseToken ParseDone(char ch)
  {
    throw new InvalidOperationException(
      $"CSV parsing already has completed, not expecting more input");
  }

  private CsvParseToken ParseExpectLineFeed1(char ch)
  {
    switch(ch)
    {
      case EOF:
        State = CsvParseState.Done;
        return CsvParseToken.EndOfFile();
      case CR:
        // Buggy input, but harmless
        State = CsvParseState.ExpectLineFeed1;
        return CsvParseToken.None;
      case LF:
        State = CsvParseState.Start;
        return CsvParseToken.EndOfLine();
      default:
        State = CsvParseState.Error;
        throw new InvalidOperationException(
          $"Expecting LF after CR, but got '{ch}'");
    }
  }

  private CsvParseToken ParseExpectLineFeed2(char ch)
  {
    switch(ch)
    {
      case EOF:
        State = CsvParseState.Done;
        return CsvParseToken.EndOfFile(ExtractField());
      case CR:
        // Buggy input, but harmless
        State = CsvParseState.ExpectLineFeed2;
        return CsvParseToken.None;
      case LF:
        State = CsvParseState.Start;
        return CsvParseToken.EndOfLine(ExtractField());
      default:
        State = CsvParseState.Error;
        throw new InvalidOperationException(
          $"Expecting LF after CR, but got '{ch}'");
    }
  }

  private CsvParseToken ParseNextField(char ch)
  {
    switch(ch)
    {
      case EOF:
        State = CsvParseState.Done;
        return CsvParseToken.EndOfFile("");
      case CR:
        State = CsvParseState.ExpectLineFeed2;
        return CsvParseToken.None;
      case LF:
        State = CsvParseState.Start;
        return CsvParseToken.EndOfLine("");
      case QUOTE:
        State = CsvParseState.QuotedField;
        return CsvParseToken.None;
      default:
        if(ch == Separator)
        {
          State = CsvParseState.NextField;
          return CsvParseToken.Field("");
        }
        else
        {
          State = CsvParseState.PlainField;
          return Store(ch);
        }
    }
  }

  private CsvParseToken ParsePlainField(char ch)
  {
    switch(ch)
    {
      case EOF:
        State = CsvParseState.Done;
        return CsvParseToken.EndOfFile(ExtractField());
      case CR:
        State = CsvParseState.ExpectLineFeed2;
        return CsvParseToken.None;
      case LF:
        State = CsvParseState.Start;
        return CsvParseToken.EndOfLine(ExtractField());
      case QUOTE:
        State = CsvParseState.Error;
        throw new InvalidOperationException(
          $"Found '\"' inside an unquoted field (so quoting the field is obligatory). Field content so far: {_fieldBuilder.ToString()}");
      default:
        if(ch == Separator)
        {
          State = CsvParseState.NextField;
          return CsvParseToken.Field(ExtractField());
        }
        else
        {
          State = CsvParseState.PlainField;
          return Store(ch);
        }
    }
  }

  private CsvParseToken ParseQuotedField(char ch)
  {
    switch(ch)
    {
      case EOF:
        State = CsvParseState.Error;
        throw new InvalidOperationException(
          "Unexpected EOF inside a quoted field");
      case QUOTE:
        State = CsvParseState.QuoteQuote;
        return CsvParseToken.None;
      default:
        State = CsvParseState.QuotedField;
        return Store(ch);
    }
  }

  private CsvParseToken ParseQuoteQuote(char ch)
  {
    switch(ch)
    {
      case EOF:
        State = CsvParseState.Done;
        return CsvParseToken.EndOfFile(ExtractField());
      case CR:
        State = CsvParseState.ExpectLineFeed2;
        return CsvParseToken.None;
      case LF:
        State = CsvParseState.Start;
        return CsvParseToken.EndOfLine(ExtractField());
      case QUOTE:
        State = CsvParseState.QuotedField;
        return Store('\"');
      default:
        if(ch == Separator)
        {
          State = CsvParseState.NextField;
          return CsvParseToken.Field(ExtractField());
        }
        else
        {
          State = CsvParseState.Error;
          throw new InvalidOperationException(
            $"Expecting separator, end of line, end of file or another '\"' after a '\"' inside a quoted string, but found '{ch}'");
        }
    }
  }

  /// <summary>
  /// Get the value from the field buffer and empty it
  /// </summary>
  /// <returns></returns>
  private string ExtractField()
  {
    var field = _fieldBuilder.ToString();
    _fieldBuilder.Clear();
    return field;
  }

  private CsvParseToken Store(char ch)
  {
    _fieldBuilder.Append(ch);
    return CsvParseToken.None;
  }

  private const char EOF = '\0';

  private const char CR = '\r';

  private const char LF = '\n';

  private const char QUOTE = '"';
}
