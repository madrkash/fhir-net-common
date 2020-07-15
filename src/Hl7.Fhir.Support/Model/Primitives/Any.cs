/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Support.Utility;
using Hl7.Fhir.Utility;
using System;

namespace Hl7.Fhir.Model.Primitives
{
    public abstract class Any
    {      
        /// <summary>
        /// Returns the concrete subclass of Any that is used to represent the
        /// type given in parmameter <paramref name="name"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryGetSystemTypeByName(string name, out Type? result)
        {
            result = get();
            return result != null;

            Type? get() =>
                name switch
                {
                    "Any" => typeof(Any),
                    "Boolean" => typeof(Boolean),
                    "Code" => typeof(Code),
                    "Concept" => typeof(Concept),
                    "Decimal" => typeof(Decimal),
                    "Integer" => typeof(Integer),
                    "Long" => typeof(Long),
                    "Date" => typeof(PartialDate),
                    "DateTime" => typeof(PartialDateTime),
                    "Ratio" => typeof(Ratio),
                    "Time" => typeof(PartialTime),
                    "Quantity" => typeof(Quantity),
                    "String" => typeof(String),
                    "Void" => typeof(void),
                    _ => null,
                };
        }

        public static object Parse(string value, Type primitiveType)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            return TryParse(value, primitiveType, out var result) ? result! :
                throw new FormatException($"Input string '{value}' was not in a correct format for type '{primitiveType}'.");
        }

        public static bool TryParse(string value, Type primitiveType, out object? parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!typeof(Any).IsAssignableFrom(primitiveType)) throw new ArgumentException($"Must be a subclass of {nameof(Any)}.", nameof(primitiveType));

            bool success;
            (success, parsed) = parse();
            return success;

            (bool success, object? p) parse()
            {
                if (primitiveType == typeof(Boolean))
                    return (Boolean.TryParse(value, out var p), p?.Value);
                else if (primitiveType == typeof(Code))
                    return (Code.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Concept))
                    return (success: Concept.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Decimal))
                    return (success: Decimal.TryParse(value, out var p), p?.Value);
                else if (primitiveType == typeof(Integer))
                    return (success: Integer.TryParse(value, out var p), p?.Value);
                else if (primitiveType == typeof(Long))
                    return (success: Long.TryParse(value, out var p), p?.Value);
                else if (primitiveType == typeof(PartialDate))
                    return (success: PartialDate.TryParse(value, out var p), p);
                else if (primitiveType == typeof(PartialDateTime))
                    return (success: PartialDateTime.TryParse(value, out var p), p);
                else if (primitiveType == typeof(PartialTime))
                    return (success: PartialTime.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Ratio))
                    return (success: Ratio.TryParse(value, out var p), p);
                else if (primitiveType == typeof(Quantity))
                    return (success: Quantity.TryParse(value, out var p), p);
                else if (primitiveType == typeof(String))
                    return (success: String.TryParse(value, out var p), p?.Value);
                else
                    return (false, null);
            }
        }

        internal static (bool, T) DoConvert<T>(Func<T> parser)
        {
            try
            {
                return (true, parser());
            }
            catch (Exception)
            {
                return (false, default);
            }
        }

        /// <summary>
        /// Try to convert a .NET instance to a Cql/FhirPath Any-based type.
        /// </summary>
        public static bool TryConvert(object value, out Any? primitiveValue)
        {
            primitiveValue = conv();
            return primitiveValue != null;

            Any? conv()
            {
                // NOTE: Keep Any.TryConvertToSystemValue, TypeSpecifier.TryGetNativeType and TypeSpecifier.ForNativeType in sync
                if (value is Any a)
                    return a;
                else if (value is bool b)
                    return new Boolean(b);
                else if (value is string s)
                    return new String(s);
                else if (value is char c)
                    return new String(new string(c, 1));
                else if (value is int || value is short || value is ushort || value is uint)
                    return new Integer(System.Convert.ToInt32(value));
                else if (value is long || value is ulong)
                    return new Long(System.Convert.ToInt64(value));
                else if (value is DateTimeOffset dto)
                    return PartialDateTime.FromDateTimeOffset(dto);
                else if (value is float || value is double || value is decimal)
                    return new Decimal(System.Convert.ToDecimal(value));
                else if (value is Enum en)
                    return new String(en.GetLiteral());
                else if (value is Uri u)
                    return new String(u.OriginalString);
                else
                    return null;
            }
        }

        /// <summary>
        /// Converts a .NET instance to a Cql/FhirPath Any-based type.
        /// </summary>
        public static Any? Convert(object value)
        {
            if (value == null) return null;

            if (TryConvert(value, out var result))
                return result;
            else
                throw new NotSupportedException($"There is no known Cql/FhirPath type corresponding to the .NET type {value.GetType().Name} of this instance (with value '{value}').");
        }

        // some utility methods shared by the subclasses

        protected static ArgumentException NotSameTypeComparison(object me, object them) => 
            new ArgumentException($"Cannot compare {me} (of type {me.GetType()}) to {them} (of type {them.GetType()}), because the types differ.");

        protected static readonly ArgumentNullException ArgNullException = new ArgumentNullException();

        protected static Result<T> propagateNull<T>(object obj, Func<T> a) => obj is null ? 
            (Result<T>)new Fail<T>(ArgNullException) : new Ok<T>(a());
    }


    public interface ICqlEquatable
    {
        bool? IsEqualTo(Any other);
        bool IsEquivalentTo(Any other);
    } 

    public interface ICqlOrderable
    { 
        int? CompareTo(Any other);
    }
}
