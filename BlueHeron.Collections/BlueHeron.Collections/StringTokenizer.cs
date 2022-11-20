using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BlueHeron.Collections
{
	/// <summary>
	/// Tokenizes a string into a collection of values.
	/// </summary>
	public partial class StringTokenizer
	{
		#region Objects and variables

		private const char _COMMA = ',';
		private const char _SEMICOLON = ';';
		private const char _SINGLEQUOTE = '\'';

		private const string errEmptyToken = "Empty token error occurred.";
		private const string errExtraDataEncountered = "Extra data encountered.";
		private const string errMissingEndQuote = "Missing end quote error occurred.";

		private char mArgSeparator;
		private int mCharIndex;
		private int mCurTokenIndex;
		private int mCurTokenLength;
		private int mLength;
		private char mQuoteChar;
		private bool mSeparatorFound;
		private string mString;

		/// <summary>
		/// The <see cref="Regex"/> to use to split text into words.
		/// </summary>
		[GeneratedRegex("(\\W+)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
		private static partial Regex RegexTokenizeWords();

		#endregion

		#region Construction

		/// <summary> 
		/// Creates a StringTokenizer with the given <see cref="IFormatProvider"/>.
		/// If the IFormatProvider is null, use the thread's IFormatProvider info. 
		/// ',' is used as the list separator, unless it's the same as the decimal separator. If it *is*, then e.g. "23,5" can't be determined to be one number or two. In this case, use ";" as the separator.
		/// </summary> 
		/// <param name="str">The string to tokenize</param>
		/// <param name="formatProvider">The <see cref="IFormatProvider"/> which controls this tokenization</param> 
		public StringTokenizer(string str, IFormatProvider formatProvider)
		{
			var numberSeparator = GetNumericListSeparator(formatProvider);
			Initialize(str, _SINGLEQUOTE, numberSeparator);
		}

		/// <summary>
		/// Initializes the StringTokenizer with the string to tokenize, the character which represents quotes and the list separator.
		/// </summary> 
		/// <param name="str">The string to tokenize</param>
		/// <param name="quoteChar">The quote character</param> 
		/// <param name="separator">The list separator</param> 
		public StringTokenizer(string str, char quoteChar, char separator)
		{
			Initialize(str, quoteChar, separator);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Determines whether a valid separator could be found in the string.
		/// </summary>
		public bool IsSeparatorFound => mSeparatorFound;

		#endregion

		#region Public methods and functions

		/// <summary>
		/// Returns the mCurrent token if present, else null.
		/// </summary>
		/// <returns>A <see cref="string"/>, representing the mCurrent token</returns>
		public string GetCurrentToken()
		{            
			if (mCurTokenIndex < 0) // if no current token, return null 
			{
				return null;
			}
			return mString.Substring(mCurTokenIndex, mCurTokenLength);
		}

		/// <summary>
		/// Get the numeric list separator for a given IFormatProvider.
		/// Separator is a comma [,] if the decimal separator is not a comma, else a semicolon [;] is used.
		/// </summary>
		/// <param name="provider">The <see cref="IFormatProvider"/> to use</param>
		/// <returns>The separator character</returns>
		public static char GetNumericListSeparator(IFormatProvider provider)
		{
			var numericSeparator = _COMMA;
			var numberFormat = NumberFormatInfo.GetInstance(provider); // Get the NumberFormatInfo out of the provider, if possible. If the IFormatProvider doesn't not contain a NumberFormatInfo, then this method returns the mCurrent culture's NumberFormatInfo

			if (numberFormat.NumberDecimalSeparator.Length > 0 && numericSeparator == numberFormat.NumberDecimalSeparator[0]) // If the decimal separator is the same as the mList separator, use the ";"
			{
				numericSeparator = _SEMICOLON;
			}
			return numericSeparator;
		}

		/// <summary> 
		/// Advances to the next token.
		/// </summary>
		/// <returns>True if next token was found, false if at end of string</returns>
		public bool NextToken()
		{
			return NextToken(false);
		}

		/// <summary> 
		/// Advances to the next token, throwing an exception if not present.
		/// </summary>
		/// <returns>The next token found</returns>
		public string NextTokenRequired()
		{
			if (!NextToken(false))
			{
				throw new InvalidOperationException(nameof(NextTokenRequired));
			}
			return GetCurrentToken();
		}

		/// <summary>
		/// Advances to the next token, throwing an exception if not present.
		/// </summary> 
		/// <param name="allowQuotedToken">Allow quoted tokens</param>
		/// <returns>The next token found</returns>
		public string NextTokenRequired(bool allowQuotedToken)
		{
			if (!NextToken(allowQuotedToken))
			{
				throw new InvalidOperationException(nameof(NextTokenRequired));
			}
			return GetCurrentToken();
		}

		/// <summary>
		/// Advances to the next token.
		/// </summary>
		/// <param name="allowQuotedToken">Allow quoted tokens</param>
		/// <returns>True if next token was found, false if at end of string</returns> 
		public bool NextToken(bool allowQuotedToken)
		{
			return NextToken(allowQuotedToken, mArgSeparator); // use the mCurrent separator character
		}

		/// <summary>
		/// Advances to the next token. A separator character can be specified, which overrides the one previously set. 
		/// </summary>
		/// <param name="allowQuotedToken">Allow quoted tokens</param>
		/// <param name="separator">The separator character</param>
		/// <returns>True if next token was found, false if at end of string</returns> 
		public bool NextToken(bool allowQuotedToken, char separator)
		{
			mCurTokenIndex = -1; // reset the mCurTokenIndex 
			mSeparatorFound = false; // reset

			if (mCharIndex >= mLength) // If the end of the string has beeen reached, just return false
			{
				return false;
			}

			var currentChar = mString[mCharIndex];
			var quoteCount = 0; // initialize the quoteCount 

			if (allowQuotedToken && currentChar == mQuoteChar) // If quoted tokens are allowed and this token begins with a quote, set up the quote count and skip the initial quote
			{
				quoteCount++; // increment quote count
				++mCharIndex; // move to next character 
			}

			var newTokenIndex = mCharIndex;
			var newTokenLength = 0;

			// loop until end of string is hit or a , or whitespace is hit. If at end of string just return false
			while (mCharIndex < mLength)
			{
				currentChar = mString[mCharIndex];

				if (quoteCount > 0) // if there is Quote count and this is a quote, decrement the quoteCount
				{
					if (currentChar == mQuoteChar) // if anything but a quote char: move on
					{
						--quoteCount;

						if (0 == quoteCount) // if at zero which it always should for now break out of the loop
						{
							++mCharIndex; // move past the quote
							break;
						}
					}
				}
				else if (char.IsWhiteSpace(currentChar) || currentChar == separator)
				{
					if (currentChar == separator)
					{
						mSeparatorFound = true;
					}
					break;
				}

				++mCharIndex;
				++newTokenLength;
			}

			if (quoteCount > 0) // if quoteCount isn't zero we hit the end of the string before the ending quote
			{
				throw new InvalidOperationException(errMissingEndQuote);
			}

			ScanToNextToken(separator); // move so at the start of the nextToken for next call 

			mCurTokenIndex = newTokenIndex; // finally made it, update the mCurrentToken values
			mCurTokenLength = newTokenLength;

			if (mCurTokenLength < 1)
			{
				throw new InvalidOperationException(errEmptyToken);
			}
			return true;
		}

		/// <summary>
		/// Simply tokenizes a string into words (where word is the regex definition of a word).
		/// </summary>
		/// <param name="input">The string to tokenize</param>
		/// <returns>An <see cref="IEnumerable{String}"/></returns>
		public static IEnumerable<string> Split(string input)
		{
			return RegexTokenizeWords().Split(input).AsEnumerable();
		}

		#endregion

		#region Private methods and functions

		/// <summary>
		/// Initialize the StringTokenizer with the string to tokenize, the char which represents quotes and the list separator.
		/// </summary> 
		/// <param name="str">The string to tokenize</param>
		/// <param name="quoteChar">The quote char</param> 
		/// <param name="separator">The list separator</param> 
		private void Initialize(string str, char quoteChar, char separator)
		{
			mString = str;
			mLength = str == null ? 0 : str.Length;
			mCurTokenIndex = -1;
			mQuoteChar = quoteChar;
			mArgSeparator = separator;

			while (mCharIndex < mLength) // immediately forward past any whitespace so NextToken() logic always starts on the first character of the next token
			{
				if (!char.IsWhiteSpace(mString, mCharIndex))
				{
					break;
				}
				++mCharIndex;
			}
		}

		/// <summary>
		/// Helper to move the char index to the next token or to the end of the string.
		/// </summary>
		/// <param name="separator">The separator character</param>
		/// <exception cref="InvalidOperationException">Empty token or extra data encountered</exception>
		private void ScanToNextToken(char separator)
		{
			if (mCharIndex < mLength) // If already at end of the string don't bother
			{
				var currentChar = mString[mCharIndex];

				if (!(currentChar == separator) && !char.IsWhiteSpace(currentChar)) // check that the currentChar is a space or the separator. If not, there is an error. This may happen in the quote case where the char after the quotes string isn't a char
				{
					throw new InvalidOperationException(errExtraDataEncountered);
				}
 
				var argSepCount = 0;
				while (mCharIndex < mLength) // loop until a character is hit that isn't an argument separator or whitespace. TODO: if more than one arg is set throw an exception
				{
					currentChar = mString[mCharIndex];

					if (currentChar == separator)
					{
						mSeparatorFound = true;
						++argSepCount;
						mCharIndex++;

						if (argSepCount > 1)
						{
							throw new InvalidOperationException(errEmptyToken);
						}
					}
					else if (char.IsWhiteSpace(currentChar))
					{
						++mCharIndex;
					}
					else
					{
						break;
					}
				}

				if (argSepCount > 0 && mCharIndex >= mLength) // if there was a separator char then the end of string shoudn't be reached or it means there was a separator but there isn't a separator argument
				{
					throw new InvalidOperationException(errEmptyToken);
				}
			}
		}

		#endregion
	}
}