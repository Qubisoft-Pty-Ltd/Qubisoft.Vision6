using System.Text.Json.Serialization;

namespace Qubisoft.Vision6.Models
{
    public class Contact : IContact
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? id { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public string? email { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? mobile { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? first_name { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? last_name { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? password { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public ISubscribedStatus? subscribed { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public bool? is_active { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public bool? double_opt_in { get; set; }
    }
}