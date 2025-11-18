using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TteLcl.Csv;

/// <summary>
/// Tracks mappings between column names and column indexes.
/// </summary>
public class ColumnMapper
{
  private readonly Dictionary<string, int> _columnIndices; // includes ghost columns
  private readonly Dictionary<int, string> _columnNames; // does not contain ghost columns
  private int _nextIndex = 0;

  /// <summary>
  /// Create a new <see cref="ColumnMapper"/>
  /// </summary>
  /// <param name="caseSensitive">
  /// True to treat column names as case sensitive, false to treat them as case insensitive.
  /// </param>
  public ColumnMapper(bool caseSensitive)
  {
    _columnIndices = new Dictionary<string, int>(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
    _columnNames = new Dictionary<int, string>();
  }

  /// <summary>
  /// Create a new <see cref="ColumnMapper"/> and add the column names in the given header
  /// in the order given
  /// </summary>
  /// <param name="caseSensitive">
  /// True to treat column names as case sensitive, false to treat them as case insensitive.
  /// </param>
  /// <param name="header">
  /// Names of columns to create. In case of duplicate names, modified names are used
  /// (in the same way as <see cref="AddIndex(int, string?)"/> does)
  /// </param>
  public ColumnMapper(bool caseSensitive, IEnumerable<string> header)
    : this(caseSensitive)
  {
    var index = 0;
    foreach(var name in header)
    {
      AddIndex(index++, name);
    }
  }

  /// <summary>
  /// Add a new bidirectional column mapping between <paramref name="columnName"/> and <paramref name="columnIndex"/>.
  /// If the exact same mapping already exists this returns immediately.
  /// If the <paramref name="columnName"/> is already mapped to a different index, or if 
  /// <paramref name="columnIndex"/> is not <see cref="ColumnName.GhostIndex"/> and already mapped
  /// to a different name this method throws an exception.
  /// </summary>
  /// <param name="columnName">
  /// The name of the column to map, which must not be in use yet, or already mapped to 
  /// <paramref name="columnIndex"/>.
  /// </param>
  /// <param name="columnIndex">
  /// The column index. This must be a 'real' (non-negative) column index that is not in use yet, 
  /// or be <see cref="ColumnName.GhostIndex"/> (in which case only the name-to-index mapping is tracked)
  /// </param>
  /// <returns>
  /// A <see cref="ColumnIndex"/> representing the new or existing mapping.
  /// </returns>
  /// <exception cref="ArgumentException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
  public ColumnIndex Add(string columnName, int columnIndex)
  {
    if(columnIndex < 0 && columnIndex != ColumnName.GhostIndex)
    {
      throw new ArgumentException(
        $"Cannot add column '{columnName}' as index {columnIndex}: negative indexes (other than the special code {ColumnName.GhostIndex}) are not allowed");
    }
    if(_columnIndices.TryGetValue(columnName, out var existingIndex))
    {
      if(columnIndex == existingIndex)
      {
        return new ColumnIndex(columnName, columnIndex);
      }
      throw new InvalidOperationException(
        $"Cannot add column '{columnName}' as index {columnIndex} because it is already mapped to '{existingIndex}'");
    }
    if(columnIndex == ColumnName.GhostIndex)
    {
      // no mapping in _columnNames in this case, only in _columnIndices
      _columnIndices[columnName] = columnIndex;
      return new ColumnIndex(columnName, columnIndex);
    }
    if(_columnNames.TryGetValue(columnIndex, out var existingName))
    {
      throw new InvalidOperationException(
        $"Cannot map '{columnName}' to {columnIndex} since that index is already in use by '{existingName}'.");
    }
    var nextIndex = columnIndex + 1;
    if(nextIndex > _nextIndex)
    {
      // keep track of highest unused index
      _nextIndex = nextIndex;
    }
    _columnIndices[columnName] = columnIndex;
    _columnNames[columnIndex] = columnName;
    return new ColumnIndex(columnName, columnIndex);
  }

  /// <summary>
  /// Add new column name and assign it the next available column index (or return the existing index if it is
  /// in use already)
  /// </summary>
  /// <param name="columnName"></param>
  /// <returns></returns>
  public ColumnIndex Add(string columnName)
  {
    if(_columnIndices.TryGetValue(columnName, out var existingIndex))
    {
      return new ColumnIndex(columnName, existingIndex);
    }
    return Add(columnName, _nextIndex);
  }

  /// <summary>
  /// Add a <see cref="ColumnName"/>: if it has a binding defined then add it with that
  /// index, otherwise add it as a new column, using the next available index, and bind
  /// it to that index.<para/>
  /// <i>For some use cases <see cref="ColumnName.Bind(ColumnMapper, bool, bool)"/> may be a better fitting API.</i>
  /// </summary>
  /// <param name="cn"></param>
  /// <returns></returns>
  public ColumnIndex Add(ColumnName cn)
  {
    if(cn.IsDefined)
    {
      return Add(cn.Name, cn.Index);
    }
    else
    {
      var ci = Add(cn.Name);
      cn.Bind(ci.Index);
      return ci;
    }
  }

  /// <summary>
  /// If there is no mapping for the given column yet, create one. If <paramref name="nameHint"/>
  /// is still available as a name, use it as the name. Otherwise try constructing names until one is
  /// found that does not cause a name clash. This method is useful to generate unique column names
  /// from a set of candidates that may contain duplicates.
  /// </summary>
  /// <param name="columnIndex">
  /// The index that a name must be found for
  /// </param>
  /// <param name="nameHint">
  /// A hint for the name to use or a basis for generating a name.
  /// </param>
  /// <returns></returns>
  public ColumnIndex AddIndex(int columnIndex, string? nameHint = null)
  {
    nameHint ??= $"Column_{columnIndex+1:D3}";
    if(columnIndex < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(columnIndex),
        $"The column index to add must be non-negative");
    }
    if(_columnNames.TryGetValue(columnIndex, out var existingName))
    {
      // No need to add anything when it is there already
      return new ColumnIndex(existingName, columnIndex);
    }
    if(!_columnIndices.TryGetValue(nameHint, out var existingIndex))
    {
      // no need to generate anything when the hint works fine as is
      return Add(nameHint, columnIndex);
    }
    var genIndex = 1;
    // If the hint looks like something we constructed, cut off the number at the end
    var m = Regex.Match(nameHint, @"^(.*)_(\d+)$");
    if(m.Success)
    {
      nameHint = m.Groups[1].Value;
      var oldIndex = Int32.Parse(m.Groups[2].Value);
      genIndex = oldIndex;
    }
    string newName;
    do
    {
      genIndex++;
      newName = $"{nameHint}_{genIndex:D3}";
    } while(_columnIndices.ContainsKey(newName));
    return Add(newName, columnIndex);
  }

