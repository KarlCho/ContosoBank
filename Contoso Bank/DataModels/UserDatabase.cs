using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contoso_Bank.DataModels
{
    public class UserDatabase
    {
        [JsonProperty(PropertyName = "ID")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Surname")]
        public string Surname { get; set; }

        [JsonProperty(PropertyName = "NZD")]
        public double NZD { get; set; }

        [JsonProperty(PropertyName = "USD")]
        public double USD { get; set; }

        [JsonProperty(PropertyName = "AUD")]
        public double AUD { get; set; }

        [JsonProperty(PropertyName = "updatedAt")]
        public string updatedAt { get; set; }
    }
}