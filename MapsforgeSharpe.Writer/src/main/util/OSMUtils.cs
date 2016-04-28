using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2015 devemux86
 * Copyright 2015 lincomatic
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
namespace org.mapsforge.map.writer.util
{

	using TShortArrayList = gnu.trove.list.array.TShortArrayList;


	using OSMTag = org.mapsforge.map.writer.model.OSMTag;
	using SpecialTagExtractionResult = org.mapsforge.map.writer.model.SpecialTagExtractionResult;
	using Entity = org.openstreetmap.osmosis.core.domain.v0_6.Entity;
	using Tag = org.openstreetmap.osmosis.core.domain.v0_6.Tag;
	using Way = org.openstreetmap.osmosis.core.domain.v0_6.Way;

	/// <summary>
	/// OpenStreetMap related utility methods.
	/// </summary>
	public sealed class OSMUtils
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly Logger LOGGER = Logger.getLogger(typeof(OSMUtils).FullName);

		private const int MAX_ELEVATION = 9000;

		private static readonly Pattern NAME_LANGUAGE_PATTERN = Pattern.compile("(name)(:)([a-zA-Z]{1,3}(?:[-_][a-zA-Z0-9]{1,8})*)");

		/// <summary>
		/// Extracts known POI tags and returns their ids.
		/// </summary>
		/// <param name="entity">
		///            the node </param>
		/// <returns> the ids of the identified tags </returns>
		public static short[] extractKnownPOITags(Entity entity)
		{
			TShortArrayList currentTags = new TShortArrayList();
			OSMTagMapping mapping = OSMTagMapping.Instance;
			if (entity.Tags != null)
			{
				foreach (Tag tag in entity.Tags)
				{
					OSMTag wayTag = mapping.getPoiTag(tag.Key, tag.Value);
					if (wayTag != null)
					{
						currentTags.add(wayTag.Id);
					}
				}
			}
			return currentTags.toArray();
		}

		/// <summary>
		/// Extracts known way tags and returns their ids.
		/// </summary>
		/// <param name="entity">
		///            the way </param>
		/// <returns> the ids of the identified tags </returns>
		public static short[] extractKnownWayTags(Entity entity)
		{
			TShortArrayList currentTags = new TShortArrayList();
			OSMTagMapping mapping = OSMTagMapping.Instance;
			if (entity.Tags != null)
			{
				foreach (Tag tag in entity.Tags)
				{
					OSMTag wayTag = mapping.getWayTag(tag.Key, tag.Value);
					if (wayTag != null)
					{
						currentTags.add(wayTag.Id);
					}
				}
			}
			return currentTags.toArray();
		}

