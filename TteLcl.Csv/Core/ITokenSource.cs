using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TteLcl.Csv.Core;

/// <summary>
/// A source of tokens of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITokenSource<out T> where T: struct
{
  /// <summary>
  /// Get the next token
  /// </summary>
  /// <returns></returns>
  T NextToken();
}
