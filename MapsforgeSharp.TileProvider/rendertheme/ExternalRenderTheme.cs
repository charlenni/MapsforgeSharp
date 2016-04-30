/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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
    using PCLStorage;
    using PCLStorage.Exceptions;
    using System;
    using System.IO;    /// <summary>
                        /// An ExternalRenderTheme allows for customizing the rendering style of the map via an XML file.
                        /// </summary>
    public class ExternalRenderTheme : IXmlRenderTheme
	{
		private long lastModifiedTime;
		private XmlRenderThemeMenuCallback menuCallback;
        private IFile renderThemeFile;
        private Stream renderThemeFileStream;

        /// <summary>
        /// Create external render theme async
        /// </summary>
        /// <param name="renderThemeFile"></param>
        /// <param name="menuCallback"></param>
        /// <returns></returns>
        public static ExternalRenderTheme CreateExternalRenderTheme(string renderThemeFilePath, XmlRenderThemeMenuCallback menuCallback = null)
        {
            var externalRenderTheme = new ExternalRenderTheme();

            externalRenderTheme.Initialize(renderThemeFilePath, menuCallback);

            return externalRenderTheme;
        }

        /// <param name="renderThemeFile">
        ///            the XML render theme file. </param>
        public ExternalRenderTheme()
        {
        }

        public async void Initialize(string renderThemeFilePath, XmlRenderThemeMenuCallback menuCallback)
		{
            var fileExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(renderThemeFilePath);

            if (fileExists == ExistenceCheckResult.NotFound)
			{
				throw new PCLStorage.Exceptions.FileNotFoundException("file does not exist: " + renderThemeFilePath);
			}
			else if (fileExists == ExistenceCheckResult.FolderExists)
			{
				throw new PCLStorage.Exceptions.FileNotFoundException("not a file: " + renderThemeFilePath);
			}

            // Open file
            this.renderThemeFile = await FileSystem.Current.LocalStorage.GetFileAsync(renderThemeFilePath);
            this.renderThemeFileStream = await this.renderThemeFile.OpenAsync(FileAccess.Read);
            this.menuCallback = menuCallback;

            if (this.renderThemeFileStream == null)
			{
				throw new PCLStorage.Exceptions.FileNotFoundException("cannot read file: " + renderThemeFile.Path);
			}

            // TODO: Add lastModified to PCLStorage
            this.lastModifiedTime = DateTimeHelperClass.CurrentUnixTimeMillis(); // renderThemeFile.lastModified();
			if (this.lastModifiedTime == 0L)
			{
				throw new PCLStorage.Exceptions.FileNotFoundException("cannot read last modified time: " + renderThemeFile.Path);
			}
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is ExternalRenderTheme))
			{
				return false;
			}
			ExternalRenderTheme other = (ExternalRenderTheme) obj;
			if (this.lastModifiedTime != other.lastModifiedTime)
			{
				return false;
			}
			if (this.renderThemeFile == null)
			{
				if (other.renderThemeFile != null)
				{
					return false;
				}
			}
			else if (!this.renderThemeFile.Equals(other.renderThemeFile))
			{
				return false;
			}
			return true;
		}

		public XmlRenderThemeMenuCallback MenuCallback
		{
			get
			{
				return this.menuCallback;
			}
		}

        public string RelativePathPrefix
        {
            get
            {
                return this.renderThemeFile.Path;
            }
        }

        public System.IO.Stream RenderThemeAsStream
		{
			get
			{
				return this.renderThemeFileStream;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + (int)(this.lastModifiedTime ^ ((long)((ulong)this.lastModifiedTime >> 32)));
			result = prime * result + ((this.renderThemeFile == null) ? 0 : this.renderThemeFile.GetHashCode());
			return result;
		}
	}
}