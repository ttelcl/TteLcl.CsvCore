using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TteLcl.Csv.Core;

namespace TteLcl.Csv;

/// <summary>
/// Augments <see cref="CsvReaderBase"/> with stream / file management
/// </summary>
public class CsvReader: CsvReaderBase, IDisposable
{
  private readonly CsvStreamParser _parser;
  private bool _disposed;

  /// <summary>
  /// Create a new <see cref="CsvReader"/> wrapping a pre-created <see cref="CsvStreamParser"/>
  /// </summary>
  /// <param name="streamParser"></param>
  /// <param name="columnNamesAreCaseSensitive"></param>
  /// <param name="allowGhosting"></param>
  public CsvReader(
    CsvStreamParser streamParser,
    bool columnNamesAreCaseSensitive,
    bool allowGhosting)
    : base(streamParser.ParseTokenSource, columnNamesAreCaseSensitive, allowGhosting)
  {
    _parser = streamParser;
  }

  /// <summary>
  /// Create a new <see cref="CsvReader"/> to wrap the the given <see cref="TextReader"/> 
  /// </summary>
  /// <param name="reader"></param>
  /// <param name="columnNamesAreCaseSensitive"></param>
  /// <param name="allowGhosting"></param>
  /// <param name="separator"></param>
  public CsvReader(
    TextReader reader,
    bool columnNamesAreCaseSensitive,
    bool allowGhosting,
    char separator = ',')
    : this(new CsvStreamParser(reader, separator), columnNamesAreCaseSensitive, allowGhosting)
  {
  }

  /// <summary>
  /// Create a new <see cref="CsvReader"/> reading from the given file
  /// </summary>
  /// <param name="fileName"></param>
  /// <param name="columnNamesAreCaseSensitive"></param>
  /// <param name="allowGhosting"></param>
  /// <param name="separator"></param>
  public CsvReader(
    string fileName,
    bool columnNamesAreCaseSensitive,
    bool allowGhosting,
    char separator = ',')
    : this(new CsvStreamParser(fileName, separator), columnNamesAreCaseSensitive, allowGhosting)
  {
  }

  /// <summary>
  /// Implements the dispose pattern
  /// </summary>
  protected virtual void Dispose(bool disposing)
  {
    if(!_disposed)
    {
      _disposed=true;
      if(disposing)
      {
        _parser.Dispose();
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
