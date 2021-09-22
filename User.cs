using System;
using Newtonsoft.Json;

namespace Blog5Users
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
    }
}