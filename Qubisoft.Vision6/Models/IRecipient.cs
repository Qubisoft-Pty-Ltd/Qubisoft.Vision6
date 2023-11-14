namespace Qubisoft.Vision6.Models
{
    public interface IRecipient
    {
        string? id { get; set; }
        string? name { get; set; }
        string? email { get; set; }
        string? mobile { get; set; }
        List<Attachment>? attachments { get; set; }
    }
}
