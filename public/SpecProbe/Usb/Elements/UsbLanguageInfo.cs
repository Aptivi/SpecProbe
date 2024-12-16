//
// SpecProbe  Copyright (C) 2023-2024  Aptivi
//
// This file is part of SpecProbe
//
// SpecProbe is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SpecProbe is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

namespace SpecProbe.Usb.Elements
{
    /// <summary>
    /// USB language information
    /// </summary>
    public class UsbLanguageInfo
    {
        internal string languageName;
        internal int languageId;
        internal UsbLanguageDialectInfo[] dialects = [];

        /// <summary>
        /// Language name
        /// </summary>
        public string Name =>
            languageName;

        /// <summary>
        /// Language ID
        /// </summary>
        public int Id =>
            languageId;

        /// <summary>
        /// List of language dialects that this language provides
        /// </summary>
        public UsbLanguageDialectInfo[] Dialects =>
            dialects;

        internal UsbLanguageInfo(string languageName, int languageId)
        {
            this.languageName = languageName;
            this.languageId = languageId;
        }
    }
}
