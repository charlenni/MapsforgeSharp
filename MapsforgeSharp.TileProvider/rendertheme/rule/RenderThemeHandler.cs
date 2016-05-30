/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014 devemux86
 * Copyright 2016 Dirk Weltz
 *
 * This program is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace org.mapsforge.map.rendertheme.rule
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.IO;
    using Acrotech.PortableLogAdapter;

    using IGraphicFactory = MapsforgeSharp.Core.Graphics.IGraphicFactory;
    using DisplayModel = org.mapsforge.map.model.DisplayModel;
    using Area = org.mapsforge.map.rendertheme.renderinstruction.Area;
    using Caption = org.mapsforge.map.rendertheme.renderinstruction.Caption;
    using Circle = org.mapsforge.map.rendertheme.renderinstruction.Circle;
    using Line = org.mapsforge.map.rendertheme.renderinstruction.Line;
    using LineSymbol = org.mapsforge.map.rendertheme.renderinstruction.LineSymbol;
    using PathText = org.mapsforge.map.rendertheme.renderinstruction.PathText;
    using RenderInstruction = org.mapsforge.map.rendertheme.renderinstruction.RenderInstruction;
    using Symbol = org.mapsforge.map.rendertheme.renderinstruction.Symbol;

    /// <summary>
    /// KXML handler to parse XML render theme files.
    /// </summary>
    public sealed class RenderThemeHandler
	{
		private enum Element
		{
			RENDER_THEME,
			RENDERING_INSTRUCTION,
			RULE,
			RENDERING_STYLE
		}

        private static readonly ILogger LOGGER = (new Acrotech.PortableLogAdapter.Managers.DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info)).GetLogger(nameof(RenderThemeHandler));
        private const string ELEMENT_NAME_RULE = "rule";
		private const string UNEXPECTED_ELEMENT = "unexpected element: ";

		public static RenderTheme GetRenderTheme(IGraphicFactory graphicFactory, DisplayModel displayModel, IXmlRenderTheme xmlRenderTheme)
		{
            XmlReader reader = null;

            using (System.IO.Stream inputStream = xmlRenderTheme.RenderThemeAsStream)
            {
                inputStream.Position = 0;
                reader = XmlReader.Create(inputStream);

                RenderThemeHandler renderThemeHandler = new RenderThemeHandler(graphicFactory, displayModel, xmlRenderTheme.RelativePathPrefix, xmlRenderTheme, reader);

                renderThemeHandler.ProcessRenderTheme();
                return renderThemeHandler.renderTheme;
            }
        }

        private ISet<string> categories;
		private Rule currentRule;
		private readonly DisplayModel displayModel;
		private readonly Stack<Element> elementStack = new Stack<Element>();
		private readonly IGraphicFactory graphicFactory;
		private int level;
		private readonly XmlReader reader;
		private string qName;
		private readonly string relativePathPrefix;
		private RenderTheme renderTheme;
		private readonly Stack<Rule> ruleStack = new Stack<Rule>();
		private IDictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
		private readonly IXmlRenderTheme xmlRenderTheme;
		private XmlRenderThemeStyleMenu renderThemeStyleMenu;
		private XmlRenderThemeStyleLayer currentLayer;

		private RenderThemeHandler(IGraphicFactory graphicFactory, DisplayModel displayModel, string relativePathPrefix, IXmlRenderTheme xmlRenderTheme, XmlReader reader) : base()
		{
			this.reader = reader;
			this.graphicFactory = graphicFactory;
			this.displayModel = displayModel;
			this.relativePathPrefix = relativePathPrefix;
			this.xmlRenderTheme = xmlRenderTheme;
		}

		public void ProcessRenderTheme()
		{
			//reader.MoveToContent();

            while (reader.Read())
			{
                if (reader.NodeType == XmlNodeType.Element)
				{
                    var empty = reader.IsEmptyElement;

                    StartElement();

                    if (empty)
                    {
                        EndElement();
                    }
				}
				else if (reader.NodeType == XmlNodeType.EndElement)
				{
					EndElement();
				}
			}

			EndDocument();
		}


		private void EndDocument()
		{
			if (this.renderTheme == null)
			{
				throw new System.ArgumentException("missing element: rules");
			}

			this.renderTheme.Levels = this.level;
			this.renderTheme.Complete();
		}

		private void EndElement()
		{
			qName = reader.Name;

			this.elementStack.Pop();

			if (ELEMENT_NAME_RULE.Equals(qName))
			{
				this.ruleStack.Pop();
				if (this.ruleStack.Count == 0)
				{
					if (IsVisible(this.currentRule))
					{
						this.renderTheme.AddRule(this.currentRule);
					}
				}
				else
				{
					this.currentRule = this.ruleStack.Peek();
				}
			}
			else if ("stylemenu".Equals(qName))
			{
				// when we are finished parsing the menu part of the file, we can get the
				// categories to render from the initiator. This allows the creating action
				// to select which of the menu options to choose
				if (null != this.xmlRenderTheme.MenuCallback)
				{
					// if there is no callback, there is no menu, so the categories will be null
					this.categories = this.xmlRenderTheme.MenuCallback.GetCategories(this.renderThemeStyleMenu);
				}
				return;
			}

		}

		private void StartElement()
		{
			qName = reader.Name;

			try
			{
				if ("rendertheme".Equals(qName))
				{
					CheckState(qName, Element.RENDER_THEME);
					this.renderTheme = (new RenderThemeBuilder(qName, reader)).Build();
				}

				else if (ELEMENT_NAME_RULE.Equals(qName))
				{
					CheckState(qName, Element.RULE);
					Rule rule = (new RuleBuilder(qName, reader, this.ruleStack)).Build();
					if (this.ruleStack.Count > 0 && IsVisible(rule))
					{
						this.currentRule.AddSubRule(rule);
					}
					this.currentRule = rule;
					this.ruleStack.Push(this.currentRule);
				}

				else if ("area".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_INSTRUCTION);
					Area area = new Area(this.graphicFactory, this.displayModel, qName, reader, this.level++, this.relativePathPrefix);
					if (IsVisible(area))
					{
						this.currentRule.AddRenderingInstruction(area);
					}
				}

				else if ("caption".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_INSTRUCTION);
					Caption caption = new Caption(this.graphicFactory, this.displayModel, qName, reader, symbols);
					if (IsVisible(caption))
					{
						this.currentRule.AddRenderingInstruction(caption);
					}
				}

				else if ("cat".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_STYLE);
					this.currentLayer.AddCategory(GetStringAttribute("id"));
				}

				else if ("circle".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_INSTRUCTION);
					Circle circle = new Circle(this.graphicFactory, this.displayModel, qName, reader, this.level++);
					if (IsVisible(circle))
					{
						this.currentRule.AddRenderingInstruction(circle);
					}
				}

				// rendertheme menu layer
				else if ("layer".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_STYLE);
					bool enabled = false;
					if (!string.ReferenceEquals(GetStringAttribute("enabled"), null))
					{
						enabled = Convert.ToBoolean(GetStringAttribute("enabled"));
					}
					bool visible = Convert.ToBoolean(GetStringAttribute("visible"));
					this.currentLayer = this.renderThemeStyleMenu.CreateLayer(GetStringAttribute("id"), visible, enabled);
					string parent = GetStringAttribute("parent");
					if (null != parent)
					{
						XmlRenderThemeStyleLayer parentEntry = this.renderThemeStyleMenu.GetLayer(parent);
						if (null != parentEntry)
						{
							foreach (string cat in parentEntry.Categories)
							{
								this.currentLayer.AddCategory(cat);
							}
							foreach (XmlRenderThemeStyleLayer overlay in parentEntry.Overlays)
							{
								this.currentLayer.AddOverlay(overlay);
							}
						}
					}
				}

				else if ("line".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_INSTRUCTION);
					Line line = new Line(this.graphicFactory, this.displayModel, qName, reader, this.level++, this.relativePathPrefix);
					if (IsVisible(line))
					{
						this.currentRule.AddRenderingInstruction(line);
					}
				}

				else if ("lineSymbol".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_INSTRUCTION);
					LineSymbol lineSymbol = new LineSymbol(this.graphicFactory, this.displayModel, qName, reader, this.relativePathPrefix);
					if (IsVisible(lineSymbol))
					{
						this.currentRule.AddRenderingInstruction(lineSymbol);
					}
				}

				// render theme menu name
				else if ("name".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_STYLE);
					this.currentLayer.AddTranslation(GetStringAttribute("lang"), GetStringAttribute("value"));
				}

				// render theme menu overlay
				else if ("overlay".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_STYLE);
					XmlRenderThemeStyleLayer overlay = this.renderThemeStyleMenu.GetLayer(GetStringAttribute("id"));
					if (overlay != null)
					{
						this.currentLayer.AddOverlay(overlay);
					}
				}

				else if ("pathText".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_INSTRUCTION);
					PathText pathText = new PathText(this.graphicFactory, this.displayModel, qName, reader);
					if (IsVisible(pathText))
					{
						this.currentRule.AddRenderingInstruction(pathText);
					}
				}

				else if ("stylemenu".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_STYLE);

					this.renderThemeStyleMenu = new XmlRenderThemeStyleMenu(GetStringAttribute("id"), GetStringAttribute("defaultlang"), GetStringAttribute("defaultvalue"));
				}

				else if ("symbol".Equals(qName))
				{
					CheckState(qName, Element.RENDERING_INSTRUCTION);
					Symbol symbol = new Symbol(this.graphicFactory, this.displayModel, qName, reader, this.relativePathPrefix);
					this.currentRule.AddRenderingInstruction(symbol);
					string symbolId = symbol.Id;
					if (!string.ReferenceEquals(symbolId, null))
					{
						this.symbols[symbolId] = symbol;
					}
				}

				else
				{
					throw new InvalidOperationException("unknown element: " + qName);
				}
			}
			catch (IOException e)
			{
				LOGGER.Warn("Rendertheme missing or invalid resource " + e.Message);
			}
		}

		private void CheckElement(string elementName, Element element)
		{
			switch (element)
			{
				case org.mapsforge.map.rendertheme.rule.RenderThemeHandler.Element.RENDER_THEME:
					if (this.elementStack.Count > 0)
					{
						throw new InvalidOperationException(UNEXPECTED_ELEMENT + elementName);
					}
					return;

				case org.mapsforge.map.rendertheme.rule.RenderThemeHandler.Element.RULE:
					Element parentElement = this.elementStack.Peek();
					if (parentElement != Element.RENDER_THEME && parentElement != Element.RULE)
					{
						throw new InvalidOperationException(UNEXPECTED_ELEMENT + elementName);
					}
					return;

				case org.mapsforge.map.rendertheme.rule.RenderThemeHandler.Element.RENDERING_INSTRUCTION:
					if (this.elementStack.Peek() != Element.RULE)
					{
						throw new InvalidOperationException(UNEXPECTED_ELEMENT + elementName);
					}
					return;

				case org.mapsforge.map.rendertheme.rule.RenderThemeHandler.Element.RENDERING_STYLE:
					return;
			}

			throw new InvalidOperationException("unknown enum value: " + element);
		}

		private void CheckState(string elementName, Element element)
		{
			CheckElement(elementName, element);
			this.elementStack.Push(element);
		}

		private string GetStringAttribute(string name)
		{
			int n = reader.AttributeCount;
			for (int i = 0; i < n; i++)
			{
                reader.MoveToAttribute(i);

				if (reader.Name.Equals(name))
				{
					return reader.Value;
				}
			}
			return null;
		}

		private bool IsVisible(RenderInstruction renderInstruction)
		{
			return this.categories == null || renderInstruction.Category == null || this.categories.Contains(renderInstruction.Category);
		}

		private bool IsVisible(Rule rule)
		{
			// a rule is visible if categories is not set, the rule has not category or the
			// categories contain this rule's category
			return this.categories == null || rule.cat == null || this.categories.Contains(rule.cat);
		}
	}
}