		/// <summary>
		/// Extracts special fields and returns their values as an array of strings.
		/// <para>
		/// Use '\r' delimiter among names and '\b' delimiter between each language and name.  
		/// 
		/// </para>
		/// </summary>
		/// <param name="entity">
		///            the entity </param>
		/// <param name="preferredLanguages">
		///            the preferred language(s) </param>
		/// <returns> a string array, [0] = name, [1] = ref, [2} = housenumber, [3] layer, [4] elevation, [5] relationType </returns>
		public static SpecialTagExtractionResult extractSpecialFields(Entity entity, IList<string> preferredLanguages)
		{
			string name = null;
			string @ref = null;
			string housenumber = null;
			sbyte layer = 5;
			short elevation = 0;
			string relationType = null;

			if (entity.Tags != null)
			{
				// Process 'name' tags
				if (preferredLanguages != null && preferredLanguages.Count > 1)
				{ // Multilingual map
					// Convert tag collection to list and sort it
					// i.e. making sure default 'name' comes first
					IList<Tag> tags = new List<Tag>(entity.Tags);
					tags.Sort();

					string defaultName = null;
					IList<string> restPreferredLanguages = new List<string>(preferredLanguages);
					foreach (Tag tag in tags)
					{
						string key = tag.Key.ToLower(Locale.ENGLISH);
						if ("name".Equals(key))
						{ // Default 'name'
							defaultName = tag.Value;
							name = defaultName;
						}
						else
						{ // Localized name
							Matcher matcher = NAME_LANGUAGE_PATTERN.matcher(key);
							if (!matcher.matches())
							{
								continue;
							}
							if (tag.Value.Equals(defaultName))
							{ // Same with default 'name'?
								continue;
							}
							string language = matcher.group(3).ToLower(Locale.ENGLISH).Replace('_', '-');
							if (preferredLanguages.Contains(language))
							{
								restPreferredLanguages.Remove(language);
								name = (!string.ReferenceEquals(name, null) ? name + '\r' : "") + language + '\b' + tag.Value;
							}
						}
					}

					// Check rest preferred languages for falling back to base
					if (restPreferredLanguages.Count > 0)
					{
						IDictionary<string, string> fallbacks = new Dictionary<string, string>();
						foreach (string preferredLanguage in restPreferredLanguages)
						{
							foreach (Tag tag in tags)
							{
								string key = tag.Key.ToLower(Locale.ENGLISH);
								Matcher matcher = NAME_LANGUAGE_PATTERN.matcher(key);
								if (!matcher.matches())
								{
									continue;
								}
								if (tag.Value.Equals(defaultName))
								{ // Same with default 'name'?
									continue;
								}
								string language = matcher.group(3).ToLower(Locale.ENGLISH).Replace('_', '-');
								if (!fallbacks.ContainsKey(language) && !language.Contains("-") && (preferredLanguage.Contains("-") || preferredLanguage.Contains("_")) && preferredLanguage.ToLower(Locale.ENGLISH).StartsWith(language, StringComparison.Ordinal))
								{
									fallbacks[language] = tag.Value;
								}
							}
						}
						foreach (string language in fallbacks.Keys)
						{
							name = (!string.ReferenceEquals(name, null) ? name + '\r' : "") + language + '\b' + fallbacks[language];
						}
					}
				}
				else
				{ // Non multilingual map
					bool foundPreferredLanguageName = false;
					foreach (Tag tag in entity.Tags)
					{
						string key = tag.Key.ToLower(Locale.ENGLISH);
						if ("name".Equals(key) && !foundPreferredLanguageName)
						{
							name = tag.Value;
						}
						else if (preferredLanguages != null && !foundPreferredLanguageName)
						{
							Matcher matcher = NAME_LANGUAGE_PATTERN.matcher(key);
							if (matcher.matches())
							{
								string language = matcher.group(3);
								if (language.Equals(preferredLanguages[0], StringComparison.CurrentCultureIgnoreCase))
								{
									name = tag.Value;
									foundPreferredLanguageName = true;
								}
							}
						}
					}
				}

				// Process rest tags
				foreach (Tag tag in entity.Tags)
				{
					string key = tag.Key.ToLower(Locale.ENGLISH);
					if ("piste:name".Equals(key) && string.ReferenceEquals(name, null))
					{
						name = tag.Value;
					}
					else if ("addr:housenumber".Equals(key))
					{
						housenumber = tag.Value;
					}
					else if ("ref".Equals(key))
					{
						@ref = tag.Value;
					}
					else if ("layer".Equals(key))
					{
						string l = tag.Value;
						try
						{
							sbyte testLayer = sbyte.Parse(l);
							if (testLayer >= -5 && testLayer <= 5)
							{
								testLayer += 5;
							}
							layer = testLayer;
						}
						catch (System.FormatException)
						{
							LOGGER.finest("could not parse layer information to byte type: " + tag.Value + "\t entity-id: " + entity.Id + "\tentity-type: " + entity.Type.name());
						}
					}
					else if ("ele".Equals(key))
					{
						string strElevation = tag.Value;
						strElevation = strElevation.replaceAll("m", "");
						strElevation = strElevation.replaceAll(",", ".");
						try
						{
							double testElevation = double.Parse(strElevation);
							if (testElevation < MAX_ELEVATION)
							{
								elevation = (short) testElevation;
							}
						}
						catch (System.FormatException)
						{
							LOGGER.finest("could not parse elevation information to double type: " + tag.Value + "\t entity-id: " + entity.Id + "\tentity-type: " + entity.Type.name());
						}
					}
					else if ("type".Equals(key))
					{
						relationType = tag.Value;
					}
				}
			}

			return new SpecialTagExtractionResult(name, @ref, housenumber, layer, elevation, relationType);
		}


		/// <summary>
		/// Heuristic to determine from attributes if a way is likely to be an area.
		/// Precondition for this call is that the first and last node of a way are the
		/// same, so that this method should only return false if it is known that the
		/// feature should not be an area even if the geometry is a polygon.
		/// 
		/// Determining what is an area is neigh impossible in OSM, this method inspects tag elements
		/// to give a likely answer. See http://wiki.openstreetmap.org/wiki/The_Future_of_Areas and
		/// http://wiki.openstreetmap.org/wiki/Way
		/// </summary>
		/// <param name="way">
		///            the way (which is assumed to be closed and have enough nodes to be an area) </param>
		/// <returns> true if tags indicate this is an area, otherwise false. </returns>
		public static bool isArea(Way way)
		{
			bool result = true;
			if (way.Tags != null)
			{
				foreach (Tag tag in way.Tags)
				{
					string key = tag.Key.ToLower(Locale.ENGLISH);
					string value = tag.Value.ToLower(Locale.ENGLISH);
					if ("area".Equals(key))
					{
						// obvious result
						if (("yes").Equals(value) || ("y").Equals(value) || ("true").Equals(value))
						{
							return true;
						}
						if (("no").Equals(value) || ("n").Equals(value) || ("false").Equals(value))
						{
							return false;
						}
					}
					if ("highway".Equals(key) || "barrier".Equals(key))
					{
						// false unless something else overrides this.
						result = false;
					}
					if ("railway".Equals(key))
					{
						// there is more to the railway tag then just rails, this excludes the
						// most common railway lines from being detected as areas if they are closed.
						// Since this method is only called if the first and last node are the same
						// this should be safe
						if ("rail".Equals(value) || "tram".Equals(value) || "subway".Equals(value) || "monorail".Equals(value) || "narrow_gauge".Equals(value) || "preserved".Equals(value) || "light_rail".Equals(value) || "construction".Equals(value))
						{
							result = false;
						}
					}
				}
			}
			return result;
		}

		private OSMUtils()
		{
		}
	}

}