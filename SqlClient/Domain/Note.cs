namespace SqlClient.Domain;

public record MyNote(int Id, string Note, DateTimeOffset Expiring);