using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Reflection;
using System.Text.Json.Serialization;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// A <see cref="DynamicObject"/> that provides extensible properties and methods.
/// This <see cref="DynamicObject"/> stores 'extra' properties in a dictionary or checks the actual properties of the instance.
/// This means this extendo can be subclassed and either native properties or properties from values in the dictionary can be retrieved.
/// Three ways to access properties are available:
///		Directly: any explicitly declared properties are accessible
///		Dynamic: dynamic cast allows access to dictionary and native properties/methods
///		Dictionary: Any of the extended properties are accessible through the <see cref="IDictionary{TKey, TValue}"/> interface
/// </summary>
public class Extendo : DynamicObject, IDynamicMetaObjectProvider
{
	#region Objects and variables

	/// <summary>
	/// Instance of an object passed in.
	/// </summary>
	private object mInstance;

    /// <summary>
    /// Cached type of the instance.
    /// </summary>
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.PublicNestedTypes |
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicEvents)] // allow trimming
    private Type mInstanceType = null!;

	#endregion

	#region Construction

	/// <summary>
	/// This constructor just works off the internal dictionary and any public properties of this object.
	/// </summary>
	[DebuggerStepThrough()]
    public Extendo() : base()
    {
        mInstance = this;
        mInstanceType = typeof(Extendo);
	}

	#endregion

	#region Properties

	/// <summary>
	/// Returns an array of <see cref="PropertyInfo"/> objects.
	/// </summary>
	[JsonIgnore]
    public PropertyInfo[]? InstancePropertyInfo
	{
		get
		{
			if (field == null && mInstanceType != null)
			{
				field = GetInstanceProperties(mInstanceType);
			}
			return field;
		}
	}

	/// <summary>
	/// Returns a dictionary of properties.
	/// </summary>
	[JsonIgnore]
	public Dictionary<string, object?> Properties { get; } = [];

	/// <summary>
	/// Convenience method that provides a string indexer to the Properties collection AND the strongly typed properties of the passed in object, by name.
	/// <code>
	/// // dynamic
	/// exp["Address"] = "112 nowhere lane"; 
	/// // strong
	/// var name = exp["StronglyTypedProperty"] as string;
	/// </code>
	/// </summary>
	/// <remarks>
	/// The getter checks the Properties dictionary first and then looks in PropertyInfo[] for properties.
	/// The setter checks the instance properties before checking the Properties dictionary.
	/// </remarks>
	/// <param name="key">The name of the property</param> 
	/// <returns>An object</returns>
	[JsonIgnore]
	public object? this[string key]
	{
		get
		{
			try
			{
				return Properties[key]; // try to get from properties collection first
			}
			catch (KeyNotFoundException)
			{
				object? result; // try reflection on instanceType
				if (GetProperty(mInstance, key, out result))
				{
					return result;
				}
				throw; // doesn't exist
			}
		}
		set
		{
			if (Properties.ContainsKey(key))
			{
				Properties[key] = value;
				return;
			}

			var miArray = mInstanceType.GetMember(key, BindingFlags.Public | BindingFlags.GetProperty); // check instance for existance of type first
			if (miArray != null && miArray.Length > 0)
			{
				SetProperty(mInstance, key, value);
			}
			else
			{
				Properties[key] = value;
			}
		}
	}

	#endregion

	#region Public methods and functions

	/// <summary>
	/// Checks whether a property exists in the Property collection or as a property on the instance.
	/// </summary>
	/// <param name="propertyName">The name of the property</param>
	/// <param name="includeInstanceProperties">If true, include fields of the Extendo based object itself in the search</param>
	/// <returns>A <see cref="bool"/>, <see langword="true"/> if a property exists with the given name</returns>
	public bool Contains(string propertyName, bool includeInstanceProperties = false)
	{
		var res = Properties.ContainsKey(propertyName);

		if (res)
		{
			return true;
		}
		if (includeInstanceProperties && InstancePropertyInfo != null)
		{
			foreach (var prop in InstancePropertyInfo)
			{
				if (prop.Name == propertyName)
				{
					return true;
				}
			}
		}
		return false;
	}

    /// <summary>
	/// Returns a new <see cref="Extendo"/> passing in an existing object instance to 'extend'.        
	/// </summary>
	/// <param name="instance">Instance of the object to extend</param>
	public static Extendo Create<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.PublicNestedTypes |
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicEvents)] T>(T instance) where T : class, new()
    {
        var xt = new Extendo
        {
            mInstance = instance,
            mInstanceType = typeof(T)
        };
        return xt;
    }

    /// <summary>
    /// Tries to retrieve a member by name first from instance properties followed by the collection entries.
    /// </summary>
    /// <param name="binder">The <see cref="GetMemberBinder"/></param>
    /// <param name="result">The member, if found; else <see langword="null"/></param>
    /// <returns>Boolean, true if the operation was successful</returns>
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
	{
		if (Properties.TryGetValue(binder.Name, out var value)) // first check the Properties collection for member
		{
			result = value;
			return true;
		}
		if (mInstance != null) // next check for Public properties via Reflection
		{
			try
			{
				return GetProperty(mInstance, binder.Name, out result);
			}
			catch { }
		}
		result = null; // failed to retrieve a property
		return false;
	}

	/// <summary>
	/// Dynamic invocation method. Currently allows only for Reflection based operation (no ability to add methods dynamically).
	/// </summary>
	/// <param name="binder">The <see cref="InvokeMemberBinder"/></param>
	/// <param name="args">Arguments to use when calling the member</param>
	/// <param name="result">The result of thye call</param>
	/// <returns>Boolean, true if the operation was successful</returns>
	public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
	{
		if (mInstance != null)
		{
			try
			{
				if (InvokeMethod(mInstance, binder.Name, args, out result)) // check instance passed in for methods to invoke
				{
					return true;
				}
			}
			catch { }
		}
		result = null;
		return false;
	}

	/// <summary>
	/// Property setter implementation that tries to set the value on the passed in instance first then into this object.
	/// </summary>
	/// <param name="binder">The <see cref="SetMemberBinder"/></param>
	/// <param name="value">The value to set</param>
	/// <returns>Boolean, true if the operation was successful</returns>
	public override bool TrySetMember(SetMemberBinder binder, object? value)
	{
		if (mInstance != null) // first check to see if there's a native property to set
		{
			try
			{
				var result = SetProperty(mInstance, binder.Name, value);
				if (result)
				{
					return true;
				}
			}
			catch { }
		}
		Properties[binder.Name] = value; // no match - set on or add to inner dictionary
		return true;
	}

    #endregion

    #region Private methods and functions

    /// <summary>
    /// Returns all properties.
    /// </summary>
    /// <param name="includeInstanceProperties">If true, include fields of the Exptendo based object itself</param>
    /// <returns>A collection of <see cref="KeyValuePair{String, Object}"/>s</returns>
    public IEnumerable<KeyValuePair<string, object?>> GetProperties(bool includeInstanceProperties = false)
	{
		if (includeInstanceProperties && InstancePropertyInfo != null)
		{
			foreach (var prop in InstancePropertyInfo)
			{
				yield return new KeyValuePair<string, object?>(prop.Name, prop.GetValue(mInstance, null));
			}
		}
		foreach (var key in Properties.Keys)
		{
			yield return new KeyValuePair<string, object?>(key, Properties[key]);
		}
	}

	/// <summary>
	/// Reflection Helper method to retrieve a property.
	/// </summary>
	/// <param name="instance">The instance to retrieve the property from (use null to use this instance)</param>
	/// <param name="name">The name of the property</param>
	/// <param name="result">The value</param>
	/// <returns>Boolean, true if the operation was successful</returns>
	protected bool GetProperty(object? instance, string name, out object? result)
	{
		instance ??= this;

		var miArray = mInstanceType.GetMember(name, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
		if (miArray != null && miArray.Length > 0)
		{
			var mi = miArray[0];
			if (mi.MemberType == MemberTypes.Property)
			{
				result = ((PropertyInfo)mi).GetValue(instance, null);
				return true;
			}
		}
		result = null;
		return false;
	}

	/// <summary>
	/// Reflection helper method to invoke a method.
	/// </summary>
	/// <param name="instance">The instance on which to invoke the method</param>
	/// <param name="name">The name of the method</param>
	/// <param name="args">The arguments</param>
	/// <param name="result">The result of invocation</param>
	/// <returns>Boolean, true if the operation was successful</returns>
	protected bool InvokeMethod(object? instance, string name, object?[]? args, out object? result)
	{
		instance ??= this;

		// Look at the instanceType
		var miArray = mInstanceType.GetMember(name,
								BindingFlags.InvokeMethod |
								BindingFlags.Public | BindingFlags.Instance);

		if (miArray != null && miArray.Length > 0)
		{
			var mi = miArray[0] as MethodInfo;
			
            if (mi != null)
            {
                result = mi.Invoke(instance, args);
                return true;
            }
		}
		result = null;
		return false;
	}

	/// <summary>
	/// Reflection helper method to set a property value.
	/// </summary>
	/// <param name="instance">The Extendo instance</param>
	/// <param name="name">The name of the property</param>
	/// <param name="value">The value to set</param>
	/// <returns>Boolean, true if the operation was successful</returns>
	protected bool SetProperty(object? instance, string name, object? value)
	{
		instance ??= this;

		var miArray = mInstanceType.GetMember(name, BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
		if (miArray != null && miArray.Length > 0)
		{
			var mi = miArray[0];
			if (mi.MemberType == MemberTypes.Property)
			{
				((PropertyInfo)mi).SetValue(instance, value, null);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Returns an array of <see cref="PropertyInfo"/> objects.
	/// </summary>
	private static PropertyInfo[] GetInstanceProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
	{
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
	}

	#endregion
}