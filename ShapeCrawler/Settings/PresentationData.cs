﻿using System;
using System.Collections.Generic;
using System.Linq;
using ShapeCrawler.Factories;
using ShapeCrawler.Placeholders;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace ShapeCrawler.Settings
{
    internal class PresentationData
    {
        private readonly Lazy<Dictionary<int, FontData>> _lvlToFontData;

        #region Constructors

        public PresentationData(P.Presentation pPresentation)
        {
            _lvlToFontData = new Lazy<Dictionary<int, FontData>>(() => ParseFontHeights(pPresentation));
        }

        #endregion Constructors

        #region Properties

        public Dictionary<int, FontData> LlvToFontData => _lvlToFontData.Value;

        #endregion Properties

        #region Private Methods

        private static Dictionary<int, FontData> ParseFontHeights(P.Presentation pPresentation)
        {
            var lvlToFontData = new Dictionary<int, FontData>();

            // from presentation default text settings
            if (pPresentation.DefaultTextStyle != null)
            {
                lvlToFontData = FontDataParser.FromCompositeElement(pPresentation.DefaultTextStyle);
            }

            // from theme default text settings
            if (lvlToFontData.Any(kvp => kvp.Value.FontSize == null))
            {
                A.TextDefault themeTextDefault =
                    pPresentation.PresentationPart.ThemePart.Theme.ObjectDefaults.TextDefault;
                if (themeTextDefault != null)
                {
                    lvlToFontData = FontDataParser.FromCompositeElement(themeTextDefault.ListStyle);
                }
            }

            return lvlToFontData;
        }

        #endregion Private Methods
    }
}