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
	<CreationDate>2010-09-24 17:32:47Z</CreationDate>
	<Origin>http://www.calciumsdk.com</Origin>
</File>
*/
#endregion

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DanielVaughan.Windows
{
	public static class VisualTree
	{
		public static DependencyObject GetParent(DependencyObject currentObject)
		{
			ArgumentValidator.AssertNotNull(currentObject, "currentObject");
			FrameworkElement element = currentObject as FrameworkElement;
			if (element != null)
			{
				return element.Parent;
			}
			return null;
		}

		public static TAncestor GetAncestorOrSelf<TAncestor>(this FrameworkElement childElement)
			where TAncestor : class
		{
			ArgumentValidator.AssertNotNull(childElement, "childElement");
			var parent = childElement;
			while (parent != null)
			{
				TAncestor result = parent as TAncestor;
				if (result != null)
				{
					return result;
				}
				parent = parent.Parent as FrameworkElement;
			}
			return null;
		}

		public static IEnumerable GetChildren(DependencyObject current)
		{
			ArgumentValidator.AssertNotNull(current, "current");
			FrameworkElement element = current as FrameworkElement;
			if (element != null)
			{
				return GetVisualChildren(element);
			}
			return new List<DependencyObject>();
		}

		public static IEnumerable<TChild> GetDescendents<TChild>(
			this FrameworkElement parent) where TChild : class
		{
			ArgumentValidator.AssertNotNull(parent, "parent");

			int childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				var candidate = child as TChild;
				if (candidate != null)
				{
					yield return candidate;
				}

				FrameworkElement element = child as FrameworkElement;
				if (element != null)
				{
					/* Can be improved with tail recursion. */
					var descendents = element.GetDescendents<TChild>();
					foreach (var descendent in descendents)
					{
						yield return descendent;
					}
				}
			}
		}

		static int nameCounter;

		internal static IEnumerable<FrameworkElement> GetVisualChildren(
			this FrameworkElement parent)
		{
			ArgumentValidator.AssertNotNull(parent, "parent");

			/* This should be rewritten to avoid naming elements. */
			if (string.IsNullOrEmpty(parent.Name))
			{
				parent.Name = "generatedName_" + nameCounter++;
			}

			string parentName = parent.Name;
			var children = parent.GetVisualChildren().OfType<FrameworkElement>();
			Stack<FrameworkElement> stack = new Stack<FrameworkElement>(children);

			while (stack.Count > 0)
			{
				FrameworkElement element = stack.Pop();
				if (element.FindName(parentName) == parent)
				{
					yield return element;
				}
				else
				{
					foreach (FrameworkElement visualChild
						in element.GetVisualChildren().OfType<FrameworkElement>())
					{
						stack.Push(visualChild);
					}
				}
			}
		}

		internal static IEnumerable<DependencyObject> GetVisualChildren(
			this DependencyObject parent)
		{
			ArgumentValidator.AssertNotNull(parent, "parent");

			int childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int counter = 0; counter < childCount; counter++)
			{
				yield return VisualTreeHelper.GetChild(parent, counter);
			}
		}

		/// <summary>
		/// Tries the get ancestor of the specified FrameworkElement 
		/// with the specified generic type. Walks the visual tree.
		/// </summary>
		/// <typeparam name="TAncestor">The type of the ancestor.</typeparam>
		/// <param name="frameworkElement">The child framework element.</param>
		/// <param name="ancestor">The resulting ancestor.</param>
		/// <returns>The <c>true</c> if an ancestor was found of the specified type; 
		/// <c>false</c> otherwise.</returns>
		public static bool TryGetAncestorOrSelf<TAncestor>(
			this FrameworkElement frameworkElement, out TAncestor ancestor)
			where TAncestor : class
		{
			ArgumentValidator.AssertNotNull(frameworkElement, "frameworkElement");
			TAncestor result = null;
			GetAncestorOrSelf(frameworkElement, ref result);
			ancestor = result;
			return ancestor != null;
		}

		static void GetAncestorOrSelf<TAncestor>(
			FrameworkElement frameworkElement, ref TAncestor result)
			where TAncestor : class
		{
			if (frameworkElement == null) /* Terminal condition. */
			{
				return; /* Terminal case. */
			}

			var castedElement = frameworkElement as TAncestor;
			if (castedElement != null) /* Terminal condition. */
			{
				result = castedElement; /* Terminal case. */
				return;
			}

			FrameworkElement parent
				= VisualTreeHelper.GetParent(frameworkElement) as FrameworkElement;
			GetAncestorOrSelf(parent, ref result); /* Tail recursive. */
		}

		/// <summary>
		/// Gets the visual parent of the specified element.
		/// </summary>
		/// <param name="childElement">The child element.</param>
		/// <returns>The visual parent</returns>
		public static FrameworkElement GetVisualParent(
			this FrameworkElement childElement)
		{
			return VisualTreeHelper.GetParent(childElement) as FrameworkElement;
		}

		/// <summary>
		/// Gets the ancestors of the element.
		/// </summary>
		/// <param name="descendentElement">The start element.</param>
		/// <returns>The ancestors of the specified element.</returns>
		public static IEnumerable<FrameworkElement> GetVisualAncestors(
			this FrameworkElement descendentElement)
		{
			ArgumentValidator.AssertNotNull(descendentElement, "descendentElement");
			FrameworkElement parent = descendentElement.GetVisualParent();
			while (parent != null)
			{
				yield return parent;
				parent = parent.GetVisualParent();
			}
		}

		/// <summary>
		/// Gets the ancestors of the element.
		/// </summary>
		/// <param name="descendentElement">The start element.</param>
		/// <returns>The ancestors of the specified element.</returns>
		public static IEnumerable<TAncestor> GetVisualAncestors<TAncestor>(
			this FrameworkElement descendentElement) where TAncestor : class
		{
			ArgumentValidator.AssertNotNull(descendentElement, "descendentElement");
			FrameworkElement parent = descendentElement.GetVisualParent();
			while (parent != null)
			{
				TAncestor item = parent as TAncestor;
				if (item != null)
				{
					yield return item;
				}
				parent = parent.GetVisualParent();
			}
		}
	}
}