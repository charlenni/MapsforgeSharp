/*
 * Copyright 2014 Ludwig M Brinckmann
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

namespace org.mapsforge.map.rendertheme
{
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using MapsforgeSharp.Core.Util;

	/*
	 * An individual layer in the rendertheme V4+ menu system.
	 * A layer can have translations, categories that will always be enabled
	 * when the layer is selected as well as optional overlays.
	 */
	[DataContract]
	public class XmlRenderThemeStyleLayer
	{
		private const long serialVersionUID = 1L;

		private readonly ISet<string> categories;
		private readonly string defaultLanguage;
		private readonly string id;
		private readonly IList<XmlRenderThemeStyleLayer> overlays;
		private readonly IDictionary<string, string> titles;
		private readonly bool visible;
		private readonly bool enabled;

		internal XmlRenderThemeStyleLayer(string id, bool visible, bool enabled, string defaultLanguage)
		{
			this.id = id;
			this.titles = new Dictionary<string, string>();
			this.categories = new LinkedHashSet<string>();
			this.visible = visible;
			this.defaultLanguage = defaultLanguage;
			this.enabled = enabled;
			this.overlays = new List<XmlRenderThemeStyleLayer>();
		}

		public virtual void AddCategory(string category)
		{
			this.categories.Add(category);
		}

		public virtual void AddOverlay(XmlRenderThemeStyleLayer overlay)
		{
			this.overlays.Add(overlay);
		}

		public virtual void AddTranslation(string language, string name)
		{
			this.titles[language] = name;
		}

		public virtual ISet<string> Categories
		{
			get
			{
				return this.categories;
			}
		}

		public virtual string Id
		{
			get
			{
				return this.id;
			}
		}

		public virtual IList<XmlRenderThemeStyleLayer> Overlays
		{
			get
			{
				return this.overlays;
			}
		}

		public virtual string GetTitle(string language)
		{
			return this.titles[language] ?? this.titles[this.defaultLanguage];
		}

		public virtual IDictionary<string, string> Titles
		{
			get
			{
				return this.titles;
			}
		}

		public virtual bool Enabled
		{
			get
			{
				return this.enabled;
			}
		}

		public virtual bool Visible
		{
			get
			{
				return this.visible;
			}
		}
	}
}