  /// <summary>
  /// Try to find the mapping for the given <paramref name="name"/>. If successful
  /// this returns true and sets <paramref name="ci"/> to the <see cref="ColumnIndex"/>
  /// found. Otherwise this returns false and <paramref name="ci"/> is set to a synthesized
  /// value containing <paramref name="name"/> and <see cref="ColumnName.UndefinedIndex"/>.
  /// </summary>
  /// <param name="name">
  /// The name to find
  /// </param>
  /// <param name="ci">
  /// The <see cref="ColumnIndex"/> that was found, or a synthesized one with index
  /// <see cref="ColumnName.UndefinedIndex"/>.
  /// </param>
  /// <returns></returns>
  public bool TryFind(string name, out ColumnIndex ci)
  {
    if(_columnIndices.TryGetValue(name, out var index))
    {
      ci = new ColumnIndex(name, index);
      return true;
    }
    ci = new ColumnIndex(name, ColumnName.UndefinedIndex);
    return false;
  }

  /// <summary>
  /// Try to find the name-index mapping for the given real (non-negative) index.
  /// Note that this method returns the failure status for negative indexes, including
  /// <see cref="ColumnName.GhostIndex"/>.
  /// </summary>
  /// <param name="index">
  /// The index to find the name for
  /// </param>
  /// <param name="ci">
  /// The <see cref="ColumnIndex"/> found, or a dummy value if not found
  /// </param>
  /// <returns></returns>
  public bool TryFind(int index, out ColumnIndex ci)
  {
    if(_columnNames.TryGetValue(index, out var name))
    {
      ci = new ColumnIndex(name, index);
      return true;
    }
    ci = new ColumnIndex("", index);
    return false;
  }

  /// <summary>
  /// A view on the mapping of column names to column indexes. This mapping does include
  /// ghost columns (columns mapping to index <see cref="ColumnName.GhostIndex"/>)
  /// </summary>
  public IReadOnlyDictionary<string, int> IndexForName => _columnIndices;

  /// <summary>
  /// A view on the mapping of column indexes to column names. This mapping does
  /// NOT include ghost columns (because there may be multiple of them and the 
  /// mapping value is only a single name)
  /// </summary>
  public IReadOnlyDictionary<int, string> NameForIndex => _columnNames;
}
