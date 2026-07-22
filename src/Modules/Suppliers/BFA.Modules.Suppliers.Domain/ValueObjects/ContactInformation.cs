using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Suppliers.Domain.ValueObjects;

public sealed class ContactInformation : ValueObject
{
    public string ContactPerson { get; }
    public string Email { get; }
    public string Phone { get; }

    public ContactInformation(string contactPerson, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(contactPerson))
        {
            throw new DomainException("Contact person is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Contact email is required.");
        }

        ContactPerson = contactPerson.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ContactPerson;
        yield return Email;
        yield return Phone;
    }
}
