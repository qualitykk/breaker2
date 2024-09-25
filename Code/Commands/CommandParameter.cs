using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Breaker;

public class CommandParameter
{
	public TypeDescription Type { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public bool Optional { get; set; }
	public object DefaultValue { get; set; }

	public CommandParameter()
	{

	}

	public CommandParameter( ParameterInfo parameter )
	{
		Type = TypeLibrary.GetType(parameter.ParameterType);
		Name = parameter.Name;
		DefaultValue = parameter.DefaultValue;
		Optional = DefaultValue != null;
	}
	public object Parse( string argument )
	{
		Assert.False( string.IsNullOrEmpty(argument) );
		Assert.NotNull( Type );

		var parseMethod = Type.Methods.FirstOrDefault( m => m.Name == "Parse" );
		if ( parseMethod != null )
		{
			try
			{
				var parsed = parseMethod.InvokeWithReturn<object>( null, new object[] { argument } );

				if ( parsed.GetType() == Type.TargetType )
					return parsed;
			}
			catch
			{
				Log.Info( $"Could not parse to type {Type} using builtin interface" );
			}
		}
		else if ( Type.IsValueType )
		{
			return Convert.ChangeType( argument, Type.TargetType );
		}

		var parseMethods = TypeLibrary.GetMethodsWithAttribute<ParameterParserAttribute>().Where(p => p.Attribute.TargetType == Type);
		if(parseMethods.Any())
		{
			foreach((var method, var attrib) in parseMethods)
			{
				var result = method.InvokeWithReturn<object>( null, new object[] { argument } );
				if ( result.GetType() == Type.TargetType )
					return result;
			}
		}

		return null;
	}
}

/// <summary>
/// This method is used to parse a string into a certain type.
/// Takes 1 string as argument and returns an object of the given type.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ParameterParserAttribute : Attribute
{
	public TypeDescription TargetType { get; set; }

	public ParameterParserAttribute(Type target)
	{
		Assert.NotNull( target );

		TargetType = TypeLibrary.GetType(target);

		if(TargetType == null)
		{
			throw new ArgumentException( $"Type {target} is not in type library!" );
		}
	}
}
