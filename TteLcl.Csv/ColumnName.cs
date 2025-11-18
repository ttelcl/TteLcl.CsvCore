using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv;

/// <summary>
/// A column name and a latent index that can be bound
/// </summary>
public class ColumnName
{

  /// <summary>
  /// Manually create a new <see cref="ColumnName"/>
  /// </summary>
  /// <param name="name">
  /// The name of the column
  /// </param>
  /// <param name="index">
  /// The initial real or virtual index of this column (default <see cref="UndefinedIndex"/>)
  /// </param>
  public ColumnName(string name, int index = UndefinedIndex)
  {
    Name = name;
    Index = index;
  }

  /// <summary>
  /// Create a new unbound <see cref="ColumnName"/>
  /// </summary>
  /// <param name="name">
  /// The name of the column
  /// </param>
  /// <returns></returns>
  public static ColumnName CreateUnboundColumn(string name) => new ColumnName(name, UndefinedIndex);

  /// <summary>
  /// Create a new ghost <see cref="ColumnName"/>
  /// </summary>
  /// <param name="name">
  /// The name of the column
  /// </param>
  /// <returns></returns>
  public static ColumnName CreateGhostColumn(string name) => new ColumnName(name, GhostIndex);

  /// <summary>
  /// Create a new bound <see cref="ColumnName"/>
  /// </summary>
  /// <param name="name">
  /// The name of the column
  /// </param>
  /// <param name="index">
  /// The column index. While this is expected to be a real (non-negative) index, that is not
  /// verified (and thus the returned column may in fact not be "bound").
  /// </param>
  /// <returns></returns>
  public static ColumnName CreateBoundColumn(string name, int index) => new ColumnName(name, index);

  /// <summary>
  /// Create a new bound <see cref="ColumnName"/> based on an existing <see cref="AsColumnIndex"/>.
  /// </summary>
  /// <param name="ci">
  /// The column index to get the name and index from. While this method expects a real (non-negative)
  /// index, that is not verified (and thus the returned column may in fact not be "bound")
  /// </param>
  /// <returns></returns>
  public static ColumnName CreateBoundColumn(ColumnIndex ci) => new ColumnName(ci.Name, ci.Index);

  /// <summary>
  /// The name of the column
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Either the actual non-negative index of the column, or <see cref="UndefinedIndex"/> (-1)
  /// to indicate an unbound column, or <see cref="GhostIndex"/> (-2) to indicate a
  /// "ghost column" that always reads as an empty string.
  /// </summary>
  public int Index { get; private set; }

  /// <summary>
  /// True if the index is bound to a real column or the ghost column
  /// </summary>
  public bool IsDefined => Index >= 0 || Index == GhostIndex;

  /// <summary>
  /// This <see cref="ColumnName"/>'s current <see cref="ColumnIndex"/> equivalent.
  /// This is also the return value of the implicit conversion operator.
  /// </summary>
  public ColumnIndex AsColumnIndex => new ColumnIndex(Name, Index);

  /// <summary>
  /// Conversion operator, enabling using <see cref="ColumnName"/> where <see cref="ColumnIndex"/>
  /// is expected.
  /// </summary>
  /// <param name="cn"></param>
  public static implicit operator ColumnIndex(ColumnName cn)
  {
    return cn.AsColumnIndex;
  }

  /// <summary>
  /// Get the value for this column from <paramref name="readBuffer"/>.
  /// If not done so yet, this will bind this <see cref="ColumnName"/> to the header
  /// captured in the buffer.
  /// </summary>
  /// <param name="readBuffer"></param>
  /// <returns></returns>
  public string Get(CsvReadBuffer readBuffer)
  {
    return readBuffer[this];
  }

  /// <summary>
  /// Bind or re-bind this <see cref="ColumnName"/> to a column Index
  /// </summary>
  /// <param name="index">
  /// The new index value
  /// </param>
  /// <param name="allowRebind">
  /// If false (default) "re-binding" is disallowed. Binding is considered to be
  /// rebinding if the new index is different from the existing one and the existing
  /// index is anything other than <see cref="UndefinedIndex"/>.
  /// </param>
  /// <exception cref="InvalidOperationException"></exception>
  public void Bind(int index, bool allowRebind = false)
  {
    if(!allowRebind && Index != UndefinedIndex && index != Index)
    {
      throw new InvalidOperationException(
        $"Cannot re-bind column '{Name}' to {index} (it is already bound to {Index})");
    }
    Index = index;
  }

  /// <summary>
  /// Bind this name to the name found in <paramref name="mapper"/>.
  /// If not found, the behaviour depends on <paramref name="mustExist"/>: if true,
  /// an exception is thrown. If false, this name is bound as ghost column and
  /// added as such to the mapper.<para/>
  /// <i>For some use cases <see cref="ColumnMapper.Add(ColumnName)"/> may be a better fitting API.</i>
  /// </summary>
  /// <param name="mapper">
  /// The mapper to find the column index in
  /// </param>
  /// <param name="mustExist">
  /// If true and this name is not found in <paramref name="mapper"/>, an exception is thrown.
  /// If false and this name is not found in <paramref name="mapper"/>, this column is turned into
  /// a ghost column and added as such to the mapper
  /// </param>
  /// <param name="allowRebind">
  /// If false, changing an existing binding is disallowed.
  /// </param>
  /// <exception cref="InvalidOperationException"></exception>
  public void Bind(ColumnMapper mapper, bool mustExist, bool allowRebind = false)
  {
    if(mapper.TryFind(Name, out var ci))
    {
      Bind(ci.Index, allowRebind);
      return;
    }
    if(mustExist)
    {
      throw new InvalidOperationException(
        $"Cannot bind '{Name}': name not found in the given column mapper");
    }
    Bind(GhostIndex, allowRebind);
    mapper.Add(Name, GhostIndex);
  }

  /// <summary>
  /// The index placeholder used to indicate that this column name is not yet
  /// bound to a column index.
  /// </summary>
  public const int UndefinedIndex = -1;

  /// <summary>
  /// The index placeholder used to indicate that this column is a virtual column
  /// that always reads as an empty string and can not be set.
  /// </summary>
  public const int GhostIndex = -2;


}
