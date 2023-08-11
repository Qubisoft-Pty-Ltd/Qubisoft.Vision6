namespace Qubisoft.Vision6.Models
{
    public interface IContact
    {
        string? email { get; set; }
        string? mobile { get; set; }
        ISubscribedStatus? subscribed { get; set; }
        bool? is_active { get; set; }
        bool? double_opt_in { get; set; }

    }
}
