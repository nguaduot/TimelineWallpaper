﻿using Newtonsoft.Json;
using System;

namespace TimelineWallpaper.Beans {
    public class ReleaseApi {
        public string Version { get; set; }

        public string Url { get; set; }
    }

    /// <summary>
    /// Github的发布响应结果.
    /// </summary>
    public class GithubApi {
        /// <summary>
        /// 网址.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "html_url", Required = Required.Default)]
        public string Url { get; set; }

        /// <summary>
        /// 版本标签.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "tag_name", Required = Required.Default)]
        public string TagName { get; set; }

        /// <summary>
        /// 标题.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "name", Required = Required.Default)]
        public string Name { get; set; }

        /// <summary>
        /// 是否为预发布版本.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "prerelease", Required = Required.Default)]
        public bool IsPreRelease { get; set; }

        /// <summary>
        /// 发布时间.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "published_at", Required = Required.Default)]
        public DateTime PublishTime { get; set; }

        /// <summary>
        /// 发布说明.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "body", Required = Required.Default)]
        public string Description { get; set; }
    }
}