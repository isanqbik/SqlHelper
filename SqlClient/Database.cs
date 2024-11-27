using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SqlClient.Domain;
using SqlClient.SeedWork;
using static Azure.Core.HttpHeader;

namespace SqlClient;

public class Database(string connectionString) : IDatabase
{
    private readonly SqlConnection connection = new(connectionString);

    public IEnumerable<MyNote> Read()
    {
        const string query = "SELECT Id, Note, Inserted AS Expiring FROM Notes";

        connection.Open();
        var note = connection.Query<MyNote>(query);

        /*
        using var command = new SqlCommand(query, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            yield return new(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDateTimeOffset(2)
            );
        }
        */
        
        connection.Close();
        return note;
    }

    public void Insert(string note)
    {
        const string query = "INSERT INTO Notes (Note, Inserted) VALUES (@Note, @Inserted)";

        connection.Open();
        var transaction = connection.BeginTransaction();
        try
        {
            
            connection.Execute(query, new { Id = Guid.NewGuid(), Note = note, Inserted = DateTimeOffset.Now }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
        }

        /*
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Note", note);
        command.Parameters.AddWithValue("@Inserted", DateTimeOffset.Now);
        command.ExecuteNonQuery();
        */
        
        connection.Close();
    }

    public void Update(int id, string note)
    {
        const string updateQuery = "UPDATE Notes SET Note = @Note WHERE Id = @Id";

        connection.Open();
        var transaction = connection.BeginTransaction();
        try {
            connection.Execute(updateQuery, new { Id = id, Note = note }, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
        } 

        /*
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Note", note);

        command.ExecuteNonQuery();
        */

        connection.Close();
    }

    public void Delete(int id)
    {
        const string deleteQuery = "DELETE FROM Notes WHERE Id = @Id";

        connection.Open();
        var transaction = connection.BeginTransaction();
        try
        {
            connection.Execute(deleteQuery, new { Id = id }, transaction);
            transaction.Commit();

        }
        catch
        {
            transaction.Rollback();
        }

        /*
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        command.ExecuteNonQuery();
        */
        
        connection.Close();
    }

    public void MassiveInsert(ICollection<string> notes)
    {
        const string query = "INSERT INTO Notes (Note, Inserted) VALUES (@Note, @Inserted)";

        connection.Open();
        var transaction = connection.BeginTransaction();
        
        try
        {
            foreach (var note in notes)
            {
                connection.Execute(query, new { Id = Guid.NewGuid(), Note = note, Inserted = DateTimeOffset.Now } , transaction);
            }
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
        }
       
        connection.Close();
        
    }

    public async ValueTask DisposeAsync() => await connection.DisposeAsync();

    public void Dispose() => connection.Dispose();
}