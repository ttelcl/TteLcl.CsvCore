using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TteLcl.Csv.Core;

namespace TteLcl.Csv;

/// <summary>
/// A row of <see cref="CsvWriteCell"/>s for buffering an entire CSV row.
/// The cells in this buffer are fixed at construction time. Use a
/// <see cref="CsvWriteRowBuilder"/> to incrementally add cells / columns,
/// and build an immutable <see cref="CsvWriteRow"/> from it (immutable
/// in the sense that column names and indexes are locked in; the cell
/// values are of course mutable)
/// </summary>
public class CsvWriteRow: IReadOnlyList<CsvWriteCell>
{
  private readonly List<CsvWriteCell> _cells;

  /// <summary>
  /// Create a new <see cref="CsvWriteRow"/> containing the specified cells.
  /// Do not call this directly; use <see cref="CsvWriteRowBuilder.Build"/> instead.
  /// </summary>
  /// <param name="cells"></param>
  internal CsvWriteRow(
    IEnumerable<CsvWriteCell> cells)
  {
    _cells = new List<CsvWriteCell>(cells);
  }

  /// <inheritdoc/>
  public CsvWriteCell this[int index] => _cells[index];

  /// <inheritdoc/>
  public int Count => _cells.Count;

  /// <inheritdoc/>
  public IEnumerator<CsvWriteCell> GetEnumerator()
  {
    return _cells.GetEnumerator();
  }

  /// <inheritdoc/>
  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  /// <summary>
  /// Clear all values in this row, preparing to receive the data for the next line
  /// </summary>
  public void ClearValues()
  {
    foreach(var cell in _cells)
    {
      cell.Clear();
    }
  }

  /// <summary>
  /// Writes all values as a new row and clear the values
  /// </summary>
  /// <param name="writer">
  /// The CSV Writer to write to
  /// </param>
  public void WriteValuesTo(ICsvWriter writer)
  {
    writer.WriteLine(_cells.Select(cwc => cwc.Get()));
    ClearValues();
  }

  /// <summary>
  /// Writes all cell names as a new row and ensures that all values are cleared.
  /// </summary>
  /// <param name="writer">
  /// The CSV Writer to write to
  /// </param>
  public void WriteNamesTo(ICsvWriter writer)
  {
    writer.WriteLine(_cells.Select(cwc => cwc.Name));
    ClearValues();
  }
}
