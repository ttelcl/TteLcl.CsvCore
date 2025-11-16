/*
 * (c) 2022  ttelcl / ttelcl
 * (based on my original dating back to 2014) 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv.Core;

/// <summary>
/// CSV parser state machine
/// </summary>
internal class ParserState
{
  internal enum State
  {
    Error,
    Done,
    Main,
    FieldPending,
    UnquotedMain,
    WaitForLf,
    DoubleQuotedMain,
    DoubleQuotedEscape,
    SingleQuotedMain,
    SingleQuotedEscape,
  }

  private State _state;
  private readonly StringBuilder _field;
  private readonly List<string> _line;
  private char _separator;
  private bool _skipEmptyFields;
  private bool _trimSpace;

  public ParserState(
    char separator = ',',
    bool skipEmptyFields = false,
    bool trimSpace = true,
    bool supportSingleQuote = false)
  {
    _state = State.Main;
    _field = new StringBuilder();
    _line = new List<string>();
    _skipEmptyFields = skipEmptyFields;
    _trimSpace = trimSpace;
    SupportSingleQuote = supportSingleQuote;
    SetSeparator(separator);
  }

  public bool SupportSingleQuote { get; set; }

  /// <summary>
  /// Change the separator character. Valid separator characters are 
  /// the charcters in the string ",; :|/*\#+_"
  /// </summary>
  public void SetSeparator(char ch)
  {
    TestValidSeparator(ch);
    _separator = ch;
  }

  internal static void TestValidSeparator(char separator)
  {
    if(",; :|/*\\#+_".IndexOf(separator) < 0)
    {
      throw new ArgumentException("Not a valid separator character");
    }
  }

  /// <summary>
  /// Parse a character. The character '\0' is interpreted as EOF
  /// </summary>
  /// <param name="ch">
  /// The character to parse or '\0' to indicate EOF
  /// </param>
  /// <returns>
  /// null to indicate more data is needed or no new output was produced
  /// on EOF, or a string array containing the fields of the line parsed.
  /// The result can only be non-null if the input is '\n' or '\0'.
  /// </returns>
  public IReadOnlyList<string>? ParseChar(char ch)
  {
    //Console.WriteLine("@{0}:{1}",_state,(ch<=' ')?String.Format("0x{0:2X}",(int)ch):ch.ToString());
    switch(_state)
    {
      default:
        throw new InvalidOperationException("Invalid state");
      case State.Error:
        throw new InvalidOperationException("Attempt to recover after previous error");
      case State.Done:
        _state = State.Error;
        throw new InvalidOperationException("Attempt to continue parsing after EOF");
      case State.Main: // start or after a field completion
        {
          Trace.Assert(_field.Length == 0);
          switch(ch)
          {
            case '\0':
              _state = State.Done;
              IReadOnlyList<string>? ret = null;
              if(_line.Count > 0)
              {
                ret = FinishLine();
              }
              return ret;
            case '\r':
              _state = State.WaitForLf;
              return null;
            case '\n':
              _state = State.Error;
              throw new InvalidOperationException("Got LF without CR");
            case '"':
              _state = State.DoubleQuotedMain;
              return null;
            case '\'':
              if(SupportSingleQuote)
              {
                _state = State.SingleQuotedMain;
              }
              else
              {
                // same as default
                _state = State.UnquotedMain;
                _field.Append(ch);
              }
              return null;
            default:
              if(ch == _separator)
              {
                // push an empty field and tag as another field expected
                PushField(false);
                _state = State.FieldPending;
                return null;
              }
              else
              {
                _state = State.UnquotedMain;
                _field.Append(ch);
                return null;
              }
          }
        }
      case State.FieldPending: // after a comma indicating next field
        {
          Trace.Assert(_field.Length == 0);
          switch(ch)
          {
            case '\0':
              _state = State.Done;
              PushField(false);
              return FinishLine();
            case '\r':
              PushField(false);
              _state = State.WaitForLf;
              return null;
            case '\n':
              _state = State.Error;
              throw new InvalidOperationException("Got LF without CR");
            case '"':
              _state = State.DoubleQuotedMain;
              return null;
            case '\'':
              if(SupportSingleQuote)
              {
                _state = State.SingleQuotedMain;
              }
              else
              {
                // same as default
                _state = State.UnquotedMain;
                _field.Append(ch);
              }
              return null;
            default:
              if(ch == _separator)
              {
                // push an empty field and tag as another field expected
                PushField(false);
                _state = State.FieldPending;
                return null;
              }
              else
              {
                _state = State.UnquotedMain;
                _field.Append(ch);
                return null;
              }
          }
        }
      case State.WaitForLf:
        {
          Trace.Assert(_field.Length == 0);
          switch(ch)
          {
            case '\n':
              _state = State.Main;
              return FinishLine();
            default:
              throw new InvalidOperationException("Expecting LF after CR");
          }
        }
      case State.UnquotedMain:
        {
          Trace.Assert(_field.Length > 0);
          switch(ch)
          {
            case '\0':
              _state = State.Done;
              PushField(false);
              return FinishLine();
            case '\r':
              PushField(false);
              _state = State.WaitForLf;
              return null;
            case '\n':
              _state = State.Error;
              throw new InvalidOperationException("Got LF without CR");
            case '"':
              _state = State.Error;
              throw new InvalidOperationException("Found \" in an unquoted field");
            // BEWARE! For compatibility *do* allow single quote here!
            default:
              if(ch == _separator)
              {
                PushField(false);
                _state = State.FieldPending;
                return null;
              }
              else
              {
                _state = State.UnquotedMain;
                _field.Append(ch);
                return null;
              }
          }
        }
      case State.DoubleQuotedMain:
        {
          switch(ch)
          {
            case '\0':
              _state = State.Error;
              throw new InvalidOperationException("Found EOF in an quoted field");
            case '"':
              // need next character to find out what this means
              _state = State.DoubleQuotedEscape;
              return null;
            default:
              _state = State.DoubleQuotedMain;
              _field.Append(ch);
              return null;
          }
        }
      case State.SingleQuotedMain:
        {
          if(!SupportSingleQuote)
          {
            throw new InvalidOperationException(
              "Internal error - SingleQuoting was not enabled yet triggered");
          }
          switch(ch)
          {
            case '\0':
              _state = State.Error;
              throw new InvalidOperationException("Found EOF in an quoted field");
            case '\'':
              // need next character to find out what this means
              _state = State.SingleQuotedEscape;
              return null;
            default:
              _state = State.SingleQuotedMain;
              _field.Append(ch);
              return null;
          }
        }
      case State.DoubleQuotedEscape:
        {
          switch(ch)
          {
            case '\0':
              _state = State.Done;
              PushField(true);
              return FinishLine();
            case '\r':
              PushField(true);
              _state = State.WaitForLf;
              return null;
            case '\n':
              _state = State.Error;
              throw new InvalidOperationException("Got LF without CR");
            case '"':
              // it was an escaped quote character
              _field.Append('"');
              _state = State.DoubleQuotedMain;
              return null;
            default:
              if(ch == _separator)
              {
                PushField(true);
                _state = State.FieldPending;
                return null;
              }
              else
              {
                _state = State.Error;
                throw new InvalidOperationException(
                  "Expecting '\"', ',', CRLF or EOF after embedded double quote character ");
              }
          }
        }
      case State.SingleQuotedEscape:
        {
          if(!SupportSingleQuote)
          {
            throw new InvalidOperationException(
              "Internal error - SingleQuoting was not enabled yet triggered");
          }
          switch(ch)
          {
            case '\0':
              _state = State.Done;
              PushField(true);
              return FinishLine();
            case '\r':
              PushField(true);
              _state = State.WaitForLf;
              return null;
            case '\n':
              _state = State.Error;
              throw new InvalidOperationException("Got LF without CR");
            case '\'':
              // it was an escaped quote character
              _field.Append('\'');
              _state = State.SingleQuotedMain;
              return null;
            default:
              if(ch == _separator)
              {
                PushField(true);
                _state = State.FieldPending;
                return null;
              }
              else
              {
                _state = State.Error;
                throw new InvalidOperationException(
                  "Expecting '\'', ',', CRLF or EOF after embedded single quote character");
              }
          }
        }
    }
  }

  public IReadOnlyList<string>? ParseEof()
  {
    return ParseChar('\0');
  }

  private void PushField(bool quoted)
  {
    var value = _field.ToString();
    if(!quoted && _trimSpace && !_skipEmptyFields)
    {
      value = value.Trim();
    }
    if(!_skipEmptyFields || value.Length > 0 || quoted)
    {
      _line.Add(value);
    }
    _field.Clear();
  }

  private IReadOnlyList<string> FinishLine()
  {
    Trace.Assert(_field.Length == 0);
    var ret = _line.ToArray();
    _line.Clear();
    return ret;
  }
}
