using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
namespace org.mapsforge.map.writer
{

	using TShortIntHashMap = gnu.trove.map.hash.TShortIntHashMap;
	using TShortIntProcedure = gnu.trove.procedure.TShortIntProcedure;
	using TShortHashSet = gnu.trove.set.hash.TShortHashSet;



	using OSMTag = org.mapsforge.map.writer.model.OSMTag;
	using MapFileWriterTask = org.mapsforge.map.writer.osmosis.MapFileWriterTask;
	using Document = org.w3c.dom.Document;
	using Element = org.w3c.dom.Element;
	using NamedNodeMap = org.w3c.dom.NamedNodeMap;
	using Node = org.w3c.dom.Node;
	using NodeList = org.w3c.dom.NodeList;
	using SAXException = org.xml.sax.SAXException;
	using SAXParseException = org.xml.sax.SAXParseException;

	/// <summary>
	/// Reorders and maps tag ids according to their frequency in the input data. Ids are remapped so that the most frequent
	/// entities receive the lowest ids.
	/// </summary>
	public sealed class OSMTagMapping
	{
		private class HistogramEntry : IComparable<HistogramEntry>
		{
			private readonly OSMTagMapping outerInstance;

			internal readonly int amount;
			internal readonly short id;

			public HistogramEntry(OSMTagMapping outerInstance, short id, int amount) : base()
			{
				this.outerInstance = outerInstance;
				this.id = id;
				this.amount = amount;
			}

			/// <summary>
			/// First order: amount Second order: id (reversed order).
			/// </summary>
			public virtual int CompareTo(HistogramEntry o)
			{
				if (this.amount > o.amount)
				{
					return 1;
				}
				else if (this.amount < o.amount)
				{
					return -1;
				}
				else
				{
					if (this.id < o.id)
					{
						return 1;
					}
					else if (this.id > o.id)
					{
						return -1;
					}
					else
					{
						return 0;
					}
				}
			}

			public override bool Equals(object obj)
			{
				if (this == obj)
				{
					return true;
				}
				if (obj == null)
				{
					return false;
				}
				if (this.GetType() != obj.GetType())
				{
					return false;
				}
				HistogramEntry other = (HistogramEntry) obj;
				if (!OuterType.Equals(other.OuterType))
				{
					return false;
				}
				if (this.amount != other.amount)
				{
					return false;
				}
				if (this.id != other.id)
				{
					return false;
				}
				return true;
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + OuterType.GetHashCode();
				result = prime * result + this.amount;
				result = prime * result + this.id;
				return result;
			}

