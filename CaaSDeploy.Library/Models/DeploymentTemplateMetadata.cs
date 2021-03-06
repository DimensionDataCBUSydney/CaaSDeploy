﻿using Newtonsoft.Json;

namespace DD.CBU.CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents the metadata of a deployment template.
    /// </summary>
    public class DeploymentTemplateMetadata
    {
        /// <summary>
        /// Gets or sets the schema version.
        /// </summary>
        [JsonProperty("schemaVersion")]
        public string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the template.
        /// </summary>
        [JsonProperty("templateName")]
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets the description of the template.
        /// </summary>
        [JsonProperty("templateDescription")]
        public string TemplateDescription { get; set; }

        /// <summary>
        /// Gets or sets the product description.
        /// </summary>
        [JsonProperty("productDescription")]
        public string ProductDescription { get; set; }

        /// <summary>
        /// Gets or sets the product category.
        /// </summary>
        [JsonProperty("productCategory")]
        public string ProductCategory { get; set; }

        /// <summary>
        /// Gets or sets the product URL.
        /// </summary>
        [JsonProperty("productUrl")]
        public string ProductUrl { get; set; }
    }
}
