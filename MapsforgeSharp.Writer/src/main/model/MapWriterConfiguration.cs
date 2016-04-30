using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
namespace org.mapsforge.map.writer.model
{


	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using LatLong = org.mapsforge.core.model.LatLong;
	using LatLongUtils = org.mapsforge.core.util.LatLongUtils;

	/// <summary>
	/// Configuration for the map file writer.
	/// </summary>
	public class MapWriterConfiguration
	{
		private BoundingBox bboxConfiguration;
		private int bboxEnlargement;
		private string comment;

		private string dataProcessorType;
		private long date;

		private bool debugStrings;
		private EncodingChoice encodingChoice;
		private int fileSpecificationVersion;

		private bool labelPosition;
		private LatLong mapStartPosition;
		private int mapStartZoomLevel;
		private File outputFile;
		private bool polygonClipping;
		private IList<string> preferredLanguages;

		private double simplification;

		private bool skipInvalidRelations;

		private OSMTagMapping tagMapping;
		private bool wayClipping;

		private string writerVersion;
		private ZoomIntervalConfiguration zoomIntervalConfiguration;

		/// <summary>
		/// Convenience method.
		/// </summary>
		/// <param name="bbox">
		///            the bounding box specification in format minLat, minLon, maxLat, maxLon in exactly this order as
		///            degrees </param>
		public virtual void addBboxConfiguration(string bbox)
		{
			if (!string.ReferenceEquals(bbox, null))
			{
				BboxConfiguration = BoundingBox.fromString(bbox);
			}
		}

		/// <summary>
		/// Convenience method.
		/// </summary>
		/// <param name="encoding">
		///            the choice for the encoding, either auto, single or double are valid parameters </param>
		public virtual void addEncodingChoice(string encoding)
		{
			if (!string.ReferenceEquals(encoding, null))
			{
				EncodingChoice = EncodingChoice.fromString(encoding);
			}
		}

		/// <summary>
		/// Convenience method.
		/// </summary>
		/// <param name="position">
		///            the map start position in format latitude, longitude </param>
		public virtual void addMapStartPosition(string position)
		{
			if (!string.ReferenceEquals(position, null))
			{
				MapStartPosition = LatLongUtils.fromString(position);
			}
		}

		/// <summary>
		/// Convenience method.
		/// </summary>
		/// <param name="zoom">
		///            the map start zoom level </param>
		public virtual void addMapStartZoom(string zoom)
		{
			if (!string.ReferenceEquals(zoom, null))
			{
				try
				{
					int intZoom = int.Parse(zoom);
					if (intZoom < 0 || intZoom > 21)
					{
						throw new System.ArgumentException("not a valid map start zoom: " + zoom);
					}
					MapStartZoomLevel = intZoom;
				}
				catch (System.FormatException e)
				{
					throw new System.ArgumentException("not a valid map start zoom: " + zoom, e);
				}
			}
			else
			{
				MapStartZoomLevel = -1;
			}
		}

		/// <summary>
		/// Convenience method.
		/// </summary>
		/// <param name="file">
		///            the path to the output file </param>
		public virtual void addOutputFile(string file)
		{
			if (!string.ReferenceEquals(file, null))
			{
				File f = new File(file);
				if (f.Directory)
				{
					throw new System.ArgumentException("output file parameter points to a directory, must be a file");
				}
				else if (f.exists() && !f.canWrite())
				{
					throw new System.ArgumentException("output file parameter points to a file we have no write permissions");
				}

				OutputFile = f;
			}
		}

		/// <summary>
		/// Convenience method.
		/// </summary>
		/// <param name="zoomIntervalConfiguaration">
		///            the zoom interval configuration </param>
		public virtual void addZoomIntervalConfiguration(string zoomIntervalConfiguaration)
		{
			if (!string.ReferenceEquals(zoomIntervalConfiguaration, null))
			{
				ZoomIntervalConfiguration = ZoomIntervalConfiguration.fromString(zoomIntervalConfiguaration);
			}
			else
			{
				ZoomIntervalConfiguration = ZoomIntervalConfiguration.StandardConfiguration;
			}
		}

		/// <returns> the bboxConfiguration </returns>
		public virtual BoundingBox BboxConfiguration
		{
			get
			{
				return this.bboxConfiguration;
			}
			set
			{
				this.bboxConfiguration = value;
			}
		}

		/// <returns> the bboxEnlargement </returns>
		public virtual int BboxEnlargement
		{
			get
			{
				return this.bboxEnlargement;
			}
			set
			{
				this.bboxEnlargement = value;
			}
		}

		/// <returns> the comment </returns>
		public virtual string Comment
		{
			get
			{
				return this.comment;
			}
			set
			{
				if (!string.ReferenceEquals(value, null) && value.Length > 0)
				{
					this.comment = value;
				}
			}
		}

		/// <returns> the dataProcessorType </returns>
		public virtual string DataProcessorType
		{
			get
			{
				return this.dataProcessorType;
			}
			set
			{
				this.dataProcessorType = value;
			}
		}

		/// <returns> the date </returns>
		public virtual long Date
		{
			get
			{
				return this.date;
			}
			set
			{
				this.date = value;
			}
		}