			internal virtual OSMTagMapping OuterType
			{
				get
				{
					return outerInstance;
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly Logger LOGGER = Logger.getLogger(typeof(OSMTagMapping).FullName);

		private static OSMTagMapping mapping;
		private const string XPATH_EXPRESSION_DEFAULT_ZOOM = "/tag-mapping/@default-zoom-appear";

		private const string XPATH_EXPRESSION_POIS = "//pois/osm-tag[" + "(../@enabled='true' or not(../@enabled)) and (./@enabled='true' or not(./@enabled)) " + "or (../@enabled='false' and ./@enabled='true')]";
		private const string XPATH_EXPRESSION_WAYS = "//ways/osm-tag[" + "(../@enabled='true' or not(../@enabled)) and (./@enabled='true' or not(./@enabled)) " + "or (../@enabled='false' and ./@enabled='true')]";

		/// <returns> a new instance </returns>
		public static OSMTagMapping Instance
		{
			get
			{
				lock (typeof(OSMTagMapping))
				{
					if (mapping == null)
					{
						mapping = getInstance(typeof(MapFileWriterTask).ClassLoader.getResource("tag-mapping.xml"));
					}
            
					return mapping;
				}
			}
		}

		/// <param name="tagConf">
		///            the <seealso cref="URL"/> to a file that contains a tag configuration </param>
		/// <returns> a new instance </returns>
		public static OSMTagMapping getInstance(URL tagConf)
		{
			if (mapping != null)
			{
				throw new System.InvalidOperationException("mapping already initialized");
			}

			mapping = new OSMTagMapping(tagConf);
			return mapping;
		}

		private readonly IDictionary<short?, OSMTag> idToPoiTag = new LinkedHashMap<short?, OSMTag>();
		private readonly IDictionary<short?, OSMTag> idToWayTag = new LinkedHashMap<short?, OSMTag>();

		private readonly IDictionary<short?, short?> optimizedPoiIds = new LinkedHashMap<short?, short?>();
		private readonly IDictionary<short?, short?> optimizedWayIds = new LinkedHashMap<short?, short?>();

		private short poiID = 0;

		private readonly IDictionary<short?, ISet<OSMTag>> poiZoomOverrides = new LinkedHashMap<short?, ISet<OSMTag>>();

		// we use LinkedHashMaps as they guarantee to uphold the
		// insertion order when iterating over the key or value "set"
		private readonly IDictionary<string, OSMTag> stringToPoiTag = new LinkedHashMap<string, OSMTag>();

		private readonly IDictionary<string, OSMTag> stringToWayTag = new LinkedHashMap<string, OSMTag>();

		private short wayID = 0;

		private readonly IDictionary<short?, ISet<OSMTag>> wayZoomOverrides = new LinkedHashMap<short?, ISet<OSMTag>>();

		private OSMTagMapping(URL tagConf)
		{
			try
			{
				sbyte defaultZoomAppear;

				// ---- Parse XML file ----
				DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
				DocumentBuilder builder = factory.newDocumentBuilder();
				Document document = builder.parse(tagConf.openStream());

				XPath xpath = XPathFactory.newInstance().newXPath();

				XPathExpression xe = xpath.compile(XPATH_EXPRESSION_DEFAULT_ZOOM);
				defaultZoomAppear = sbyte.Parse((string) xe.evaluate(document, XPathConstants.STRING));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.HashMap<Nullable<short>, java.util.Set<String>> tmpPoiZoomOverrides = new java.util.HashMap<>();
				Dictionary<short?, ISet<string>> tmpPoiZoomOverrides = new Dictionary<short?, ISet<string>>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.HashMap<Nullable<short>, java.util.Set<String>> tmpWayZoomOverrides = new java.util.HashMap<>();
				Dictionary<short?, ISet<string>> tmpWayZoomOverrides = new Dictionary<short?, ISet<string>>();

				// ---- Get list of poi nodes ----
				xe = xpath.compile(XPATH_EXPRESSION_POIS);
				NodeList pois = (NodeList) xe.evaluate(document, XPathConstants.NODESET);

				for (int i = 0; i < pois.Length; i++)
				{
					NamedNodeMap attributes = pois.item(i).Attributes;
					string key = attributes.getNamedItem("key").TextContent;
					string value = attributes.getNamedItem("value").TextContent;

					string[] equivalentValues = null;
					if (attributes.getNamedItem("equivalent-values") != null)
					{
						equivalentValues = attributes.getNamedItem("equivalent-values").TextContent.Split(",");
					}

					sbyte zoom = attributes.getNamedItem("zoom-appear") == null ? defaultZoomAppear : sbyte.Parse(attributes.getNamedItem("zoom-appear").TextContent);

					bool renderable = attributes.getNamedItem("renderable") == null ? true : bool.Parse(attributes.getNamedItem("renderable").TextContent);

					bool forcePolygonLine = attributes.getNamedItem("force-polygon-line") == null ? false : bool.Parse(attributes.getNamedItem("force-polygon-line").TextContent);

					OSMTag osmTag = new OSMTag(this.poiID, key, value, zoom, renderable, forcePolygonLine);
					if (this.stringToPoiTag.ContainsKey(osmTag.tagKey()))
					{
						LOGGER.warning("duplicate osm-tag found in tag-mapping configuration (ignoring): " + osmTag);
						continue;
					}
					LOGGER.finest("adding poi: " + osmTag);
					this.stringToPoiTag[osmTag.tagKey()] = osmTag;
					if (equivalentValues != null)
					{
						foreach (string equivalentValue in equivalentValues)
						{
							this.stringToPoiTag[OSMTag.tagKey(key, equivalentValue)] = osmTag;
						}
					}
					this.idToPoiTag[Convert.ToInt16(this.poiID)] = osmTag;

					// also fill optimization mapping with identity
					this.optimizedPoiIds[Convert.ToInt16(this.poiID)] = Convert.ToInt16(this.poiID);

					// check if this tag overrides the zoom level spec of another tag
					NodeList zoomOverrideNodes = pois.item(i).ChildNodes;
					for (int j = 0; j < zoomOverrideNodes.Length; j++)
					{
						Node overriddenNode = zoomOverrideNodes.item(j);
						if (overriddenNode is Element)
						{
							string keyOverridden = overriddenNode.Attributes.getNamedItem("key").TextContent;
							string valueOverridden = overriddenNode.Attributes.getNamedItem("value").TextContent;
							ISet<string> s = tmpPoiZoomOverrides[Convert.ToInt16(this.poiID)];
							if (s == null)
							{
								s = new HashSet<>();
								tmpPoiZoomOverrides[Convert.ToInt16(this.poiID)] = s;
							}
							s.Add(OSMTag.tagKey(keyOverridden, valueOverridden));
						}
					}

					this.poiID++;
				}

				// ---- Get list of way nodes ----
				xe = xpath.compile(XPATH_EXPRESSION_WAYS);
				NodeList ways = (NodeList) xe.evaluate(document, XPathConstants.NODESET);

				for (int i = 0; i < ways.Length; i++)
				{
					NamedNodeMap attributes = ways.item(i).Attributes;
					string key = attributes.getNamedItem("key").TextContent;
					string value = attributes.getNamedItem("value").TextContent;

					string[] equivalentValues = null;
					if (attributes.getNamedItem("equivalent-values") != null)
					{
						equivalentValues = attributes.getNamedItem("equivalent-values").TextContent.Split(",");
					}

					sbyte zoom = attributes.getNamedItem("zoom-appear") == null ? defaultZoomAppear : sbyte.Parse(attributes.getNamedItem("zoom-appear").TextContent);

					bool renderable = attributes.getNamedItem("renderable") == null ? true : bool.Parse(attributes.getNamedItem("renderable").TextContent);

					bool forcePolygonLine = attributes.getNamedItem("force-polygon-line") == null ? false : bool.Parse(attributes.getNamedItem("force-polygon-line").TextContent);

					OSMTag osmTag = new OSMTag(this.wayID, key, value, zoom, renderable, forcePolygonLine);
					if (this.stringToWayTag.ContainsKey(osmTag.tagKey()))
					{
						LOGGER.warning("duplicate osm-tag found in tag-mapping configuration (ignoring): " + osmTag);
						continue;
					}
					LOGGER.finest("adding way: " + osmTag);
					this.stringToWayTag[osmTag.tagKey()] = osmTag;
					if (equivalentValues != null)
					{
						foreach (string equivalentValue in equivalentValues)
						{
							this.stringToWayTag[OSMTag.tagKey(key, equivalentValue)] = osmTag;
						}
					}
					this.idToWayTag[Convert.ToInt16(this.wayID)] = osmTag;

					// also fill optimization mapping with identity
					this.optimizedWayIds[Convert.ToInt16(this.wayID)] = Convert.ToInt16(this.wayID);

					// check if this tag overrides the zoom level spec of another tag
					NodeList zoomOverrideNodes = ways.item(i).ChildNodes;
					for (int j = 0; j < zoomOverrideNodes.Length; j++)
					{
						Node overriddenNode = zoomOverrideNodes.item(j);
						if (overriddenNode is Element)
						{
							string keyOverridden = overriddenNode.Attributes.getNamedItem("key").TextContent;
							string valueOverridden = overriddenNode.Attributes.getNamedItem("value").TextContent;
							ISet<string> s = tmpWayZoomOverrides[Convert.ToInt16(this.wayID)];
							if (s == null)
							{
								s = new HashSet<>();
								tmpWayZoomOverrides[Convert.ToInt16(this.wayID)] = s;
							}
							s.Add(OSMTag.tagKey(keyOverridden, valueOverridden));
						}
					}

					this.wayID++;
				}

				// copy temporary values from zoom-override data sets
				foreach (KeyValuePair<short?, ISet<string>> entry in tmpPoiZoomOverrides.SetOfKeyValuePairs())
				{
					ISet<OSMTag> overriddenTags = new HashSet<OSMTag>();
					foreach (string tagString in entry.Value)
					{
						OSMTag tag = this.stringToPoiTag[tagString];
						if (tag != null)
						{
							overriddenTags.Add(tag);
						}
					}
					if (overriddenTags.Count > 0)
					{
						this.poiZoomOverrides[entry.Key] = overriddenTags;
					}
				}

				foreach (KeyValuePair<short?, ISet<string>> entry in tmpWayZoomOverrides.SetOfKeyValuePairs())
				{
					ISet<OSMTag> overriddenTags = new HashSet<OSMTag>();
					foreach (string tagString in entry.Value)
					{
						OSMTag tag = this.stringToWayTag[tagString];
						if (tag != null)
						{
							overriddenTags.Add(tag);
						}
					}
					if (overriddenTags.Count > 0)
					{
						this.wayZoomOverrides[entry.Key] = overriddenTags;
					}
				}

				// ---- Error handling ----
			}
			catch (SAXParseException spe)
			{
				LOGGER.severe("\n** Parsing error, line " + spe.LineNumber + ", uri " + spe.SystemId);
				throw new System.InvalidOperationException(spe);
			}
			catch (SAXException sxe)
			{
				throw new System.InvalidOperationException(sxe);
			}
			catch (ParserConfigurationException pce)
			{
				throw new System.InvalidOperationException(pce);
			}
			catch (IOException ioe)
			{
				throw new System.InvalidOperationException(ioe);
			}
			catch (XPathExpressionException e)
			{
				throw new System.InvalidOperationException(e);
			}
		}

		/// <returns> a mapping that maps original tag ids to the optimized ones </returns>
		public IDictionary<short?, short?> OptimizedPoiIds
		{
			get
			{
				return this.optimizedPoiIds;
			}
		}

		/// <returns> a mapping that maps original tag ids to the optimized ones </returns>
		public IDictionary<short?, short?> OptimizedWayIds
		{
			get
			{
				return this.optimizedWayIds;
			}
		}

		/// <param name="id">
		///            the id </param>
		/// <returns> the corresponding <seealso cref="OSMTag"/> </returns>
		public OSMTag getPoiTag(short id)
		{
			return this.idToPoiTag[Convert.ToInt16(id)];
		}

		/// <param name="key">
		///            the key </param>
		/// <param name="value">
		///            the value </param>
		/// <returns> the corresponding <seealso cref="OSMTag"/> </returns>
		public OSMTag getPoiTag(string key, string value)
		{
			return this.stringToPoiTag[OSMTag.tagKey(key, value)];
		}

		/// <param name="id">
		///            the id </param>
		/// <returns> the corresponding <seealso cref="OSMTag"/> </returns>
		public OSMTag getWayTag(short id)
		{
			return this.idToWayTag[Convert.ToInt16(id)];
		}

		// /**
		// * @param tags
		// * the tags
		// * @return
		// */
		// private static short[] tagIDsFromList(List<OSMTag> tags) {
		// short[] tagIDs = new short[tags.size()];
		// int i = 0;
		// for (OSMTag tag : tags) {
		// tagIDs[i++] = tag.getId();
		// }
		//
		// return tagIDs;
		// }

		/// <param name="key">
		///            the key </param>
		/// <param name="value">
		///            the value </param>
		/// <returns> the corresponding <seealso cref="OSMTag"/> </returns>
		public OSMTag getWayTag(string key, string value)
		{
			return this.stringToWayTag[OSMTag.tagKey(key, value)];
		}

		/// <param name="tagSet">
		///            the tag set </param>
		/// <returns> the minimum zoom level of all tags in the tag set </returns>
		public sbyte getZoomAppearPOI(short[] tagSet)
		{
			if (tagSet == null || tagSet.Length == 0)
			{
				return sbyte.MaxValue;
			}

			TShortHashSet tmp = new TShortHashSet(tagSet);

			if (this.poiZoomOverrides.Count > 0)
			{
				foreach (short s in tagSet)
				{
					ISet<OSMTag> overriddenTags = this.poiZoomOverrides[Convert.ToInt16(s)];
					if (overriddenTags != null)
					{
						foreach (OSMTag osmTag in overriddenTags)
						{
							tmp.remove(osmTag.Id);
						}
					}
				}

				if (tmp.Empty)
				{
					StringBuilder sb = new StringBuilder();
					foreach (short s in tagSet)
					{
						sb.Append(this.idToPoiTag[Convert.ToInt16(s)].tagKey() + "; ");
					}
					LOGGER.severe("ERROR: You have a cycle in your zoom-override definitions. Look for these tags: " + sb.ToString());
				}
			}

			sbyte zoomAppear = sbyte.MaxValue;
			foreach (short s in tmp.toArray())
			{
				OSMTag tag = this.idToPoiTag[Convert.ToInt16(s)];
				if (tag.Renderable)
				{
					zoomAppear = (sbyte) Math.Min(zoomAppear, tag.ZoomAppear);
				}
			}

			return zoomAppear;
		}

		/// <param name="tagSet">
		///            the tag set </param>
		/// <returns> the minimum zoom level of all the tags in the set </returns>
		public sbyte getZoomAppearWay(short[] tagSet)
		{
			if (tagSet == null || tagSet.Length == 0)
			{
				return sbyte.MaxValue;
			}

			TShortHashSet tmp = new TShortHashSet(tagSet);

			if (this.wayZoomOverrides.Count > 0)
			{
				foreach (short s in tagSet)
				{
					ISet<OSMTag> overriddenTags = this.wayZoomOverrides[Convert.ToInt16(s)];
					if (overriddenTags != null)
					{
						foreach (OSMTag osmTag in overriddenTags)
						{
							tmp.remove(osmTag.Id);
						}
					}
				}

				if (tmp.Empty)
				{
					StringBuilder sb = new StringBuilder();
					foreach (short s in tagSet)
					{
						sb.Append(this.idToWayTag[Convert.ToInt16(s)].tagKey() + "; ");
					}
					LOGGER.severe("ERROR: You have a cycle in your zoom-override definitions. Look for these tags: " + sb.ToString());
				}
			}
			sbyte zoomAppear = sbyte.MaxValue;
			foreach (short s in tmp.toArray())
			{
				OSMTag tag = this.idToWayTag[Convert.ToInt16(s)];
				if (tag.Renderable)
				{
					zoomAppear = (sbyte) Math.Min(zoomAppear, tag.ZoomAppear);
				}
			}

			return zoomAppear;
		}

		/// <param name="histogram">
		///            a histogram that represents the frequencies of tags </param>
		public void optimizePoiOrdering(TShortIntHashMap histogram)
		{
			this.optimizedPoiIds.Clear();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.TreeSet<HistogramEntry> poiOrdering = new java.util.TreeSet<>();
			SortedSet<HistogramEntry> poiOrdering = new SortedSet<HistogramEntry>();

			histogram.forEachEntry(new TShortIntProcedureAnonymousInnerClassHelper(this, poiOrdering));

			short tmpPoiID = 0;

			OSMTag currentTag = null;
			foreach (HistogramEntry histogramEntry in poiOrdering.descendingSet())
			{
				currentTag = this.idToPoiTag[Convert.ToInt16(histogramEntry.id)];
				this.optimizedPoiIds[Convert.ToInt16(histogramEntry.id)] = Convert.ToInt16(tmpPoiID);
				LOGGER.finer("adding poi tag: " + currentTag.tagKey() + " id:" + tmpPoiID + " amount: " + histogramEntry.amount);
				tmpPoiID++;
			}
		}

		private class TShortIntProcedureAnonymousInnerClassHelper : TShortIntProcedure
		{
			private readonly OSMTagMapping outerInstance;

			private SortedSet<HistogramEntry> poiOrdering;

			public TShortIntProcedureAnonymousInnerClassHelper(OSMTagMapping outerInstance, SortedSet<HistogramEntry> poiOrdering)
			{
				this.outerInstance = outerInstance;
				this.poiOrdering = poiOrdering;
			}

			public override bool execute(short tag, int amount)
			{
				poiOrdering.Add(new HistogramEntry(outerInstance, tag, amount));
				return true;
			}
		}

		/// <param name="histogram">
		///            a histogram that represents the frequencies of tags </param>
		public void optimizeWayOrdering(TShortIntHashMap histogram)
		{
			this.optimizedWayIds.Clear();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.TreeSet<HistogramEntry> wayOrdering = new java.util.TreeSet<>();
			SortedSet<HistogramEntry> wayOrdering = new SortedSet<HistogramEntry>();

			histogram.forEachEntry(new TShortIntProcedureAnonymousInnerClassHelper2(this, wayOrdering));
			short tmpWayID = 0;

			OSMTag currentTag = null;
			foreach (HistogramEntry histogramEntry in wayOrdering.descendingSet())
			{
				currentTag = this.idToWayTag[Convert.ToInt16(histogramEntry.id)];
				this.optimizedWayIds[Convert.ToInt16(histogramEntry.id)] = Convert.ToInt16(tmpWayID);
				LOGGER.finer("adding way tag: " + currentTag.tagKey() + " id:" + tmpWayID + " amount: " + histogramEntry.amount);
				tmpWayID++;
			}
		}

		private class TShortIntProcedureAnonymousInnerClassHelper2 : TShortIntProcedure
		{
			private readonly OSMTagMapping outerInstance;

			private SortedSet<HistogramEntry> wayOrdering;

			public TShortIntProcedureAnonymousInnerClassHelper2(OSMTagMapping outerInstance, SortedSet<HistogramEntry> wayOrdering)
			{
				this.outerInstance = outerInstance;
				this.wayOrdering = wayOrdering;
			}

			public override bool execute(short tag, int amount)
			{
				wayOrdering.Add(new HistogramEntry(outerInstance, tag, amount));
				return true;
			}
		}
	}

}