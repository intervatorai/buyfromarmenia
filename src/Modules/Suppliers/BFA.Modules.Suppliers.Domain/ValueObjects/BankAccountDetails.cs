using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Suppliers.Domain.ValueObjects;

public sealed class BankAccountDetails : ValueObject
{
    public string BankName { get; }
    public string AccountHolder { get; }
    public string Iban { get; }
    public string? Swift { get; }
    public string Currency { get; }

    public BankAccountDetails(
        string bankName,
        string accountHolder,
        string iban,
        string currency,
        string? swift = null)
    {
        if (string.IsNullOrWhiteSpace(bankName))
        {
            throw new DomainException("Bank name is required.");
        }

        if (string.IsNullOrWhiteSpace(accountHolder))
        {
            throw new DomainException("Account holder is required.");
        }

        if (string.IsNullOrWhiteSpace(iban))
        {
            throw new DomainException("IBAN is required.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new DomainException("Currency must be a 3-letter ISO code.");
        }

        BankName = bankName.Trim();
        AccountHolder = accountHolder.Trim();
        Iban = iban.Trim().ToUpperInvariant();
        Swift = swift?.Trim().ToUpperInvariant();
        Currency = currency.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return BankName;
        yield return AccountHolder;
        yield return Iban;
        yield return Swift;
        yield return Currency;
    }
}
