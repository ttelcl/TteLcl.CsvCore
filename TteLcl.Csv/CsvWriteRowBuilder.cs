using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv;

/// <summary>
/// A builder for a CsvWriteRow. You can create new cells in this builder,
/// but not to the <see cref="CsvWriteRow"/> it builds.
/// </summary>
public class CsvWriteRowBuilder
{
  private readonly List<CsvWriteCell> _cells;

  /// <summary>
  /// Create a new empty <see cref="CsvWriteRowBuilder"/>
  /// </summary>
  public CsvWriteRowBuilder()
  {
    _cells = [];
  }

  /// <summary>
  /// Add a new cell to this builder and return it. The cell name is not checked for
  /// uniqueness. The caller should hold on the new cell to call its
  /// <see cref="CsvWriteCell.Set(string)"/> method later on.
  /// </summary>
  /// <param name="name">
  /// The column / cell name. This value is not checked for uniqueness. If you want to
  /// create multiple columns with the same name you can do so.
  /// </param>
  /// <returns></returns>
  public CsvWriteCell AddCell(string name)
  {
    var cell = new CsvWriteCell(name, _cells.Count);
    _cells.Add(cell);
    return cell;
  }

  /// <summary>
  /// Return a new <see cref="CsvWriteRow"/> containing a snapshot of the cells
  /// added so far using <see cref="AddCell(string)"/>
  /// </summary>
  /// <returns></returns>
  public CsvWriteRow Build()
  {
    return new CsvWriteRow(_cells);
  }

  /// <summary>
  /// Clear the set a cells.
  /// </summary>
  public void Clear()
  {
    _cells.Clear();
  }
}