		/// <returns> the encodingChoice </returns>
		public virtual EncodingChoice EncodingChoice
		{
			get
			{
				return this.encodingChoice;
			}
			set
			{
				this.encodingChoice = value;
			}
		}

		/// <returns> the fileSpecificationVersion </returns>
		public virtual int FileSpecificationVersion
		{
			get
			{
				return this.fileSpecificationVersion;
			}
			set
			{
				this.fileSpecificationVersion = value;
			}
		}

		/// <returns> the mapStartPosition </returns>
		public virtual LatLong MapStartPosition
		{
			get
			{
				return this.mapStartPosition;
			}
			set
			{
				this.mapStartPosition = value;
			}
		}

		/// <returns> the mapStartZoomLevel </returns>
		public virtual int MapStartZoomLevel
		{
			get
			{
				return this.mapStartZoomLevel;
			}
			set
			{
				this.mapStartZoomLevel = value;
			}
		}

		/// <returns> the outputFile </returns>
		public virtual File OutputFile
		{
			get
			{
				return this.outputFile;
			}
			set
			{
				this.outputFile = value;
			}
		}

		/// <returns> the preferred language(s) </returns>
		public virtual IList<string> getPreferredLanguages()
		{
			return this.preferredLanguages;
		}

		/// <returns> the simplification </returns>
		public virtual double Simplification
		{
			get
			{
				return this.simplification;
			}
			set
			{
				if (value < 0)
				{
					throw new Exception("simplification must be >= 0");
				}
    
				this.simplification = value;
			}
		}

		/// <returns> the tagMapping </returns>
		public virtual OSMTagMapping TagMapping
		{
			get
			{
				return this.tagMapping;
			}
		}

		/// <returns> the writerVersion </returns>
		public virtual string WriterVersion
		{
			get
			{
				return this.writerVersion;
			}
			set
			{
				this.writerVersion = value;
			}
		}

		/// <returns> the zoomIntervalConfiguration </returns>
		public virtual ZoomIntervalConfiguration ZoomIntervalConfiguration
		{
			get
			{
				return this.zoomIntervalConfiguration;
			}
			set
			{
				this.zoomIntervalConfiguration = value;
			}
		}

		/// <summary>
		/// Convenience method.
		/// </summary>
		/// <returns> true if map start zoom level is set </returns>
		public virtual bool hasMapStartZoomLevel()
		{
			return MapStartZoomLevel >= 0;
		}

		/// <returns> the debugStrings </returns>
		public virtual bool DebugStrings
		{
			get
			{
				return this.debugStrings;
			}
			set
			{
				this.debugStrings = value;
			}
		}

		/// <returns> the labelPosition </returns>
		public virtual bool LabelPosition
		{
			get
			{
				return this.labelPosition;
			}
			set
			{
				this.labelPosition = value;
			}
		}

		/// <returns> the polygonClipping </returns>
		public virtual bool PolygonClipping
		{
			get
			{
				return this.polygonClipping;
			}
			set
			{
				this.polygonClipping = value;
			}
		}

		/// <returns> the skipInvalidRelations </returns>
		public virtual bool SkipInvalidRelations
		{
			get
			{
				return this.skipInvalidRelations;
			}
			set
			{
				this.skipInvalidRelations = value;
			}
		}

		/// <returns> the wayClipping </returns>
		public virtual bool WayClipping
		{
			get
			{
				return this.wayClipping;
			}
			set
			{
				this.wayClipping = value;
			}
		}

		/// <summary>
		/// Convenience method.
		/// </summary>
		/// <param name="file">
		///            the path to the output file </param>
		public virtual void loadTagMappingFile(string file)
		{
			if (!string.ReferenceEquals(file, null))
			{
				File f = new File(file);
				if (!f.exists())
				{
					throw new System.ArgumentException("tag mapping file parameter points to a file that does not exist");
				}
				if (f.Directory)
				{
					throw new System.ArgumentException("tag mapping file parameter points to a directory, must be a file");
				}
				else if (!f.canRead())
				{
					throw new System.ArgumentException("tag mapping file parameter points to a file we have no read permissions");
				}

				try
				{
					this.tagMapping = OSMTagMapping.getInstance(f.toURI().toURL());
				}
				catch (MalformedURLException e)
				{
					throw new Exception(e);
				}
			}
			else
			{
				this.tagMapping = OSMTagMapping.Instance;
			}
		}














		/// <param name="preferredLanguages">
		///            the preferred language(s) to set separated with ',' </param>
		public virtual void setPreferredLanguages(string preferredLanguages)
		{
			if (!string.ReferenceEquals(preferredLanguages, null) && preferredLanguages.Trim().Length > 0)
			{
				this.preferredLanguages = Arrays.asList(preferredLanguages.Split(",", true));
			}
		}






		/// <summary>
		/// Validates this configuration.
		/// </summary>
		/// <exception cref="IllegalArgumentException">
		///             thrown if configuration is invalid </exception>
		public virtual void validate()
		{
			if (this.mapStartPosition != null && this.bboxConfiguration != null && !this.bboxConfiguration.contains(this.mapStartPosition))
			{
				throw new System.ArgumentException("map start position is not valid, must be included in bounding box of the map, bbox: " + this.bboxConfiguration.ToString() + " - map start position: " + this.mapStartPosition.ToString());
			}
		}
	}

}