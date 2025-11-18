using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv;

/// <summary>
/// Combines a <see cref="ColumnName"/> and a <see cref="CsvReaderBase"/> to provide
/// a compact API for reading a single column in a CSV reader
/// </summary>
public class CsvColumnReader: IStringValue
{
  /// <summary>
  /// Create a new <see cref="CsvColumnReader"/>. Consider using <see cref="CsvReaderBase.GetColumn(string)"/>,
  /// <see cref="CsvReaderBase.GetColumn(ColumnName)"/> or <see cref="CsvReaderBase.GetColumn(int)"/> instead.
  /// </summary>
  public CsvColumnReader(
    CsvReaderBase reader,
    ColumnName name)
  {
    Reader = reader;
    ColName = name;
  }

  /// <summary>
  /// The target reader
  /// </summary>
  public CsvReaderBase Reader { get; }

  /// <summary>
  /// The column name (and index cache)
  /// </summary>
  public ColumnName ColName { get; }

  /// <summary>
  /// Get the value of this column in the reader
  /// </summary>
  /// <returns></returns>
  public string Get() => Reader.Get(ColName);

}
