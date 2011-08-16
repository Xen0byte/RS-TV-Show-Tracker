﻿namespace RoliSoft.TVShowTracker.Parsers.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    using HtmlAgilityPack;

    using NUnit.Framework;

    using RoliSoft.TVShowTracker.Downloaders;
    using RoliSoft.TVShowTracker.Downloaders.Engines;

    /// <summary>
    /// Represents a download link search engine.
    /// </summary>
    public abstract class DownloadSearchEngine : ParserEngine
    {
        /// <summary>
        /// Gets a value indicating whether the site requires authentication.
        /// </summary>
        /// <value><c>true</c> if requires authentication; otherwise, <c>false</c>.</value>
        public virtual bool Private
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the cookies used to access the site.
        /// </summary>
        /// <value>The cookies in the same format in which <c>alert(document.cookie)</c> returns in a browser.</value>
        public virtual string Cookies { get; set; }

        /// <summary>
        /// Gets the names of the required cookies for the authentication.
        /// </summary>
        /// <value>The required cookies for authentication.</value>
        public virtual string[] RequiredCookies { get; internal set; }

        /// <summary>
        /// Gets the URL to the login page.
        /// </summary>
        /// <value>The URL to the login page.</value>
        public virtual string LoginURL
        {
            get
            {
                return Site + "login.php";
            }
        }

        /// <summary>
        /// Gets the input fields of the login form.
        /// </summary>
        /// <value>The input fields of the login form.</value>
        public virtual Dictionary<string, object> LoginFields { get; internal set; }

        /// <summary>
        /// Gets the type of the link.
        /// </summary>
        /// <value>The type of the link.</value>
        public abstract Types Type { get; }

        /// <summary>
        /// Returns an <c>IDownloader</c> object which can be used to download the URLs provided by this parser.
        /// </summary>
        /// <value>The downloader.</value>
        public virtual IDownloader Downloader
        {
            get
            {
                return new HTTPDownloader();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this site is deprecated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if deprecated; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Deprecated
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Occurs when a download link search is done.
        /// </summary>
        public event EventHandler<EventArgs<List<Link>>> DownloadSearchDone;

        /// <summary>
        /// Occurs when a download link search has encountered an error.
        /// </summary>
        public event EventHandler<EventArgs<string, Exception>> DownloadSearchError;

        /// <summary>
        /// Searches for download links on the service.
        /// </summary>
        /// <param name="query">The name of the release to search for.</param>
        /// <returns>List of found download links.</returns>
        public abstract IEnumerable<Link> Search(string query);

        private Thread _job;

        /// <summary>
        /// Searches for download links on the service asynchronously.
        /// </summary>
        /// <param name="query">The name of the release to search for.</param>
        public void SearchAsync(string query)
        {
            CancelAsync();

            _job = new Thread(() =>
                {
                    try
                    {
                        var list = Search(query);
                        DownloadSearchDone.Fire(this, list.ToList());
                    }
                    catch (Exception ex)
                    {
                        DownloadSearchError.Fire(this, "There was an error while searching for download links.", ex);
                    }
                });
            _job.Start();
        }

        /// <summary>
        /// Cancels the active asynchronous search.
        /// </summary>
        public void CancelAsync()
        {
            if (_job != null)
            {
                _job.Abort();
                _job = null;
            }
        }

        /// <summary>
        /// Tests the parser by searching for "House" on the tracker.
        /// </summary>
        [Test]
        public virtual void TestSearch()
        {
            if (Private)
            {
                Cookies = Settings.Get(Name + " Cookies");

                if (string.IsNullOrWhiteSpace(Cookies))
                {
                    Assert.Inconclusive("Cookies are required to test a private site.");
                }
            }

            var list = Search("House").ToList();

            Assert.Greater(list.Count, 0, "Failed to grab any download links for House on {0}.".FormatWith(Name));

            Console.WriteLine("┌────────────────────────────────────────────────────┬────────────┬────────────┬──────────────────────────────────────────┬──────────────────────────────────────────────────────────────┬──────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ Release name                                       │ Size       │ Quality    │ Additional informations                  │ Details page URL                                             │ Downloadable file URL                                        │");
            Console.WriteLine("├────────────────────────────────────────────────────┼────────────┼────────────┼──────────────────────────────────────────┼──────────────────────────────────────────────────────────────┼──────────────────────────────────────────────────────────────┤");
            list.ForEach(item => Console.WriteLine("│ {0,-50} │ {1,-10} │ {2,-10} │ {3,-40} │ {4,-60} │ {5,-60} │".FormatWith(item.Release.Transliterate().CutIfLonger(50), (item.Size ?? string.Empty).CutIfLonger(10), item.Quality, (item.Infos ?? string.Empty).CutIfLonger(40), (item.InfoURL ?? string.Empty).CutIfLonger(60), (item.FileURL ?? string.Empty).CutIfLonger(60))));
            Console.WriteLine("└────────────────────────────────────────────────────┴────────────┴────────────┴──────────────────────────────────────────┴──────────────────────────────────────────────────────────────┴──────────────────────────────────────────────────────────────┘");
        }

        /// <summary>
        /// Tests the login.
        /// </summary>
        [Test]
        public virtual void TestLogin()
        {
            if (!Private || LoginFields == null)
            {
                Assert.Inconclusive("This parser does not support authentication.");
            }

            var user = "RoliSoft";
            var pass = string.Empty;

            Console.WriteLine("Logging in as '" + user + "':");

            var cookies = TrackerDoLogin(user, pass);

            Assert.IsNotNullOrEmpty(cookies, "Didn't receive any cookies from the login page.");

            Console.WriteLine("┌────────────────────────────────┬────────────────────────────────────────────────────┐");
            Console.WriteLine("│ Cookie name                    │ Cookie value                                       │");
            Console.WriteLine("├────────────────────────────────┼────────────────────────────────────────────────────┤");

            foreach (var cookie in Regex.Split(cookies, @";\s"))
            {
                var kv = cookie.Split(new[] { '=' }, 2);
                if (kv.Length < 2) continue;

                Console.WriteLine("│ {0,-30} │ {1,-50} │".FormatWith(kv[0].CutIfLonger(30), kv[1].CutIfLonger(50)));
            }
            
            Console.WriteLine("└────────────────────────────────┴────────────────────────────────────────────────────┘");
        }

        /// <summary>
        /// Checks if a Gazelle or TBSource-based tracker requires authentication.
        /// </summary>
        /// <param name="node">The HTML document node.</param>
        /// <returns>
        ///   <c>true</c> if login is required; otherwise, <c>false</c>.
        /// </returns>
        internal virtual bool TrackerLoginRequired(HtmlNode node)
        {
            return node.SelectSingleNode("//form[@method = 'post' and contains(@action, 'login.php')]//input[@type = 'hidden' and (@name = 'returnto' or @name = 'return' or @name = 'back' or @name = 'honnan')]") == null;
        }
        
        /// <summary>
        /// Initiates a login on a Gazelle or TBSource-based tracker.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// Cookies.
        /// </returns>
        internal virtual string TrackerDoLogin(string username, string password)
        {
            var post = new StringBuilder();

            foreach (var field in LoginFields)
            {
                if (post.Length != 0)
                {
                    post.Append("&");
                }

                post.Append(field.Key + "=");
                var value = string.Empty;

                if (field.Value is string)
                {
                    value = (string)field.Value;
                }
                else if (field.Value is LoginFieldTypes)
                {
                    switch ((LoginFieldTypes)field.Value)
                    {
                        case LoginFieldTypes.UserName:
                            value = username;
                            break;

                        case LoginFieldTypes.Password:
                            value = password;
                            break;

                        case LoginFieldTypes.ReturnTo:
                            value = "/";
                            break;
                    }
                }

                post.Append(Uri.EscapeDataString(value));
            }

            var cookies = new StringBuilder();
            
            Utils.GetURL(LoginURL, post.ToString(),
                request: req =>
                    {
                        req.Referer = Site;
                        req.AllowAutoRedirect = false;
                    },
                response: resp =>
                    {
                        if (resp.Cookies == null || resp.Cookies.Count == 0)
                        {
                            return;
                        }

                        foreach (Cookie cookie in resp.Cookies)
                        {
                            if (cookie.Name == "PHPSESSID" || cookie.Name == "JSESSIONID" || cookie.Value == "deleted")
                            {
                                continue;
                            }

                            cookies.Append(cookie.Name + "=" + cookie.Value + "; ");
                        }
                    }
            );

            return cookies.ToString();
        }

        /// <summary>
        /// Gets the cookies for this parser.
        /// </summary>
        /// <returns>
        /// The specified cookies for this parser.
        /// </returns>
        internal virtual string GetCookies()
        {
            return Settings.Get(Name + " Cookies");
        }

        /// <summary>
        /// Sets the cookies for this parser.
        /// </summary>
        /// <param name="value">The cookies.</param>
        internal virtual void SetCookies(string value)
        {
            Settings.Set(Name + " Cookies", value);
        }
    }
}
