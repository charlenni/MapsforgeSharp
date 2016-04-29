using System;
using System.Collections.Generic;

/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2015 devemux86
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
namespace org.mapsforge.map.writer.osmosis
{


	using BoundingBox = org.mapsforge.core.model.BoundingBox;
	using MapWriterConfiguration = org.mapsforge.map.writer.model.MapWriterConfiguration;
	using TileBasedDataProcessor = org.mapsforge.map.writer.model.TileBasedDataProcessor;
	using Constants = org.mapsforge.map.writer.util.Constants;
	using EntityContainer = org.openstreetmap.osmosis.core.container.v0_6.EntityContainer;
	using Bound = org.openstreetmap.osmosis.core.domain.v0_6.Bound;
	using Entity = org.openstreetmap.osmosis.core.domain.v0_6.Entity;
	using Node = org.openstreetmap.osmosis.core.domain.v0_6.Node;
	using Relation = org.openstreetmap.osmosis.core.domain.v0_6.Relation;
	using Way = org.openstreetmap.osmosis.core.domain.v0_6.Way;
	using Sink = org.openstreetmap.osmosis.core.task.v0_6.Sink;

	/// <summary>
	/// An Osmosis plugin that reads OpenStreetMap data and converts it to a mapsforge binary file.
	/// </summary>
	public class MapFileWriterTask : Sink
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly Logger LOGGER = Logger.getLogger(typeof(MapFileWriterTask).FullName);

		// Accounting
		private int amountOfNodesProcessed = 0;
		private int amountOfRelationsProcessed = 0;
		private int amountOfWaysProcessed = 0;

		private readonly MapWriterConfiguration configuration;
		private TileBasedDataProcessor tileBasedGeoObjectStore;

		internal MapFileWriterTask(MapWriterConfiguration configuration)
		{
			this.configuration = configuration;

			Properties properties = new Properties();
			try
			{
				properties.load(typeof(MapFileWriterTask).ClassLoader.getResourceAsStream("default.properties"));
				configuration.WriterVersion = Constants.CREATOR_NAME + "-" + properties.getProperty(Constants.PROPERTY_NAME_WRITER_VERSION);
				// If multilingual map then set newer map file version
				bool multilingual = configuration.PreferredLanguages != null && configuration.PreferredLanguages.size() > 1;
				configuration.FileSpecificationVersion = int.Parse(properties.getProperty(multilingual ? Constants.PROPERTY_NAME_FILE_SPECIFICATION_VERSION_MAX : Constants.PROPERTY_NAME_FILE_SPECIFICATION_VERSION_MIN));

				LOGGER.info("mapfile-writer version: " + configuration.WriterVersion);
				LOGGER.info("mapfile format specification version: " + configuration.FileSpecificationVersion);
			}
			catch (IOException e)
			{
				throw new Exception("could not find default properties", e);
			}
			catch (System.FormatException e)
			{
				throw new Exception("map file specification version is not an integer", e);
			}

			// CREATE DATASTORE IF BBOX IS DEFINED
			if (this.configuration.BboxConfiguration != null)
			{
				if ("ram".Equals(configuration.DataProcessorType, StringComparison.CurrentCultureIgnoreCase))
				{
					this.tileBasedGeoObjectStore = RAMTileBasedDataProcessor.newInstance(configuration);
				}
				else
				{
					this.tileBasedGeoObjectStore = HDTileBasedDataProcessor.newInstance(configuration);
				}
			}
		}

		public override void complete()
		{
			NumberFormat nfMegabyte = NumberFormat.Instance;
			NumberFormat nfCounts = NumberFormat.Instance;
			nfCounts.GroupingUsed = true;
			nfMegabyte.MaximumFractionDigits = 2;

			LOGGER.info("completing read...");
			this.tileBasedGeoObjectStore.complete();

			LOGGER.info("start writing file...");

			try
			{
				if (this.configuration.OutputFile.exists())
				{
					LOGGER.info("overwriting file " + this.configuration.OutputFile.AbsolutePath);
					this.configuration.OutputFile.delete();
				}
				MapFileWriter.writeFile(this.configuration, this.tileBasedGeoObjectStore);
			}
			catch (IOException e)
			{
				LOGGER.log(Level.SEVERE, "error while writing file", e);
			}

			LOGGER.info("finished...");
			LOGGER.fine("total processed nodes: " + nfCounts.format(this.amountOfNodesProcessed));
			LOGGER.fine("total processed ways: " + nfCounts.format(this.amountOfWaysProcessed));
			LOGGER.fine("total processed relations: " + nfCounts.format(this.amountOfRelationsProcessed));

			LOGGER.info("estimated memory consumption: " + nfMegabyte.format(+((Runtime.Runtime.totalMemory() - Runtime.Runtime.freeMemory()) / Math.Pow(1024, 2))) + "MB");
		}

		/*
		 * (non-Javadoc)
		 * @see org.openstreetmap.osmosis.core.task.v0_6.Initializable#initialize(java.util.Map)
		 */
		public override void initialize(IDictionary<string, object> metadata)
		{
			// nothing to do here
		}

		public override void process(EntityContainer entityContainer)
		{
			Entity entity = entityContainer.Entity;

			switch (entity.Type)
			{
				case Bound:
					Bound bound = (Bound) entity;
					if (this.configuration.BboxConfiguration == null)
					{
						BoundingBox bbox = new BoundingBox(bound.Bottom, bound.Left, bound.Top, bound.Right);
						this.configuration.BboxConfiguration = bbox;
						this.configuration.validate();
						if ("ram".Equals(this.configuration.DataProcessorType))
						{
							this.tileBasedGeoObjectStore = RAMTileBasedDataProcessor.newInstance(this.configuration);
						}
						else
						{
							this.tileBasedGeoObjectStore = HDTileBasedDataProcessor.newInstance(this.configuration);
						}
					}
					LOGGER.info("start reading data...");
					break;

				// *******************************************************
				// ****************** NODE PROCESSING*********************
				// *******************************************************
				case Node:

					if (this.tileBasedGeoObjectStore == null)
					{
						LOGGER.severe("No valid bounding box found in input data.\n" + "Please provide valid bounding box via command " + "line parameter 'bbox=minLat,minLon,maxLat,maxLon'.\n" + "Tile based data store not initialized. Aborting...");
						throw new System.InvalidOperationException("tile based data store not initialized, missing bounding " + "box information in input data");
					}
					this.tileBasedGeoObjectStore.addNode((Node) entity);
					// hint to GC
					entity = null;
					this.amountOfNodesProcessed++;
					break;

				// *******************************************************
				// ******************* WAY PROCESSING*********************
				// *******************************************************
				case Way:
					this.tileBasedGeoObjectStore.addWay((Way) entity);
					entity = null;
					this.amountOfWaysProcessed++;
					break;

				// *******************************************************
				// ****************** RELATION PROCESSING*********************
				// *******************************************************
				case Relation:
					Relation currentRelation = (Relation) entity;
					this.tileBasedGeoObjectStore.addRelation(currentRelation);
					this.amountOfRelationsProcessed++;
					entity = null;
					break;
			}
		}

		public override void release()
		{
			if (this.tileBasedGeoObjectStore != null)
			{
				this.tileBasedGeoObjectStore.release();
			}
		}
	}

}