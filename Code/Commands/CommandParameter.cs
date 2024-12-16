using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Breaker;

public struct CommandParameter
{
	public string TypeName { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public bool Optional { get; set; }
	[JsonIgnore] public object DefaultValue { get; set; }
	public TypeDescription GetLibraryType() => TypeLibrary.GetType( TypeName );
	public CommandParameter()
	{

	}

	public CommandParameter( ParameterInfo parameter )
	{
		TypeName = parameter.ParameterType.Name;
		DefaultValue = parameter.DefaultValue;
		Optional = parameter.IsOptional;

		var title = parameter.GetCustomAttribute<TitleAttribute>();
		if (title == null)
		{
			Name = parameter.Name;
		}
		else
		{
			Name = title.Value;
		}
	}
	public object Parse( string argument )
	{
		var type = GetLibraryType();
		Assert.NotNull( type );

		if(string.IsNullOrEmpty(argument) )
		{
			return null;
		}

		if( type.TargetType == typeof(string))
		{
			return argument;
		}
		
		if ( type.IsValueType )
		{
			return Convert.ChangeType( argument, type.TargetType );
		}

		var parseMethod = type.Methods.FirstOrDefault( m => m.Name == "Parse" );
		if ( parseMethod != null )
		{
			var parsed = parseMethod.InvokeWithReturn<object>( null, new object[] { argument, CultureInfo.InvariantCulture } );

			if ( parsed.GetType() == type.TargetType )
				return parsed;
		}

		string typeName = TypeName;
		var parseMethods = TypeLibrary.GetMethodsWithAttribute<ParameterParserAttribute>().Where(p => p.Attribute.TargetType == typeName );
		if(parseMethods.Any())
		{
			foreach((var method, var attrib) in parseMethods)
			{
				var result = method.InvokeWithReturn<object>( null, new object[] { argument } );
				if ( result.GetType() == type.TargetType )
					return result;
			}
		}

		return argument;
	}
}

/// <summary>
/// This method is used to parse a string into a certain type.
/// Takes 1 string as argument and returns an object of the given type.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ParameterParserAttribute : Attribute
{
	public string TargetType { get; set; }

	public ParameterParserAttribute(Type target)
	{
		Assert.NotNull( target );

		TargetType = target.Name;
	}
}
