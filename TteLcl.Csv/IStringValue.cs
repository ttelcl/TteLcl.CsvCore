using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv;

/// <summary>
/// An object that can read a string value from somewhere
/// </summary>
public interface IStringValue
{
  /// <summary>
  /// Read the current value
  /// </summary>
  string Get();
}
