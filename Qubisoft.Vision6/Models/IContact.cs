namespace Qubisoft.Vision6.Models
{
    public interface IContact
    {
        int? id { get; set; }
        string? email { get; set; }
        string? mobile { get; set; }
        string? first_name { get; set; }
        string? last_name { get; set; }
        string? password { get; set; }

        ISubscribedStatus? subscribed { get; set; }
        bool? is_active { get; set; }
        bool? double_opt_in { get; set; }

    }
}
