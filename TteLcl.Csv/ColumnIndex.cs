using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv;

/// <summary>
/// Combines a column name and a column index
/// </summary>
public readonly struct ColumnIndex
{
  private readonly string? _name;

  /// <summary>
  /// Create a new <see cref="ColumnIndex"/>
  /// </summary>
  /// <param name="name">
  /// The name of the column. Use <see cref="String.Empty"/> for anonymous columns.
  /// </param>
  /// <param name="index">
  /// The index of the column
  /// </param>
  public ColumnIndex(string name, int index)
  {
    _name = name;
    Index = index;
  }

  /// <summary>
  /// The column index
  /// </summary>
  public int Index { get; }

  /// <summary>
  /// The name of the column. If this <see cref="ColumnIndex"/> is uninitialized
  /// this reads as <see cref="String.Empty"/>, not null.
  /// </summary>
  public string Name => _name ?? String.Empty;
}
