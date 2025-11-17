using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv.Core;

/// <summary>
/// An object that can write CSV lines
/// </summary>
public interface ICsvWriterBase
{
  /// <summary>
  /// Write the next field
  /// </summary>
  /// <param name="field"></param>
  void WriteField(string field);

  /// <summary>
  /// Complete the current line
  /// </summary>
  void WriteLine();
}

/// <summary>
/// Augments <see cref="ICsvWriterBase"/> to include <see cref="IDisposable"/>
/// </summary>
public interface ICsvWriter: ICsvWriterBase, IDisposable
{
}

/// <summary>
/// Extension methods on <see cref="ICsvWriterBase"/> / <see cref="ICsvWriter"/>
/// </summary>
public static class CsvWriterExtensions
{
  /// <summary>
  /// Append multiple fields at once
  /// </summary>
  /// <param name="w"></param>
  /// <param name="fields"></param>
  public static void WriteFields(this ICsvWriterBase w, IEnumerable<string> fields)
  {
    foreach(var field in fields)
    {
      w.WriteField(field);
    }
  }

  /// <summary>
  /// Write a full line at once (or more precisely: append multiple fields and then
  /// a line break)
  /// </summary>
  /// <param name="w"></param>
  /// <param name="fields"></param>
  public static void WriteLine(this ICsvWriterBase w, IEnumerable<string> fields)
  {
    w.WriteFields(fields);
    w.WriteLine();
  }
}



