using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv;

/// <summary>
/// A writer buffer for a single column in a single row
/// </summary>
public class CsvWriteCell
{
  private string? _value;

  internal CsvWriteCell(
    string name,
    int index)
  {
    Name = name;
    Index = index;
  }

  /// <summary>
  /// The name of the column
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// The index in the column
  /// </summary>
  public int Index { get; }

  /// <summary>
  /// Get the current value, throwing an exception if no value has been assigned
  /// since the last reset.
  /// </summary>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public string Get() => _value ?? throw new InvalidOperationException(
    $"No value assigned to column '{Name}' (index {Index}) yet.");

  /// <summary>
  /// Set a value
  /// </summary>
  /// <param name="value"></param>
  public void Set(string value)
  {
    _value = value;
  }

  /// <summary>
  /// Clear the value to its "not set" state.
  /// </summary>
  public void Clear()
  {
    _value = null;
  }
}
