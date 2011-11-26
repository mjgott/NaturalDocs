﻿/* 
 * Struct: GregValure.NaturalDocs.Engine.Symbols.ParameterString
 * ____________________________________________________________________________
 * 
 * A struct encapsulating parameters from a symbol, which is a normalized way of representing the parenthetical
 * section of a code element or topic, such as "(int, int)" in "PackageA.PackageB.FunctionC(int, int)".  When
 * generated from prototypes, ParameterStrings only store the types of each parameter, not the names or default 
 * values.
 */

// This file is part of Natural Docs, which is Copyright © 2003-2011 Greg Valure.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;
using System.Collections.Generic;
using GregValure.NaturalDocs.Engine.Tokenization;


namespace GregValure.NaturalDocs.Engine.Symbols
	{
	public struct ParameterString : IComparable
		{
		
		// Group: Constants
		// __________________________________________________________________________

		/* Constant: SeparatorChar
		 * The character used to separate parameter strings.
		 */
		public const char SeparatorChar = SymbolString.SeparatorChar;



		// Group: Functions
		// __________________________________________________________________________
		
		
		/* Function: ParameterString
		 */
		private ParameterString (string newParameterString)
			{
			parameterString = newParameterString;
			}


		/* Function: FromParameterString
		 * Creates a ParameterString from the passed string which originally came from another ParameterString object.  This skips 
		 * the normalization stage because it should already be in the proper format.  Only use this when retrieving ParameterStrings
		 * that were stored as plain text in a database or other data file.
		 */
		public static ParameterString FromParameterString (string parameterString)
			{
			return new ParameterString(parameterString);
			}


		/* Function: FromPlainTextStrings
		 * Creates a ParameterString from a list of individual plain text parameter strings.  The strings should be the type of each
		 * parameter only and not include the name or default value.
		 */
		public static ParameterString FromPlainTextStrings (IList<string> parameterStrings)
			{
			System.Text.StringBuilder output = new System.Text.StringBuilder();

			for (int i = 0; i < parameterStrings.Count; i++)
				{
				if (i > 0)
					{  output.Append(SeparatorChar);  }

				NormalizeAndAppend(parameterStrings[i], output);
				}

			return new ParameterString(output.ToString());
			}


		/* Function: FromPlainTextString
		 * Creates a ParameterString from a single plain text string, such as "(int, int)".  This is useful when this is the only way
		 * to get a parameter string, such as when it appears in a topic title, but should not be relied on if avoidable, such as 
		 * when generating them from a prototype.  In the case of prototypes it should be parsed into a <ParsedPrototype> and 
		 * the parameter types fed into <FromPlainTextStrings()>.
		 */
		public static ParameterString FromPlainTextString (string input)
			{
			input = input.Trim();

			if (input.Length < 2 || input[0] != '(' || input[input.Length - 1] != ')')
				{  throw new Exception("Plain text parameter strings must be surrounded by parethesis.");  }

			input = input.Substring(1, input.Length - 2);  // Strip surrounding parenthesis.
			input = input.Trim();

			if (input == "")
				{  return new ParameterString();  }

			System.Text.StringBuilder output = new System.Text.StringBuilder(input.Length);

			// Ignore separators appearing within braces.  We've already filtered out the surrounding parenthesis and we shouldn't
			// have to worry about quotes because we should only have types, not default values.
			Collections.SafeStack<char> braces = new Collections.SafeStack<char>();

			int startParam = 0;
			int index = input.IndexOfAny(bracesAndParamSeparators);

			while (index != -1)
				{
				char character = input[index];

				if (character == '(' || character == '[' || character == '{' || character == '<')
					{  
					braces.Push(character);
					}

				else if ( (character == ')' && braces.Peek() == '(') ||
							  (character == ']' && braces.Peek() == '[') ||
							  (character == '}' && braces.Peek() == '{') ||
							  (character == '>' && braces.Peek() == '<') )
					{
					braces.Pop();
					}
				else if ((character == ',' || character == ';') && braces.Count == 0)
					{
					NormalizeAndAppend(input.Substring(startParam, index - startParam), output);
					output.Append(SeparatorChar);
					startParam = index + 1;
					}

				index = input.IndexOfAny(bracesAndParamSeparators, index + 1);
				}

			NormalizeAndAppend(input.Substring(startParam), output);

			return new ParameterString(output.ToString());
			}

			
			
		// Group: Operators
		// __________________________________________________________________________
		
		
		/* operator: operator string
		 * A cast operator to covert the params to a string.
		 */
		public static implicit operator string (ParameterString p)
			{
			return p.parameterString;
			}
						
		/* Operator: operator ==
		 */
		public static bool operator== (ParameterString a, object b)
			{
			// We need to make the operator compare against object intead of another ParameterString in order to support
			// directly comparing against null.
			return a.Equals(b);
			}

		/* Operator: operator !=
		 */
		public static bool operator!= (ParameterString a, object b)
			{
			return !(a.Equals(b));
			}

		/* Function: ToString
		 * Returns the SymbolString as a string.
		 */
		public override string ToString ()
			{
			return parameterString;
			}
			
		/* Function: GetHashCode
		 */
		public override int GetHashCode ()
			{
			return parameterString.GetHashCode();
			}

		/* Function: Equals
		 */
		public override bool Equals (object other)
			{
			if (other == null)
				{  return (parameterString == null);  }
			else if (other is ParameterString)
				{  return (parameterString == ((ParameterString)other).parameterString);  }
			else if (other is string)
				{  return (parameterString == (string)other);  }
			else
				{  return false;  }
			}
			
		/* Function: CompareTo
		 */
		public int CompareTo (object other)
			{
			return parameterString.CompareTo(other);
			}
		
			
		
		// Group: Private Functions
		// __________________________________________________________________________


		/* Function: NormalizeAndAppend
		 * 
		 * Normalizes the individual parameter and appends it to the passed StringBuilder.  It does not append a <SeparatorChar>,
		 * that must be done by the calling code.
		 * 
		 *		- Applies canonical normalization to Unicode (FormC).
		 *		- Removes all existing instances of <SeparatorChar>.
		 *		- Whitespace is removed unless it is between two text characters as defined by <Tokenizer.FundamentalTypeOf()>.
		 *		- Whitespace not removed is condensed into a single space.
		 *		- Unlike <SymbolString>, does NOT replace the common package separator symbols (. :: ->) with <SeparatorChar>.
		 */
		private static void NormalizeAndAppend (string parameter, System.Text.StringBuilder output)
			{
			if (parameter == null)
				{  return;  }

			parameter = parameter.Trim();

			if (parameter == "")
				{  return;  }
				
			parameter = parameter.Normalize(System.Text.NormalizationForm.FormC);  // Canonical decomposition and recombination

			int nextChar = parameter.IndexOfAny(separatorCharAndWhitespace);
			int index = 0;
			
			// Set to true if we just passed whitespace, since we only want to add it to the normalized string if it's between two 
			// text characters.  We also want to condense multiple characters to a single space.
			bool addWhitespace = false;
			
			while (nextChar != -1)
				{
				if (nextChar > index)
					{
					if (addWhitespace && output.Length > 0 &&
						Tokenizer.FundamentalTypeOf( output[output.Length - 1] ) == FundamentalType.Text &&
						Tokenizer.FundamentalTypeOf( parameter[index] ) == FundamentalType.Text)
						{
						output.Append(' ');
						}
					
					output.Append(parameter, index, nextChar - index);
					addWhitespace = false;
					}

				if (parameter[nextChar] == SeparatorChar)
					{
					// Ignore, doesn't affect anything.
					index = nextChar + 1;
					}				
				else if (parameter[nextChar] == ' ' || parameter[nextChar] == '\t')
					{  
					addWhitespace = true;
					index = nextChar + 1;  
					}

				nextChar = parameter.IndexOfAny(separatorCharAndWhitespace, index);
				}
			
			if (index < parameter.Length)
				{
				if (addWhitespace && output.Length > 0 &&
					Tokenizer.FundamentalTypeOf( output[output.Length - 1] ) == FundamentalType.Text &&
					Tokenizer.FundamentalTypeOf( parameter[index] ) == FundamentalType.Text)
					{
					output.Append(' ');
					}
				
				output.Append(parameter, index, parameter.Length - index);
				}
			}



		// Group: Variables
		// __________________________________________________________________________
		
		
		/* string: parameterString
		 * The parameter string, _always_ in normalized form.
		 */
		private string parameterString;
	
		/* var: separatorCharAndWhitespace
		 * An array containing the whitespace characters and <SeparatorChar>.
		 */
		static private char[] separatorCharAndWhitespace = new char[] { ' ', '\t', SeparatorChar };

		/* var: bracesAndParamSeparators
		 * An array containing all forms of braces, comma, and semicolon.
		 */
		static private char[] bracesAndParamSeparators = new char[] { '(', '[', '{', '<', ')', ']', '}', '>', ',', ';' };

		}
	}