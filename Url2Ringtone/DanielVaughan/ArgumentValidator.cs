#region File and License Information
/*
<File>
	<Copyright>Copyright © 2010, Daniel Vaughan. All rights reserved.</Copyright>
	<License>
		Redistribution and use in source and binary forms, with or without
		modification, are permitted provided that the following conditions are met:
			* Redistributions of source code must retain the above copyright
			  notice, this list of conditions and the following disclaimer.
			* Redistributions in binary form must reproduce the above copyright
			  notice, this list of conditions and the following disclaimer in the
			  documentation and/or other materials provided with the distribution.
			* Neither the name of the <organization> nor the
			  names of its contributors may be used to endorse or promote products
			  derived from this software without specific prior written permission.

		THIS SOFTWARE IS PROVIDED BY Daniel Vaughan ''AS IS'' AND ANY
		EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
		WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
		DISCLAIMED. IN NO EVENT SHALL Daniel Vaughan BE LIABLE FOR ANY
		DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
		(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
		LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
		ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
		(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
		SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	</License>
	<Owner Name="Daniel Vaughan" Email="dbvaughan@gmail.com"/>
	<CreationDate>2009-03-22 14:02:43Z</CreationDate>
</File>
*/
#endregion

using System;

namespace DanielVaughan
{
	/// <summary>
	/// Utility class for validating method parameters.
	/// </summary>
	public static class ArgumentValidator
	{
		/// <summary>
		/// Ensures the specified value is not null.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to test.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>The specified value.</returns>
		/// <exception cref="ArgumentNullException">Occurs if the specified value 
		/// is <code>null</code>.</exception>
		public static T AssertNotNull<T>(T value, string parameterName) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}

			return value;
		}

		/// <summary>
		/// Ensures the specified value is not <code>null</code> 
		/// or empty (a zero length string).
		/// </summary>
		/// <param name="value">The value to test.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>The specified value.</returns>
		/// <exception cref="ArgumentNullException">Occurs if the specified value 
		/// is <code>null</code> or empty (a zero length string).</exception>
		public static string AssertNotNullOrEmpty(string value, string parameterName)
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}

			if (value.Length < 1)
			{
				/* TODO: Make localizable resource. */
				throw new ArgumentException(
					"Parameter should not be an empty string.", parameterName);
			}

			return value;
		}

		/// <summary>
		/// Ensures the specified value is not <code>null</code> 
		/// or white space.
		/// </summary>
		/// <param name="value">The value to test.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>The specified value.</returns>
		/// <exception cref="ArgumentNullException">Occurs if the specified value 
		/// is <code>null</code> or consists of only white space.</exception>
		public static string AssertNotNullOrWhiteSpace(string value, string parameterName)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException(
					"Parameter should not be null or white space.", parameterName);
			}

			return value;
		}

		/// <summary>
		/// Ensures the specified value is not <code>null</code> 
		/// and that it is of the specified type.
		/// </summary>
		/// <param name="value">The value to test.</param> 
		/// <param name="parameterName">The name of the parameter.</param>
		/// <returns>The value to test.</returns>
		/// <exception cref="ArgumentNullException">Occurs if the specified value 
		/// is <code>null</code> or of type not assignable 
		/// from the specified type.</exception>
		/// <example>
		/// public DoSomething(object message)
		/// {
		/// 	this.message = ArgumentValidator.AssertNotNullAndOfType&lt;string&gt;(
		///							message, "message");	
		/// }
		/// </example>
		public static T AssertNotNullAndOfType<T>(
			object value, string parameterName) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			var result = value as T;
			if (result == null)
			{
				throw new ArgumentException(string.Format(
					"Expected argument of type {0}, but was {1}",
					typeof(T), value.GetType()),
					parameterName);
			}
			return result;
		}

		/* TODO: [DV] Comment. */
		public static int AssertGreaterThan(
			int comparisonValue, int value, string parameterName)
		{
			if (value <= comparisonValue)
			{
				/* TODO: Make localizable resource. */
				throw new ArgumentOutOfRangeException(
					"Parameter should be greater than "
					+ comparisonValue, parameterName);
			}
			return value;
		}

		/* TODO: [DV] Comment. */
		public static double AssertGreaterThan(
			double comparisonValue, double value, string parameterName)
		{
			if (value <= comparisonValue)
			{
				/* TODO: Make localizable resource. */
				throw new ArgumentOutOfRangeException(
					"Parameter should be greater than "
					+ comparisonValue, parameterName);
			}
			return value;
		}

		/* TODO: [DV] Comment. */
		public static long AssertGreaterThan(
			long comparisonValue, long value, string parameterName)
		{
			if (value <= comparisonValue)
			{
				/* TODO: Make localizable resource. */
				throw new ArgumentOutOfRangeException(
					"Parameter should be greater than "
					+ comparisonValue, parameterName);
			}
			return value;
		}

		/* TODO: [DV] Comment. */
		public static int AssertGreaterThanOrEqualTo(
			int comparisonValue, int value, string parameterName)
		{
			if (value < comparisonValue)
			{
				/* TODO: Make localizable resource. */
				throw new ArgumentOutOfRangeException(
					"Parameter should be greater than or equal to "
					+ comparisonValue, parameterName);
			}
			return value;
		}

		/* TODO: [DV] Comment. */
		public static double AssertGreaterThanOrEqualTo(
			double comparisonValue, double value, string parameterName)
		{
			if (value < comparisonValue)
			{
				/* TODO: Make localizable resource. */
				throw new ArgumentOutOfRangeException(
					"Parameter should be greater than or equal to "
					+ comparisonValue, parameterName);
			}
			return value;
		}

		/* TODO: [DV] Comment. */
		public static long AssertGreaterThanOrEqualTo(
			long comparisonValue, long value, string parameterName)
		{
			if (value < comparisonValue)
			{
				/* TODO: Make localizable resource. */
				throw new ArgumentOutOfRangeException(
					"Parameter should be greater than "
					+ comparisonValue, parameterName);
			}
			return value;
		}

		/* TODO: [DV] Comment. */
		public static double AssertLessThan(
			double comparisonValue, double value, string parameterName)
		{
			if (value >= comparisonValue)
			{
				/* TODO: Make localizable resource. */
				throw new ArgumentOutOfRangeException(
					"Parameter should be less than "
					+ comparisonValue, parameterName);
			}
			return value;
		}

	}
}

