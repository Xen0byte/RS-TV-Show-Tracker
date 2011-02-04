﻿namespace RoliSoft.TVShowTracker.Parsers.Subtitles.Engines
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    using NUnit.Framework;

    /// <summary>
    /// Provides support for scraping Hosszupuska Sub.
    /// </summary>
    [TestFixture]
    public class Hosszupuska : SubtitleSearchEngine
    {
        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return "Hosszupuska";
            }
        }

        /// <summary>
        /// Gets the URL of the site.
        /// </summary>
        /// <value>The site location.</value>
        public override string Site
        {
            get
            {
                return "http://hosszupuskasub.com/";
            }
        }

        /// <summary>
        /// Gets the URL to the favicon of the site.
        /// </summary>
        /// <value>The icon location.</value>
        public override string Icon
        {
            get
            {
                return "http://hosszupuskasub.com/favicon.ico";
            }
        }

        /// <summary>
        /// Searches for subtitles on the service.
        /// </summary>
        /// <param name="query">The name of the release to search for.</param>
        /// <returns>List of found subtitles.</returns>
        public override IEnumerable<Subtitle> Search(string query)
        {
            var html = Utils.GetHTML(Site + "sorozatok.php", "cim=" + Uri.EscapeUriString(ShowNames.Tools.Normalize(query)), encoding: Encoding.GetEncoding("iso-8859-2"));
            var subs = html.DocumentNode.SelectNodes("//td/a[starts-with(@href,'download.php?file=')]");

            if (subs == null)
            {
                yield break;
            }

            foreach (var node in subs)
            {
                var sub = new Subtitle(this);

                sub.Release  = Regex.Replace(node.SelectSingleNode("../../td[2]").InnerHtml, @".*?<br>", string.Empty);
                sub.Language = ParseLanguage(node.SelectSingleNode("../../td[3]/img").GetAttributeValue("src", string.Empty));
                sub.URL      = Site + node.SelectSingleNode("../../td[7]/a").GetAttributeValue("href", string.Empty);

                yield return sub;
            }
        }

        /// <summary>
        /// Parses the language of the subtitle.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>Strongly-typed language of the subtitle.</returns>
        public static Languages ParseLanguage(string language)
        {
            switch(language)
            {
                case "flags/1.gif":
                    return Languages.Hungarian;

                case "flags/2.gif":
                    return Languages.English;

                default:
                    return Languages.Unknown;
            }
        }
    }
}
