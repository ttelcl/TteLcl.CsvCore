using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TteLcl.Csv.Core;

namespace TteLcl.Csv;

/// <summary>
/// Combines the basic elements for reading a CSV token source.
/// Use <see cref="Next()"/> to load the next line. Use <see cref="Get"/>
/// (or the indexer) to get the value of a column for a <see cref="ColumnName"/>.
/// Create and cache <see cref="ColumnName"/> instances to use as index (and
/// cache the column index for a column name).
/// </summary>
public class CsvReaderBase
{
  private readonly ITokenSource<CsvParseToken> _tokenSource;
  private readonly CsvReadBuffer _buffer;

  /// <summary>
  /// Create a new <see cref="CsvReaderBase"/> for reading from the given
  /// token source
  /// </summary>
  /// <param name="source">
  /// The raw CSV token source (such as <see cref="CsvStreamParser"/>)
  /// </param>
  /// <param name="columnNamesAreCaseSensitive"></param>
  /// <param name="allowGhosting">
  /// If true, missing columns are bound as ghost columns (always returning "") instead
  /// of causing an exception
  /// </param>
  public CsvReaderBase(
    ITokenSource<CsvParseToken> source,
    bool columnNamesAreCaseSensitive,
    bool allowGhosting)
  {
    _tokenSource = source;
    ColumnNamesAreCaseSensitive = columnNamesAreCaseSensitive;
    AllowGhosting = allowGhosting;
    _buffer = new CsvReadBuffer();
    ColumnCount = 0;
    // Load the first line, which is assumed to be the header line
    // (Headerless CSV files are not supported)
    if(_buffer.FillLine(_tokenSource))
    {
      _buffer.CaptureHeader(ColumnNamesAreCaseSensitive);
      ColumnCount = _buffer.Count;
    }
  }

  /// <summary>
  /// A read-only view on the currently loaded line
  /// </summary>
  public IReadOnlyList<string> Buffer => _buffer;

  /// <summary>
  /// The expected number of columns
  /// </summary>
  public int ColumnCount { get; }

  /// <summary>
  /// True if column names should be treated as case sensitive
  /// </summary>
  public bool ColumnNamesAreCaseSensitive { get; }
  
  /// <summary>
  /// If true, missing columns are bound as ghost columns (always returning "").
  /// If false, column names must exist (missing columns throw an exception).
  /// </summary>
  public bool AllowGhosting { get; }

  /// <summary>
  /// Get the current value of the column. If necessary this binds the column to an index.
  /// </summary>
  /// <param name="cn">
  /// The <see cref="ColumnName"/> identifying the column and caching its index. If no index
  /// is cached yet this call caches it.
  /// </param>
  /// <returns></returns>
  public string Get(ColumnName cn)
  {
    return _buffer[cn, !AllowGhosting];
  }

  /// <summary>
  /// Alias for <see cref="Get"/>
  /// </summary>
  /// <param name="cn">
  /// The <see cref="ColumnName"/> identifying the column and caching its index. If no index
  /// is cached yet this call caches it.
  /// </param>
  /// <returns></returns>
  public string this[ColumnName cn] => Get(cn);

  /// <summary>
  /// Create a new single column accessor for the specified <see cref="ColumnName"/> instance
  /// </summary>
  public CsvColumnReader GetColumn(ColumnName cn)
  {
    return new CsvColumnReader(this, cn);
  }

  /// <summary>
  /// Create a new single column accessor for the specified column name
  /// </summary>
  public CsvColumnReader GetColumn(string columnName)
  {
    return new CsvColumnReader(this, new ColumnName(columnName, ColumnName.UndefinedIndex));
  }

  /// <summary>
  /// Create a new single column accessor for the specified anonymous column at the specified index
  /// </summary>
  public CsvColumnReader GetColumn(int index)
  {
    return new CsvColumnReader(this, new ColumnName("", index));
  }

  /// <summary>
  /// Get the name of a column by index
  /// </summary>
  /// <param name="index">
  /// The index, in the range 0 to <see cref="ColumnCount"/>-1.
  /// </param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  /// <exception cref="KeyNotFoundException"></exception>
  public string GetColumnName(int index)
  {
    var header = _buffer.Header ?? throw new InvalidOperationException("No file header loaded");
    if(!header.TryFind(index, out var ci))
    {
      throw new KeyNotFoundException(
        $"No column name found for column {index}");
    }
    return ci.Name;
  }

  /// <summary>
  /// Load the next line into the read buffer and validate that it has the expected number of columns
  /// </summary>
  /// <returns>
  /// True if a line was loaded, false at EOF
  /// </returns>
  public bool Next()
  {
    var success = _buffer.FillLine(_tokenSource);
    if(success && ColumnCount != _buffer.Count)
    {
      throw new InvalidOperationException(
        $"Inconsistent column count: expecting {ColumnCount} fields per row but got {_buffer.Count}");
    }
    return success;
  }
}
