namespace ClaimsPlatform.Api.Access.Domain;

public class User
{
    private User()
    {
    }

    private User(
        Guid id,
        string name,
        UserRole role,
        string market,
        Guid? teamId)
    {
        Id = id;
        Name = name;
        Role = role;
        Market = market;
        TeamId = teamId;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public string Market { get; private set; } = string.Empty;

    public Guid? TeamId { get; private set; }

    public static User Create(
        Guid id,
        string name,
        UserRole role,
        string market,
        Guid? teamId = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A user ID is required.",
                nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(market);

        if (market.Trim().Length != 2)
        {
            throw new ArgumentException(
                "Market must be a two-character code.",
                nameof(market));
        }

        if (role == UserRole.Claimant && teamId is not null)
        {
            throw new ArgumentException(
                "A claimant cannot belong to a claims team.",
                nameof(teamId));
        }

        if (role != UserRole.Claimant && teamId is null)
        {
            throw new ArgumentException(
                "Claims officers and managers must belong to a team.",
                nameof(teamId));
        }

        return new User(
            id: id,
            name: name.Trim(),
            role: role,
            market: market.Trim().ToUpperInvariant(),
            teamId: teamId);
    }
}