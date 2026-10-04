namespace ClaimsPlatform.Api.Access.Domain;

public class Team
{
    private Team()
    {
    }

    private Team(
        Guid id,
        string name,
        string market)
    {
        Id = id;
        Name = name;
        Market = market;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Market { get; private set; } = string.Empty;

    public Guid? ManagerId { get; private set; }

    public static Team Create(
        Guid id,
        string name,
        string market)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A team ID is required.",
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

        return new Team(
            id: id,
            name: name.Trim(),
            market: market.Trim().ToUpperInvariant());
    }

    public void AssignManager(Guid managerId)
    {
        if (managerId == Guid.Empty)
        {
            throw new ArgumentException(
                "A manager ID is required.",
                nameof(managerId));
        }

        ManagerId = managerId;
    }
}