using CommunityToolkit.Mvvm.ComponentModel;

namespace VaultIQ.WPF.ViewModels;

public partial class PinViewModel : ObservableObject
{
    [ObservableProperty] private string _pin = string.Empty;
    [ObservableProperty] private int _pinLength;

    partial void OnPinChanged(string value)
    {
        // Only keep digits
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits != value)
        {
            Pin = digits;
            return;
        }
        PinLength = digits.Length;
    }
}