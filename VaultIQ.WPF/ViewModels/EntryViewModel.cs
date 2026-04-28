using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace VaultIQ.WPF.ViewModels;

public partial class EntryViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StrengthLabel))]
    [NotifyPropertyChangedFor(nameof(StrengthScore))]
    private string _password = "";

    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _url = "";
    [ObservableProperty] private string _notes = "";

    public string StrengthLabel => PasswordStrengthService.GetLabel(_password);
    public int StrengthScore => PasswordStrengthService.Evaluate(_password);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken ct)
    {
        var command = new SaveEntryCommand(Title, Username, Password, Url, Notes);
        await _mediator.Send(command, ct);
    }

    private bool CanSave() =>
        !string.IsNullOrWhiteSpace(Title) &&
        !string.IsNullOrWhiteSpace(Password);
}