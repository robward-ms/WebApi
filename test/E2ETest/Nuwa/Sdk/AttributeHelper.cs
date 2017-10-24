using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Nuwa.Sdk
{
    public static class AttributeHelper
    {
        /// <summary>
        /// Return the first found custom attribute on a method
        /// </summary>
        /// <param name="attributeType">type of the custom attribute</param>
        /// <returns>the first found custom attribute; or null none is found</returns>
        public static IAttributeInfo GetFirstCustomAttribute<T>(this IMethodInfo me) where T : Attribute
        {
            IAttributeInfo attrInfo = null;

            foreach (var a in me.GetCustomAttributes(typeof(T)))
            {
                attrInfo = a;
                break;
            }

            return attrInfo;
        }

        /// <summary>
        /// Return the first found custom attribute
        /// </summary>
        /// <param name="attributeType">type of the custom attribute</param>
        /// <returns>the first found custom attribute; or null none is found</returns>
        public static IAttributeInfo GetFirstCustomAttribute(this ITypeInfo me, Type attributeType)
        {
            IAttributeInfo retval = null;

            foreach (var a in me.GetCustomAttributes(attributeType))
            {
                retval = a;
                break;
            }

            return retval;
        }

        /// <summary>
        /// Return the first found custom attribute
        /// </summary>
        /// <param name="attributeType">type of the custom attribute</param>
        /// <returns>the first found custom attribute; or null none is found</returns>
        public static IAttributeInfo GetFirstCustomAttribute<T>(this ITypeInfo me) where T : Attribute
        {
            IAttributeInfo attrInfo = null;

            foreach (var a in me.GetCustomAttributes(typeof(T)))
            {
                attrInfo = a;
                break;
            }

            if (attrInfo != null)
            {
                return attrInfo;
            }
            else
            {
                return null;
            }
        }

        public static IAttributeInfo[] GetCustomAttributes<T>(this ITypeInfo me) where T : Attribute
        {
            var retvals = new List<IAttributeInfo>();

            foreach (var attr in me.GetCustomAttributes(typeof(T)))
            {
                if (attr != null)
                {
                    retvals.Add(attr);
                }
            }

            return retvals.ToArray();
        }

        /// <summary>
        /// Returns the methods marked by given type of attribute
        /// </summary>
        /// <param name="me">this</param>
        /// <param name="attribute">target attribute type</typeparam>
        /// <returns>Array of the found method, return zero length array if nothing is found.</returns>
        public static IMethodInfo[] GetMethodMarkedByAttribute(this ITypeInfo me, Type attribute)
        {
            var retval = new List<IMethodInfo>();

            foreach (var m in me.GetMethods(includePrivateMethods: true))
            {
                if (m.GetCustomAttributes(attribute).Any())
                {
                    retval.Add(m);
                }
            }

            return retval.ToArray();
        }
    }
}
