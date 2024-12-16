using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public class TargetUser : IEnumerable<User>, IParsable<TargetUser>
{
	public static IReadOnlyDictionary<string, Func<List<User>>> TargetSelectors => targetSelectors.AsReadOnly();
	private static Dictionary<string, Func<List<User>>> targetSelectors = new();
	public static void AddSelector(string key, Func<List<User>> selector)
	{
		if(targetSelectors.ContainsKey(key))
		{
			Logging.Error( $"Tried to register target selector with duplicate key {key}!" );
			return;
		}

		targetSelectors.Add(key, selector);
	}
	public static void RemoveSelector(string key)
	{
		if(!targetSelectors.ContainsKey(key))
		{
			Logging.Error( $"Tried to remove non-existing target selector with key {key}" );
			return;
		}

		targetSelectors.Remove(key);
	}
	public static TargetUser Parse( string s, IFormatProvider provider = null )
	{
		var users = Connection.All.Select(User.Get).ToList();
		if(s == "*")
		{
			return new(users);
		}
		else if(s.StartsWith('@'))
		{
			string input = s.Substring( 1 );
			if(targetSelectors.TryGetValue(input, out var selector))
			{
				return new(selector?.Invoke());
			}
		}

		return new(users.Where( u => u.Name.Contains( s ) ).ToList());
	}

	public static bool TryParse( string s, IFormatProvider provider, out TargetUser result )
	{
		try
		{
			result = Parse( s, provider );
			if ( result.Targets.Count > 0 )
				return true;
			else
				return false;
		}
		catch
		{
			result = null;
			return false;
		}
	}
	public List<User> Targets { get; } = new();
	public User this[int index]
	{
		get => Targets[index];
		set => Targets[index] = value;
	}
	public int Count => Targets.Count;
	public bool Single => Count == 1;
	public bool None => Count == 0;
	public TargetUser(List<User> users)
	{
		Targets = users;
	}
	public bool Any() => Targets.Count > 0;
	public IEnumerator<User> GetEnumerator()
	{
		return ((IEnumerable<User>)Targets).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)Targets).GetEnumerator();
	}

	public override string ToString()
	{
		var names = Targets.Select( t => t.Name );
		int count = names.Count();
		if ( count == 0 )
			return "Nobody";
		else if ( count == 1 )
			return names.Single();
		else if ( count <= 5 )
			return string.Join( ", ", names );
		else if ( count == Connection.All.Count )
			return "Everyone";
		else
			return string.Join( ", ", names.Take( 5 ) ) + $" and {count - 5} others";
	}
}
