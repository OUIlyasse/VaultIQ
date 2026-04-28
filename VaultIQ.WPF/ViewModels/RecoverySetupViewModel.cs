using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VaultIQ.WPF.ViewModels;

public partial class RecoverySetupViewModel : ObservableObject
{
    [ObservableProperty] private int _currentStep = 2;   // wizard opens at step 2 per design
    [ObservableProperty] private string _countryCode = "+33";
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private bool _isPhoneValid;

    partial void OnPhoneNumberChanged(string value)
        => IsPhoneValid = System.Text.RegularExpressions.Regex.IsMatch(value, @"^0\d{9}$");

    [RelayCommand]
    private void Next()
    {
        if (CurrentStep < 4) CurrentStep++;
    }

    [RelayCommand]
    private void Back()
    {
        if (CurrentStep > 1) CurrentStep--;
    }
}