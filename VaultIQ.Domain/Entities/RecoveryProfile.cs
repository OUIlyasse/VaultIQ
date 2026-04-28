using VaultIQ.Shared.Constants;

namespace VaultIQ.Domain.Entities;

/// <summary>
/// Profil de récupération de compte VaultIQ.
/// Toutes les données sensibles sont stockées sous forme de hash PBKDF2 — jamais en clair.
/// Supporte 4 méthodes : Téléphone / Email / Code PIN / Questions secrètes.
/// </summary>
public class RecoveryProfile
{
    public bool PhoneEnabled { get; private set; }
    public string PhoneNumberMasked { get; private set; } = string.Empty;
    public string PhoneNumberHash { get; private set; } = string.Empty;
    public string PhoneCountryCode { get; private set; } = string.Empty;

    public bool EmailEnabled { get; private set; }
    public string RecoveryEmailMasked { get; private set; } = string.Empty;
    public string RecoveryEmailHash { get; private set; } = string.Empty;

    public bool RecoveryPinEnabled { get; private set; }
    public string RecoveryPinHash { get; private set; } = string.Empty;
    public int RecoveryPinLength { get; private set; }
    public int FailedPinAttempts { get; private set; }
    public DateTime? PinLockedUntil { get; private set; }

    public bool SecurityQuestionsEnabled { get; private set; }
    private readonly List<SecurityQuestion> _questions = [];
    public IReadOnlyList<SecurityQuestion> Questions => _questions.AsReadOnly();

    public string EncryptedMasterKeyBackup { get; private set; } = string.Empty;
    public DateTime? LastRecoveryAttempt { get; private set; }
    public int TotalRecoveryAttempts { get; private set; }

    public bool IsConfigured =>
        PhoneEnabled || EmailEnabled || RecoveryPinEnabled || SecurityQuestionsEnabled;

    public bool IsPinLocked =>
        PinLockedUntil.HasValue && DateTime.UtcNow < PinLockedUntil;

    public int PinLockMinutesRemaining =>
        PinLockedUntil.HasValue
            ? Math.Max(0, (int)(PinLockedUntil.Value - DateTime.UtcNow).TotalMinutes + 1)
            : 0;

    public RecoveryStrength Strength
    {
        get
        {
            int count = (PhoneEnabled ? 1 : 0) + (EmailEnabled ? 1 : 0)
                      + (RecoveryPinEnabled ? 1 : 0) + (SecurityQuestionsEnabled ? 1 : 0);
            return count switch
            {
                0 => RecoveryStrength.NotConfigured,
                1 => RecoveryStrength.Low,
                2 => RecoveryStrength.Medium,
                3 => RecoveryStrength.Good,
                _ => RecoveryStrength.Excellent
            };
        }
    }

    public string StrengthLabel => Strength switch
    {
        RecoveryStrength.NotConfigured => "❌  Non configuré",
        RecoveryStrength.Low => "🔴  Faible (1 méthode)",
        RecoveryStrength.Medium => "🟡  Moyen (2 méthodes)",
        RecoveryStrength.Good => "🟢  Bon (3 méthodes)",
        RecoveryStrength.Excellent => "🟢  Excellent (4 méthodes)",
        _ => "?"
    };

    public void SetupPhone(string countryCode, string maskedNumber, string phoneHash)
    {
        PhoneCountryCode = countryCode; PhoneNumberMasked = maskedNumber;
        PhoneNumberHash = phoneHash; PhoneEnabled = true;
    }

    public void SetupEmail(string maskedEmail, string emailHash)
    {
        RecoveryEmailMasked = maskedEmail; RecoveryEmailHash = emailHash; EmailEnabled = true;
    }

    public void SetupPin(string pinHash, int pinLength)
    {
        if (pinLength < CryptoConstants.MinPinLength || pinLength > CryptoConstants.MaxPinLength)
            throw new ArgumentOutOfRangeException(nameof(pinLength));
        RecoveryPinHash = pinHash; RecoveryPinLength = pinLength;
        RecoveryPinEnabled = true; ResetPinAttempts();
    }

    public void SetupQuestions(IEnumerable<SecurityQuestion> questions)
    {
        var list = questions.ToList();
        if (list.Count < 2) throw new ArgumentException("At least 2 questions required.");
        _questions.Clear(); _questions.AddRange(list);
        SecurityQuestionsEnabled = true;
    }

    public void SetEncryptedMasterKey(string key) => EncryptedMasterKeyBackup = key;

    public void RecordFailedPinAttempt()
    {
        FailedPinAttempts++;
        if (FailedPinAttempts >= CryptoConstants.MaxPinAttempts)
        {
            PinLockedUntil = DateTime.UtcNow.AddMinutes(CryptoConstants.PinLockoutMinutes);
            FailedPinAttempts = 0;
        }
    }

    public void ResetPinAttempts()
    { FailedPinAttempts = 0; PinLockedUntil = null; }

    public void RecordSuccessfulRecovery()
    {
        ResetPinAttempts();
        LastRecoveryAttempt = DateTime.UtcNow;
        TotalRecoveryAttempts++;
    }

    public void DisablePhone()
    { PhoneEnabled = false; PhoneNumberHash = string.Empty; }

    public void DisableEmail()
    { EmailEnabled = false; RecoveryEmailHash = string.Empty; }

    public void DisablePin()
    { RecoveryPinEnabled = false; RecoveryPinHash = string.Empty; }

    public void DisableQuestions()
    { SecurityQuestionsEnabled = false; _questions.Clear(); }
}

public class SecurityQuestion
{
    public string QuestionText { get; private set; } = string.Empty;
    public string AnswerHash { get; private set; } = string.Empty;

    protected SecurityQuestion()
    { }

    public static SecurityQuestion Create(string questionText, string answerHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionText, nameof(questionText));
        ArgumentException.ThrowIfNullOrWhiteSpace(answerHash, nameof(answerHash));
        return new SecurityQuestion { QuestionText = questionText.Trim(), AnswerHash = answerHash };
    }
}

public enum RecoveryStrength
{ NotConfigured = 0, Low = 1, Medium = 2, Good = 3, Excellent = 4 }