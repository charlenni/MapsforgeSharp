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

	using MapWriterConfiguration = org.mapsforge.map.writer.model.MapWriterConfiguration;
	using Constants = org.mapsforge.map.writer.util.Constants;
	using TaskConfiguration = org.openstreetmap.osmosis.core.pipeline.common.TaskConfiguration;
	using TaskManager = org.openstreetmap.osmosis.core.pipeline.common.TaskManager;
	using TaskManagerFactory = org.openstreetmap.osmosis.core.pipeline.common.TaskManagerFactory;
	using SinkManager = org.openstreetmap.osmosis.core.pipeline.v0_6.SinkManager;

	/// <summary>
	/// Factory for the mapfile writer osmosis plugin.
	/// </summary>
	internal class MapFileWriterFactory : TaskManagerFactory
	{
		private const string PARAM_BBOX = "bbox";
		private const string PARAM_BBOX_ENLARGEMENT = "bbox-enlargement";
		private const string PARAM_COMMENT = "comment";
		private const string PARAM_DEBUG_INFO = "debug-file";
		private const string PARAM_ENCODING = "encoding";
		private const string PARAM_LABEL_POSITION = "label-position";
		private const string PARAM_MAP_START_POSITION = "map-start-position";
		private const string PARAM_MAP_START_ZOOM = "map-start-zoom";
		private const string PARAM_OUTFILE = "file";
		private const string PARAM_POLYGON_CLIPPING = "polygon-clipping";
		private const string PARAM_PREFERRED_LANGUAGES = "preferred-languages";
		// private static final String PARAM_WAYNODE_COMPRESSION = "waynode-compression";
		private const string PARAM_SIMPLIFICATION_FACTOR = "simplification-factor";
		private const string PARAM_SKIP_INVALID_RELATIONS = "skip-invalid-relations";
		private const string PARAM_TAG_MAPPING_FILE = "tag-conf-file";
		private const string PARAM_TYPE = "type";
		private const string PARAM_WAY_CLIPPING = "way-clipping";
		private const string PARAM_ZOOMINTERVAL_CONFIG = "zoom-interval-conf";

		protected internal override TaskManager createTaskManagerImpl(TaskConfiguration taskConfig)
		{
			MapWriterConfiguration configuration = new MapWriterConfiguration();
			configuration.addOutputFile(getStringArgument(taskConfig, PARAM_OUTFILE, Constants.DEFAULT_PARAM_OUTFILE));
			configuration.loadTagMappingFile(getStringArgument(taskConfig, PARAM_TAG_MAPPING_FILE, null));

			configuration.addMapStartPosition(getStringArgument(taskConfig, PARAM_MAP_START_POSITION, null));
			configuration.addMapStartZoom(getStringArgument(taskConfig, PARAM_MAP_START_ZOOM, null));
			configuration.addBboxConfiguration(getStringArgument(taskConfig, PARAM_BBOX, null));
			configuration.addZoomIntervalConfiguration(getStringArgument(taskConfig, PARAM_ZOOMINTERVAL_CONFIG, null));

			configuration.Comment = getStringArgument(taskConfig, PARAM_COMMENT, null);
			configuration.DebugStrings = getBooleanArgument(taskConfig, PARAM_DEBUG_INFO, false);
			configuration.PolygonClipping = getBooleanArgument(taskConfig, PARAM_POLYGON_CLIPPING, true);
			configuration.WayClipping = getBooleanArgument(taskConfig, PARAM_WAY_CLIPPING, true);
			configuration.LabelPosition = getBooleanArgument(taskConfig, PARAM_LABEL_POSITION, false);
			// boolean waynodeCompression = getBooleanArgument(taskConfig, PARAM_WAYNODE_COMPRESSION,
			// true);
			configuration.Simplification = getDoubleArgument(taskConfig, PARAM_SIMPLIFICATION_FACTOR, Constants.DEFAULT_SIMPLIFICATION_FACTOR);
			configuration.SkipInvalidRelations = getBooleanArgument(taskConfig, PARAM_SKIP_INVALID_RELATIONS, false);

			configuration.DataProcessorType = getStringArgument(taskConfig, PARAM_TYPE, Constants.DEFAULT_PARAM_TYPE);
			configuration.BboxEnlargement = getIntegerArgument(taskConfig, PARAM_BBOX_ENLARGEMENT, Constants.DEFAULT_PARAM_BBOX_ENLARGEMENT);

			configuration.PreferredLanguages = getStringArgument(taskConfig, PARAM_PREFERRED_LANGUAGES, null);
			configuration.addEncodingChoice(getStringArgument(taskConfig, PARAM_ENCODING, Constants.DEFAULT_PARAM_ENCODING));

			configuration.validate();

			MapFileWriterTask task = new MapFileWriterTask(configuration);
			return new SinkManager(taskConfig.Id, task, taskConfig.PipeArgs);
		}
	}

}