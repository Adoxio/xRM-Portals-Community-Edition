/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.UI;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Compilation
{
    public class ResourceManagerExpressionBuilder : ExpressionBuilder
    {
        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {

            Type type = entry.DeclaringType;
            PropertyDescriptor descriptor =
              TypeDescriptor.GetProperties(type)
                [entry.PropertyInfo.Name];
            CodeExpression[] expressionArray =
              new CodeExpression[3];
            expressionArray[0] = new
              CodePrimitiveExpression(entry.Expression.Trim());
            expressionArray[1] = new
              CodeTypeOfExpression(type);
            expressionArray[2] = new
              CodePrimitiveExpression(entry.Name);

            return new CodeCastExpression(descriptor.PropertyType, new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(GetType()), "GetEvalData", expressionArray));

        }

        public static object GetEvalData(string expression, Type target, string entry)
        {
            return ResourceManager.GetString(expression);
        }
    }
}
