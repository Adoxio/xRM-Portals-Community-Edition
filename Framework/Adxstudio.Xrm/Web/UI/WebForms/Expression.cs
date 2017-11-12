/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Resources;
using Adxstudio.Xrm.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	public abstract class Expression : IEnumerable<Expression>
	{
		public IEnumerable<Expression> Operands { get; private set; }

		public virtual string Operator
		{
			get { return null; }
		}

		public delegate string ExpressionAction(Expression expression, IDictionary<Type, ExpressionAction> map);

		protected Expression()
		{
		}

		protected Expression(string name, object value)
			: this(new LeftLiteralExpression(name.Trim()), new RightLiteralExpression(value))
		{
		}

		protected Expression(params Expression[] operands)
		{
			Operands = new List<Expression>(operands);
		}

		public static LiteralExpression Literal(object value)
		{
			return new LiteralExpression(value);
		}

		public static OrExpression operator |(Expression left, Expression right)
		{
			return new OrExpression(left, right);
		}

		public static OrExpression Or(params Expression[] operands)
		{
			return new OrExpression(operands);
		}

		public static AndExpression operator &(Expression left, Expression right)
		{
			return new AndExpression(left, right);
		}

		public static AndExpression And(params Expression[] operands)
		{
			return new AndExpression(operands);
		}

		public static NotExpression Not(Expression operand)
		{
			return new NotExpression(operand);
		}

		public static NoOpExpression NoOp(Expression operand)
		{
			return new NoOpExpression(operand);
		}

		public static LikeExpression Like(string name, object value)
		{
			return new LikeExpression(name, value);
		}

		public static NotLikeExpression NotLike(string name, object value)
		{
			return new NotLikeExpression(name, value);
		}

		public static Expression operator ==(Expression left, Expression right)
		{
			return new EqualsExpression(left, right);
		}

		public static EqualsExpression Equals(string name, object value)
		{
			return new EqualsExpression(name, value);
		}

		public static Expression operator !=(Expression left, Expression right)
		{
			return new NotEqualsExpression(left, right);
		}

		public static NotEqualsExpression NotEquals(string name, object value)
		{
			return new NotEqualsExpression(name, value);
		}

		public static Expression operator >(Expression left, Expression right)
		{
			return new GreaterThanExpression(left, right);
		}

		public static GreaterThanExpression GreaterThan(string name, object value)
		{
			return new GreaterThanExpression(name, value);
		}

		public static Expression operator >=(Expression left, Expression right)
		{
			return new GreaterThanOrEqualsExpression(left, right);
		}

		public static GreaterThanOrEqualsExpression GreaterThanOrEquals(string name, object value)
		{
			return new GreaterThanOrEqualsExpression(name, value);
		}

		public static Expression operator <(Expression left, Expression right)
		{
			return new LessThanExpression(left, right);
		}

		public static LessThanExpression LessThan(string name, object value)
		{
			return new LessThanExpression(name, value);
		}

		public static Expression operator <=(Expression left, Expression right)
		{
			return new LessThanOrEqualsExpression(left, right);
		}

		public static LessThanOrEqualsExpression LessThanOrEquals(string name, object value)
		{
			return new LessThanOrEqualsExpression(name, value);
		}

		public static bool IsNull(Expression expression)
		{
			return (expression as object) == null;
		}

		public override string ToString()
		{
			return ToString(null);
		}

		public virtual string ToString(IDictionary<Type, ExpressionAction> map)
		{
			if (map != null)
			{
				var type = GetType();

				while (type != null)
				{
					if (map.ContainsKey(type))
					{
						return map[type](this, map);
					}

					type = type.BaseType;
				}
			}

			return null;
		}

		public abstract bool Evaluate(IExpressionEvaluator evaluator);

		public void ForEachChild(Action<Expression> action)
		{
			foreach (var operand in this)
			{
				action(operand);
			}
		}

		public void ForEachDescendant(Action<Expression> action)
		{
			foreach (var operand in this)
			{
				action(operand);
				operand.ForEachDescendant(action);
			}
		}

		public void ForSubTree(Action<Expression> action)
		{
			action(this);

			foreach (var operand in this)
			{
				operand.ForSubTree(action);
			}
		}

		public IEnumerable<Expression> Descendants
		{
			get
			{
				foreach (var operand in this)
				{
					yield return operand;

					foreach (var expression in operand.Descendants)
					{
						yield return expression;
					}
				}
			}
		}

		public IEnumerable<Expression> SubTree
		{
			get
			{
				yield return this;

				foreach (var operand in this)
				{
					foreach (var expression in operand.SubTree)
					{
						yield return expression;
					}
				}
			}
		}

		public IEnumerable<Expression> GetSubTreeEnumerator(Predicate<Expression> match)
		{
			if (match(this))
			{
				yield return this;
			}

			foreach (var operand in this)
			{
				foreach (var expression in operand.GetSubTreeEnumerator(match))
				{
					yield return expression;
				}
			}
		}

		#region Parsing Methods

		public static Expression ParseCondition(string condition)
		{
			return ParseCondition(condition, ParseValue);
		}

		public static Expression ParseCondition(string condition, Func<string, string, object> parseValue)
		{
			using (var reader = new StringReader(condition))
			{
				return RenderExpression(reader, parseValue);
			}
		}

		public static bool TryParseCondition(string condition, out Expression expression)
		{
			return TryParseCondition(condition, out expression, ParseValue);
		}

		public static bool TryParseCondition(string condition, out Expression expression, Func<string, string, object> parseValue)
		{
			try
			{
				using (var reader = new StringReader(condition))
				{
					expression = RenderExpression(reader, parseValue);
				}

				return true;
			}
			catch
			{
				expression = null;
			}

			return false;
		}

		private static Expression GetExpression(string op, string name, List<Expression> operands, Func<string, string, object> parseValue)
		{
			// check if this is a logical expression

			if (op == "&")
			{
				if (operands == null || operands.Count == 0)
				{
					throw new InvalidExpressionException(string.Format("Invalid expression {0}.", name));
				}

				return And(operands.ToArray());
			}
			if (op == "|")
			{
				if (operands == null || operands.Count == 0)
				{
					throw new InvalidExpressionException(string.Format("Invalid expression {0}.", name));
				}

				return Or(operands.ToArray());
			}
			if (op == "!")
			{
				if (operands == null || operands.Count != 1)
				{
					throw new InvalidExpressionException(string.Format("Invalid expression {0}.", name));
				}

				return Not(operands[0]);
			}
			if (op == null)
			{
				if (operands == null || operands.Count != 1)
				{
					throw new InvalidExpressionException(string.Format("Invalid expression {0}.", name));
				}

				return NoOp(operands[0]);
			}
			// check if this is a conditional string

			var ops = name.Split(new[] { op }, 2, StringSplitOptions.None);

			if (ops.Length != 2)
			{
				throw new InvalidExpressionException(string.Format("Invalid expression {0}.", name));
			}

			var attributeName = ops[0];
			var text = ops[1];
			var containsWildcard = text != null && (Regex.IsMatch(text, @"[^\\]\*") || text.StartsWith("*"));

			var value = parseValue(attributeName, text);

			// if an '*' is detected in an '=' expression, promote to 'like' expression
			if ((op == "=" || op == "==") && containsWildcard)
			{
				op = "~=";
			}

			if (op == "=" || op == "==")
			{
				return Equals(attributeName, value);
			}
			if (op == "!=")
			{
				if (containsWildcard)
				{
					// if an '*' is detected in an '=' expression, promote to 'like' expression
					return NotLike(attributeName, value);
				}
				return NotEquals(attributeName, value);
			}
			if (op == "~=")
			{
				return Like(attributeName, value);
			}
			if (op == "<")
			{
				return LessThan(attributeName, value);
			}
			if (op == "<=")
			{
				return LessThanOrEquals(attributeName, value);
			}
			if (op == ">")
			{
				return GreaterThan(attributeName, value);
			}
			if (op == ">=")
			{
				return GreaterThanOrEquals(attributeName, value);
			}

			throw new InvalidOperationException(string.Format("Unknown operator symbol {0}.", op));
		}

		private static Expression RenderExpression(StringReader reader, Func<string, string, object> parseValue)
		{
			var operands = new List<Expression>();
			var name = new StringBuilder();
			string op = null;
			var union = false;
			var unionAnd = false;
			var unionOperandCount = 0;

			while (true)
			{
				// pop the next character
				var value = reader.Read();

				// reached end of string?
				if (value == -1) break;
				
				var current = Convert.ToChar(value);

				if (current == '\\')
				{
					// may be trying to escape a special character
					var next = Convert.ToChar(reader.Peek());

					if (@"*()\@".Contains(next.ToString()))
					{
						// read the next character as a literal value
						reader.Read();
						name.Append(current);
						name.Append(next);
					}
					else
					{
						// not a special character, continue normally
						name.Append(current);
					}
				}
				else if (current == '(')
				{
					// start a recursive call to handle sub-expression
					Expression operand = RenderExpression(reader, parseValue);
					operands.Add(operand);
					union = false;
					unionOperandCount++;
				}
				else if (current == ')')
				{
					// reached end of sub-expression

					if (union)
					{
						if (operands.Count <= unionOperandCount)
						{
							var operand = GetExpression(op, name.ToString(), operands, parseValue);

							operands.Add(operand);
						}

						return unionAnd ? GetExpression("&", "&", operands, parseValue) : GetExpression("|", "|", operands, parseValue);
					}

					return GetExpression(op, name.ToString(), operands, parseValue);
				}
				else if ("&|!=<>~".Contains(current.ToString()))
				{
					if ((op != null | operands.Count > 0) && (current.ToString() == "&" | current.ToString() == "|"))
					{
						if (op != null)
						{
							var operand = GetExpression(op, name.ToString(), operands, parseValue);
							operands.Add(operand);
							unionOperandCount++;
							name.Clear();
						}

						op = current.ToString();

						if (union && unionAnd && current.ToString() == "|")
						{
							var unionOperand = GetExpression("&", "&", operands, parseValue);
							operands.Clear();
							operands.Add(unionOperand);
							unionOperandCount = 1;
						}
						if (union && !unionAnd && current.ToString() == "&")
						{
							var unionOperand = GetExpression("|", "|", operands, parseValue);
							operands.Clear();
							operands.Add(unionOperand);
							unionOperandCount = 1;
						}
						union = true;
						unionAnd = current.ToString() == "&";
					}
					else
					{
						// encountered an operator
						op = current.ToString();
						name.Append(current);

						// check if this is a 2 character operator
						if (reader.Peek() > -1)
						{
							var next = Convert.ToChar(reader.Peek());

							if ("=".Contains(next.ToString()))
							{
								// read the second character
								reader.Read();
								op += next;
								name.Append(next);
							}
						}
					}
				}
				else
				{
					// this is a character in a literal value
					name.Append(current);
				}
			}

			if (union)
			{
				if (operands.Count <= unionOperandCount)
				{
					var operand = GetExpression(op, name.ToString(), operands, parseValue);

					operands.Add(operand);
				}

				return unionAnd ? GetExpression("&", "&", operands, parseValue) : GetExpression("|", "|", operands, parseValue);
			}

			// reached end of expression
			return GetExpression(op, name.ToString(), operands, parseValue);
		}

		private static object ParseValue(string attributeName, string text)
		{
			// try convert from string to value type

			object value;
			DateTime dateTimeValue;
			double doubleValue;
			bool boolValue;
			Guid guidValue;

			if (double.TryParse(
					text,
					NumberStyles.Any,
					CultureInfo.InvariantCulture,
					out doubleValue))
			{
				value = doubleValue;
			}
			else if (DateTime.TryParse(
					text,
					CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeLocal,
					out dateTimeValue))
			{
				value = dateTimeValue;
			}
			else if (bool.TryParse(
					text,
					out boolValue))
			{
				value = boolValue;
			}
			else if (Guid.TryParse(
					text,
					out guidValue))
			{
				value = guidValue;
			}
			else if (string.Compare(text, "null", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				value = null;
			}
			else
			{
				value = text;
			}

			return value;
		}

		#endregion

		/// <summary>
		/// Replaces parameterized literals (identifiers starting with '@') of the expression tree with the actual values provided by the dictionary.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public Expression SubstituteParameters(Dictionary<string, object> parameters)
		{
			return Clone(
				(expression, parent) =>
					{
						if (expression is LiteralExpression && (expression as LiteralExpression).Value is string)
						{
							// replace '@' parameter identifiers into the actual parameter value

							var value = (expression as LiteralExpression).Value as string;

							if (value != null && value.StartsWith("@"))
							{
								if (parameters != null && parameters.ContainsKey(value))
								{
									(expression as LiteralExpression).Value = parameters[value];
								}
								else
								{
									value = value.TrimStart('@'); // the '@' character is optional in the parameter dictionary

									if (parameters != null && parameters.ContainsKey(value))
									{
										(expression as LiteralExpression).Value = parameters[value];
									}
									else
									{
										throw new InvalidOperationException(string.Format("The filter parameter {0} is missing an actual value in the dictionary.", value));
									}
								}
							}
						}

						return expression;
					});
		}

		public Expression Clone()
		{
			return Clone((expression, parent) => expression);
		}

		public Expression Clone(Func<Expression, Expression, Expression> convert)
		{
			return Clone(convert, null);
		}

		public abstract Expression Clone(Func<Expression, Expression, Expression> convert, Expression parent);

		public IEnumerator<Expression> GetEnumerator()
		{
			if (Operands != null)
			{
				foreach (var operand in Operands)
				{
					yield return operand;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			yield return GetEnumerator();
		}

		public bool Equals(Expression other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			return ReferenceEquals(this, other) || Equals(other.Operands, Operands);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return obj.GetType() == typeof(Expression) && Equals((Expression)obj);
		}

		public override int GetHashCode()
		{
			return (Operands != null ? Operands.GetHashCode() : 0);
		}
	}

	public class LiteralExpression : Expression
	{
		public object Value { get; set; }

		public LiteralExpression(object value)
		{
			if (value is string && !string.IsNullOrWhiteSpace((string)value))
			{
				value = ((string)value).Trim(new[] { ' ', '\'' });

				if (((string)value).ToLowerInvariant() == "null")
				{
					value = null;
				}
			}
			
			Value = value;
		}

		public override string ToString(IDictionary<Type, ExpressionAction> map)
		{
			var value = base.ToString(map) ?? (Value == null ? "NULL" : Value.ToString());

			return value;
		}

		public override Expression Clone(Func<Expression, Expression, Expression> convert, Expression parent)
		{
			return convert(new LiteralExpression(Value), parent);
		}

		public override bool Evaluate(IExpressionEvaluator evaluator)
		{
			// this method should be guarded from being called by all BinaryExpressions

			throw new NotSupportedException(string.Format("Unable to evaluate an expression of type {0} with the value {1}.", this, Value));
		}
	}

	public class LeftLiteralExpression : LiteralExpression
	{
		public LeftLiteralExpression(object value) : base(value)
		{
		}

		public override Expression Clone(Func<Expression, Expression, Expression> convert, Expression parent)
		{
			return convert(new LeftLiteralExpression(Value), parent);
		}
	}

	public class RightLiteralExpression : LiteralExpression
	{
		public RightLiteralExpression(object value)
			: base(value)
		{
		}

		public override Expression Clone(Func<Expression, Expression, Expression> convert, Expression parent)
		{
			return convert(new RightLiteralExpression(Value), parent);
		}
	}

	public abstract class BooleanExpression : Expression
	{
		protected BooleanExpression(params Expression[] operands)
			: base(operands)
		{
		}

		public override string ToString(IDictionary<Type, ExpressionAction> map)
		{
			var value = base.ToString(map);

			if (value == null)
			{
				var ops = Operands.Select(operand => operand.ToString(map)).ToList();

				var opsArray = new string[ops.Count];
				ops.CopyTo(opsArray);

				//value = "(\n" + string.Join(" " + Operator + " ", opsArray) + "\n)\n";
				value = string.Format("({0})", string.Join(" " + Operator + " ", opsArray));
			}

			return value;
		}

		public override Expression Clone(Func<Expression, Expression, Expression> convert, Expression parent)
		{
			return convert(Clone(Operands.Select(operand => operand.Clone(convert, this)).ToArray()), parent);
		}

		public override bool Evaluate(IExpressionEvaluator evaluator)
		{
			return Evaluate(Operands.Select(operand => operand.Evaluate(evaluator)));
		}

		public abstract Expression Clone(Expression[] operands);

		protected abstract bool Evaluate(IEnumerable<bool> operands);
	}

	public class OrExpression : BooleanExpression
	{
		public OrExpression(params Expression[] operands)
			: base(operands)
		{
		}

		public override string Operator
		{
			get { return "or"; }
		}

		public override Expression Clone(Expression[] operands)
		{
			return new OrExpression(operands);
		}

		protected override bool Evaluate(IEnumerable<bool> operands)
		{
			// find any expression that is true

			return operands.Any(operand => operand);
		}
	}

	public class AndExpression : BooleanExpression
	{
		public AndExpression(params Expression[] operands)
			: base(operands)
		{
		}

		public override string Operator
		{
			get { return "and"; }
		}

		public override Expression Clone(Expression[] operands)
		{
			return new AndExpression(operands);
		}

		protected override bool Evaluate(IEnumerable<bool> operands)
		{
			// find any expression that is false

			return !operands.Any(operand => !operand);
		}
	}

	public abstract class UnaryExpression : Expression
	{
		protected UnaryExpression(Expression operand)
			: base(operand)
		{
		}

		public Expression GetOperand()
		{
			var enumerator = Operands.GetEnumerator();
			enumerator.MoveNext();
			return enumerator.Current;
		}

		public override string ToString(IDictionary<Type, ExpressionAction> map)
		{
			var value = base.ToString(map);

			if (value == null)
			{
				var operand = GetOperand();

				// default to postfix notation
				value = string.Format(@"{0} ({1})", Operator, operand.ToString(map));
			}

			return value;
		}

		public override Expression Clone(Func<Expression, Expression, Expression> convert, Expression parent)
		{
			var operand = GetOperand();
			return convert(Clone(operand.Clone(convert, this)), parent);
		}

		public override bool Evaluate(IExpressionEvaluator evaluator)
		{
			// pass the operand through

			return Evaluate(GetOperand().Evaluate(evaluator));
		}

		public abstract Expression Clone(Expression operand);

		protected abstract bool Evaluate(bool operand);
	}

	public class NotExpression : UnaryExpression
	{
		public NotExpression(Expression operand)
			: base(operand)
		{
		}

		public override string Operator
		{
			get { return "not"; }
		}

		public override Expression Clone(Expression operand)
		{
			return new NotExpression(operand);
		}

		protected override bool Evaluate(bool operand)
		{
			// negate the operand

			return !operand;
		}
	}

	public class NoOpExpression : UnaryExpression
	{
		public NoOpExpression(Expression operand)
			: base(operand)
		{
		}

		public override string ToString(IDictionary<Type, ExpressionAction> map)
		{
			var value = base.ToString(map);

			if (value == null)
			{
				var operand = GetOperand();
				value = string.Format(@"({0})", operand.ToString(map));
			}

			return value;
		}

		public override Expression Clone(Expression operand)
		{
			return new NoOpExpression(operand);
		}

		protected override bool Evaluate(bool operand)
		{
			// pass the operand through

			return operand;
		}
	}

	public abstract class BinaryExpression : Expression
	{
		protected BinaryExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		protected BinaryExpression(string name, object value)
			: base(name, value)
		{
		}

		public Expression Left
		{
			get
			{
				var enumerator = Operands.GetEnumerator();
				enumerator.MoveNext();
				return enumerator.Current;
			}
		}

		public Expression Right
		{
			get
			{
				var enumerator = Operands.GetEnumerator();
				enumerator.MoveNext();
				enumerator.MoveNext();
				return enumerator.Current;
			}
		}

		public override string ToString(IDictionary<Type, ExpressionAction> map)
		{
			var value = base.ToString(map);

			if (value == null)
			{
				var ops = new List<LiteralExpression>(2);
				ops.AddRange(Operands.Cast<LiteralExpression>());

				// default to infix notation
				value = string.Format(@"({1} {0} {2})", Operator, ops[0].ToString(map), ops[1].ToString(map));
			}

			return value;
		}

		public override Expression Clone(Func<Expression, Expression, Expression> convert, Expression parent)
		{
			var left = Left.Clone(convert, this);
			var right = Right.Clone(convert, this);

			return convert(Clone(left, right), parent);
		}

		public override bool Evaluate(IExpressionEvaluator evaluator)
		{
			return evaluator.Evaluate(this);
		}

		public abstract Expression Clone(Expression left, Expression right);
	}

	public class LikeExpression : BinaryExpression
	{
		public LikeExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		public LikeExpression(string name, object value)
			: base(name, value)
		{
		}

		public override string Operator
		{
			get { return "like"; }
		}

		public override Expression Clone(Expression left, Expression right)
		{
			return new LikeExpression(left, right);
		}
	}

	
	public class NotLikeExpression : BinaryExpression
	{
		public NotLikeExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		public NotLikeExpression(string name, object value)
			: base(name, value)
		{
		}

		public override string Operator
		{
			get { return "not like"; }
		}

		public override Expression Clone(Expression left, Expression right)
		{
			return new NotLikeExpression(left, right);
		}
	}

	
	public class EqualsExpression : BinaryExpression
	{
		public EqualsExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		public EqualsExpression(string name, object value)
			: base(name, value)
		{
		}

		public override string Operator
		{
			get { return "="; }
		}

		public override Expression Clone(Expression left, Expression right)
		{
			return new EqualsExpression(left, right);
		}
	}

	
	public class NotEqualsExpression : BinaryExpression
	{
		public NotEqualsExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		public NotEqualsExpression(string name, object value)
			: base(name, value)
		{
		}

		public override string Operator
		{
			get { return "<>"; }
		}

		public override Expression Clone(Expression left, Expression right)
		{
			return new NotEqualsExpression(left, right);
		}
	}

	
	public class GreaterThanExpression : BinaryExpression
	{
		public GreaterThanExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		public GreaterThanExpression(string name, object value)
			: base(name, value)
		{
		}

		public override string Operator
		{
			get { return ">"; }
		}

		public override Expression Clone(Expression left, Expression right)
		{
			return new GreaterThanExpression(left, right);
		}
	}

	
	public class GreaterThanOrEqualsExpression : BinaryExpression
	{
		public GreaterThanOrEqualsExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		public GreaterThanOrEqualsExpression(string name, object value)
			: base(name, value)
		{
		}

		public override string Operator
		{
			get { return ">="; }
		}

		public override Expression Clone(Expression left, Expression right)
		{
			return new GreaterThanOrEqualsExpression(left, right);
		}
	}

	
	public class LessThanExpression : BinaryExpression
	{
		public LessThanExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		public LessThanExpression(string name, object value)
			: base(name, value)
		{
		}

		public override string Operator
		{
			get { return "<"; }
		}

		public override Expression Clone(Expression left, Expression right)
		{
			return new LessThanExpression(left, right);
		}
	}
	
	public class LessThanOrEqualsExpression : BinaryExpression
	{
		public LessThanOrEqualsExpression(Expression left, Expression right)
			: base(left, right)
		{
		}

		public LessThanOrEqualsExpression(string name, object value)
			: base(name, value)
		{
		}

		public override string Operator
		{
			get { return "<="; }
		}

		public override Expression Clone(Expression left, Expression right)
		{
			return new LessThanOrEqualsExpression(left, right);
		}
	}

	public interface IExpressionEvaluator
	{
		bool Evaluate(Expression expression);
	}

	public class EntityExpressionEvaluator : IExpressionEvaluator
	{
		protected Entity EvaluateEntity { get; private set; }
		protected OrganizationServiceContext ServiceContext { get; private set; }
		protected Dictionary<string, AttributeTypeCode?> AttributeTypeCodeDictionary { get; private set; } 

		public EntityExpressionEvaluator(OrganizationServiceContext context, Entity entity)
		{
			ServiceContext = context;
			EvaluateEntity = entity;
			AttributeTypeCodeDictionary = MetadataHelper.BuildAttributeTypeCodeDictionary(context, entity.LogicalName);
		}

		public bool Evaluate(Expression expression)
		{
			if (expression is BinaryExpression)
			{
				var left = (expression as BinaryExpression).Left;
				var right = (expression as BinaryExpression).Right;

				if (expression is LikeExpression)
				{
					return Evaluate(left, right, Match, (value, type) => value);
				}

				if (expression is NotLikeExpression)
				{
					return Evaluate(left, right, (l, r) => !Match(l, r));
				}

				if (expression is EqualsExpression)
				{
					return Evaluate(left, right, (l, r) => Compare(l, r) == 0);
				}

				if (expression is NotEqualsExpression)
				{
					return Evaluate(left, right, (l, r) => Compare(l, r) != 0);
				}

				if (expression is GreaterThanExpression)
				{
					return Evaluate(left, right, (l, r) => Compare(l, r) > 0);
				}

				if (expression is GreaterThanOrEqualsExpression)
				{
					return Evaluate(left, right, (l, r) => Compare(l, r) >= 0);
				}

				if (expression is LessThanExpression)
				{
					return Evaluate(left, right, (l, r) => Compare(l, r) < 0);
				}

				if (expression is LessThanOrEqualsExpression)
				{
					return Evaluate(left, right, (l, r) => Compare(l, r) <= 0);
				}
			} 
			else if (expression is BooleanExpression)
			{
				if (expression is AndExpression)
				{
					return expression.Operands.Select(Evaluate).Aggregate(true, (current, value) => current & value);
				}

				if (expression is OrExpression)
				{
					return expression.Operands.Select(Evaluate).Aggregate(false, (current, value) => current | value);
				}
			}

			throw new NotSupportedException(string.Format("Unable to evaluate an expression of type {0}.", expression));
		}

		protected bool Evaluate(Expression left, Expression right, Func<object, object, bool> compare)
		{
			return Evaluate(left, right, compare, null);
		}

		protected bool Evaluate(Expression left, Expression right, Func<object, object, bool> compare, Func<object, Type, object> convert)
		{
			object testValue;
			var attributeName = ((LeftLiteralExpression)left).Value as string;
			var expressionValue = ((RightLiteralExpression)right).Value;

			if (EvaluateEntity == null)
			{
				throw new NullReferenceException("EvaluateEntity is null.");
			}
			
			if (string.IsNullOrWhiteSpace(attributeName))
			{
				throw new InvalidOperationException(string.Format("Unable to recognize the attribute {0} specified in the expression.", attributeName));
			}

			var attributeTypeCode = AttributeTypeCodeDictionary.FirstOrDefault(a => a.Key == attributeName).Value;

			if (attributeTypeCode == null)
			{
				throw new InvalidOperationException(string.Format("Unable to recognize the attribute {0} specified in the expression.", attributeName));
			}

			var attributeValue = EvaluateEntity.Attributes.ContainsKey(attributeName) ? EvaluateEntity.Attributes[attributeName] : null;

			switch (attributeTypeCode)
			{
				case AttributeTypeCode.BigInt:
					if (expressionValue != null && !(expressionValue is long | expressionValue is double))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : Convert.ToInt64(expressionValue);
					break;
				case AttributeTypeCode.Boolean:
					if (expressionValue != null && !(expressionValue is bool))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : (bool)expressionValue;
					break;
				case AttributeTypeCode.Customer:
					var entityReference = EvaluateEntity.Attributes.ContainsKey(attributeName) ? (EntityReference)EvaluateEntity.Attributes[attributeName] : null;
					attributeValue = entityReference != null ? (object)entityReference.Id : null;
					if (expressionValue != null && !(expressionValue is Guid))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : (Guid)expressionValue;
					break;
				case AttributeTypeCode.DateTime:
					if (expressionValue != null && !(expressionValue is DateTime))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : ((DateTime)expressionValue).ToUniversalTime();
					break;
				case AttributeTypeCode.Decimal:
					if (expressionValue != null && !(expressionValue is int | expressionValue is double | expressionValue is decimal))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : Convert.ToDecimal(expressionValue);
					break;
				case AttributeTypeCode.Double:
					if (expressionValue != null && !(expressionValue is int | expressionValue is double))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : Convert.ToDouble(expressionValue);
					break;
				case AttributeTypeCode.Integer:
					if (expressionValue != null && !(expressionValue is int | expressionValue is double))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : Convert.ToInt32(expressionValue);
					break;
				case AttributeTypeCode.Lookup:
					var lookupEntityReference = EvaluateEntity.Attributes.ContainsKey(attributeName) ? (EntityReference)EvaluateEntity.Attributes[attributeName] : null;
					attributeValue = lookupEntityReference != null ? (object)lookupEntityReference.Id : null;
					if (expressionValue != null && !(expressionValue is Guid))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : (Guid)expressionValue;
					break;
				case AttributeTypeCode.Memo:
					if (expressionValue != null && !(expressionValue is string))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue as string;
					break;
				case AttributeTypeCode.Money:
					var money = EvaluateEntity.Attributes.ContainsKey(attributeName) ? (Money)EvaluateEntity.Attributes[attributeName] : null;
					attributeValue = money != null ? (object)Convert.ToDecimal(money.Value) : null;
					if (expressionValue != null && !(expressionValue is int | expressionValue is double | expressionValue is decimal))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : Convert.ToDecimal(expressionValue);
					break;
				case AttributeTypeCode.Picklist:
					var optionSetValue = EvaluateEntity.Attributes.ContainsKey(attributeName) ? (OptionSetValue)EvaluateEntity.Attributes[attributeName] : null;
					attributeValue = optionSetValue != null ? (object)optionSetValue.Value : null;
					if (expressionValue != null && !(expressionValue is int | expressionValue is double))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : Convert.ToInt32(expressionValue);
					break;
				case AttributeTypeCode.State:
					var stateOptionSetValue = EvaluateEntity.Attributes.ContainsKey(attributeName) ? (OptionSetValue)EvaluateEntity.Attributes[attributeName] : null;
					attributeValue = stateOptionSetValue != null ? (object)stateOptionSetValue.Value : null;
					if (expressionValue != null && !(expressionValue is int | expressionValue is double))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : Convert.ToInt32(expressionValue);
					break;
				case AttributeTypeCode.Status:
					var statusOptionSetValue = EvaluateEntity.Attributes.ContainsKey(attributeName) ? (OptionSetValue)EvaluateEntity.Attributes[attributeName] : null;
					attributeValue = statusOptionSetValue != null ? (object)statusOptionSetValue.Value : null;
					if (expressionValue != null && !(expressionValue is int | expressionValue is double))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : Convert.ToInt32(expressionValue);
					break;
				case AttributeTypeCode.String:
					if (expressionValue != null && !(expressionValue is string))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue as string;
					break;
				case AttributeTypeCode.Uniqueidentifier:
					if (expressionValue != null && !(expressionValue is Guid))
					{
						throw new InvalidOperationException(string.Format("Attribute {0} specified in the expression is expecting a {1}. The value provided isn't valid.", attributeName, attributeTypeCode));
					}
					testValue = expressionValue == null ? (object)null : (Guid)expressionValue;
					break;
				default:
					throw new InvalidOperationException(string.Format("Unsupported type of attribute {0} specified in the expression.", attributeName));
			}

			return compare(attributeValue, testValue);
		}

		private static bool Match(object left, object right)
		{
			if (left == null && right == null) return true;

			// a Regex (ie. a like condition) will only work on string values

			var l = left != null ? left.ToString() : string.Empty;
			var r = right != null ? right.ToString() : string.Empty;

			var mask = new Mask(r);

			return mask.IsMatch(l);
		}

		private static int Compare(object left, object right)
		{
			if (left == null && right == null) return 0;

			if (left is IComparable) return (left as IComparable).CompareTo(right);
			if (right is IComparable) return (right as IComparable).CompareTo(left) * -1;

			throw new InvalidOperationException(string.Format("The value {0} can't be compared to the value {1}.", left, right));
		}
	}
}
