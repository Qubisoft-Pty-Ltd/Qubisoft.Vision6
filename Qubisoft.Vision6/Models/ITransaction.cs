namespace Qubisoft.Vision6.Models
{
    public interface ITransaction
    {   
        int list_id { get; set; }
        int? group_id { get; set; }
        string? group_name { get; set; }
        int? message_id { get; set; }
        string type { get; set; }
        string? subject { get; set; }
        string? from_name { get; set; }
        string? from_address { get; set; }
        string? reply_to { get; set; }
        string? bcc_address { get; set; }
        string? body_html { get; set; }
        string? body_text { get; set; }
        bool? include_unsubscribed { get; set; }
        List<Recipient>? recipients { get; set; }
    }
}